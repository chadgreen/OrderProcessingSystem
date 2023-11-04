using Azure.Messaging.EventHubs;
using BuildingBricks.EventMessages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBricks.Notice.Functions;

public class InventoryReservedMonitor
{

	private readonly ILogger<InventoryReservedMonitor> _logger;
	private readonly NoticeServices _noticeServices;

	public InventoryReservedMonitor(
		ILogger<InventoryReservedMonitor> logger,
		NoticeServices noticeServices)
	{
		_logger = logger;
		_noticeServices = noticeServices;
	}

	[Function("Notice-InventoryReservedMonitor")]
	public async Task RunAsync([EventHubTrigger("%InventoryReservedEventHub%", Connection = "InventoryReservedConnectionString", ConsumerGroup = "%InventoryReservedConsumerGroup%")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			_logger.LogInformation("Inventory Reserved Event Received");
			InventoryReservedMessage? inventoryReservedMessage = JsonSerializer.Deserialize<InventoryReservedMessage>(eventMessage.EventBody);
			if (inventoryReservedMessage is not null && inventoryReservedMessage.Backordered)
			{
				_logger.LogWarning("Inventory Reserved Event Received for Backordered Purchase {PurchaseId}", inventoryReservedMessage.ProductId);
				await _noticeServices.SendBackorderNoticeAsync(inventoryReservedMessage);
			}
		}
	}

}