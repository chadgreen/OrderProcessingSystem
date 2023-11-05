using Azure.Messaging.EventHubs;
using BuildingBricks.EventMessages;
using BuildingBricks.Shipping;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shipping.Functions;

public class InventoryReservedMonitor
{
	private readonly ILogger<InventoryReservedMonitor> _logger;
	private readonly ShippingServices _shippingServices;

	public InventoryReservedMonitor(
		ILogger<InventoryReservedMonitor> logger,
		ShippingServices shippingServices)
	{
		_logger = logger;
		_shippingServices = shippingServices;
	}

	[Function(nameof(InventoryReservedMonitor))]
	public async Task RunAsync([EventHubTrigger("%InventoryReservedEventHub%", Connection = "InventoryReservedConnectionString", ConsumerGroup = "%InventoryReservedConsumerGroup%")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			InventoryReservedMessage? inventoryReservedMessage = JsonSerializer.Deserialize<InventoryReservedMessage>(eventMessage.EventBody);
			if (inventoryReservedMessage is not null)
			{
				_logger.LogInformation("Inventory reserved for Order #{OrderNumber}", inventoryReservedMessage.OrderId);
				await _shippingServices.StartPickingOrderAsync(inventoryReservedMessage.OrderId);
			}
		}
	}

}