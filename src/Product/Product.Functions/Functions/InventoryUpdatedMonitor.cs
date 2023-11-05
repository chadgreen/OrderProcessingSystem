using Azure.Messaging.EventHubs;
using BuildingBricks.Core.EventMessages;
using BuildingBricks.Product.Models;
using BuildingBricks.Product.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBricks.Product.Functions;

public class InventoryUpdatedMonitor
{

	private readonly ILogger<InventoryUpdatedMonitor> _logger;
	private readonly MerchandiseServices _merchandiseServices;
	private readonly AvailabilityServices _availabilityServices;

	private const int _availableNow = 1;
	private const int _backordered = 2;
	private const int _comingSoon = 3;
	private const int _outOfStock = 4;

	public InventoryUpdatedMonitor(
		ILogger<InventoryUpdatedMonitor> logger,
		MerchandiseServices merchandiseServices,
		AvailabilityServices availabilityServices)
	{
		_logger = logger;
		_merchandiseServices = merchandiseServices;
		_availabilityServices = availabilityServices;
	}

	[Function(nameof(InventoryUpdatedMonitor))]
	public async Task Run([EventHubTrigger("%InventoryUpdatedEventHub%", Connection = "InventoryUpdatedConnectionString", ConsumerGroup = "%InventoryUpdatedConsumerGroup%")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			InventoryUpdatedMessage? inventoryUpdatedMessage = JsonSerializer.Deserialize<InventoryUpdatedMessage>(eventMessage.EventBody);
			if (inventoryUpdatedMessage is not null)
			{
				_logger.LogInformation("Inventory Updated - ProductId: {ProductId} - Available: {Availability}", inventoryUpdatedMessage.ProductId, inventoryUpdatedMessage.InventoryAvailable);
				List<Availability> availabilities = await _availabilityServices.GetListAsync();
				Merchandise merchandise = await _merchandiseServices.GetAsync(inventoryUpdatedMessage.ProductId);
				if (inventoryUpdatedMessage.InventoryAvailable > 0)
				{
					Availability? availableNow = availabilities.FirstOrDefault(x => x.LegacyId == _availableNow);
					if (availableNow is not null && merchandise.AvailabilityId != availableNow.Id)
					{
						_logger.LogWarning("{ProductId} is now available", inventoryUpdatedMessage.ProductId);
						await UpdateMerchandiseAvailability(merchandise, availableNow);
					}
				}
				else if (inventoryUpdatedMessage.InventoryAvailable >= -10)
				{
					Availability? backordered = availabilities.FirstOrDefault(x => x.LegacyId == _backordered);
					if (backordered is not null && merchandise.AvailabilityId != backordered.Id)
					{
						_logger.LogWarning("{ProductId} is now backordered", inventoryUpdatedMessage.ProductId);
						await UpdateMerchandiseAvailability(merchandise, backordered);
					}
				}
				else if (inventoryUpdatedMessage.InventoryAvailable < -10)
				{
					Availability? outOfStock = availabilities.FirstOrDefault(x => x.LegacyId == _outOfStock);
					if (outOfStock is not null && merchandise.AvailabilityId != outOfStock.Id)
					{
						_logger.LogWarning("{ProductId} is now out of stock", inventoryUpdatedMessage.ProductId);
						await UpdateMerchandiseAvailability(merchandise, outOfStock);
					}
				}
				else
				{
					_logger.LogWarning("{ProductId} has no change in availability status", inventoryUpdatedMessage.ProductId);
				}
			}
		}
	}

	private async Task UpdateMerchandiseAvailability(Merchandise merchandise, Availability availability)
	{
		merchandise.AvailabilityId = availability.Id;
		merchandise.Availability = availability.Name;
		await _merchandiseServices.ReplaceAsync(merchandise);
	}

}