using Azure.Messaging.EventHubs;
using BuildingBricks.EventMessages;
using BuildingBricks.Purchase.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBricks.Purchase.Functions;

public class InventoryReservedMonitor
{

	private readonly ILogger<InventoryReservedMonitor> _logger;
	private readonly PurchaseServices _purchaseServices;

	public InventoryReservedMonitor(
		ILogger<InventoryReservedMonitor> logger,
		PurchaseServices purchaseServices)
	{
		_logger = logger;
		_purchaseServices = purchaseServices;
	}

	[Function(nameof(InventoryReservedMonitor))]
	public async Task RunAsync([EventHubTrigger("%InventoryReservedEventHub%", Connection = "InventoryReservedConnectionString", ConsumerGroup = "%InventoryReservedConsumerGroup%")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			InventoryReservedMessage? inventoryReservedMessage = JsonSerializer.Deserialize<InventoryReservedMessage>(eventMessage.EventBody);
			if (inventoryReservedMessage is not null)
			{
				_logger.LogInformation("Inventory Reserved Event Received");
				await _purchaseServices.UpdatePurchaseItemStatusAsync(inventoryReservedMessage.OrderId, inventoryReservedMessage.ProductId, PurchaseStatuses.Reserved);
			}
		}
	}

}