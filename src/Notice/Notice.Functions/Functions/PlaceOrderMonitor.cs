using Azure.Messaging.EventHubs;
using BuildingBricks.EventMessages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBricks.Notice.Functions;

public class PlaceOrderMonitor
{

	private readonly ILogger _logger;
	private readonly NoticeServices _noticeServices;

	public PlaceOrderMonitor(
		ILoggerFactory loggerFactory,
		NoticeServices noticeServices)
	{
		_logger = loggerFactory.CreateLogger<PlaceOrderMonitor>();
		_noticeServices = noticeServices;
	}

	[Function("Notice-PlaceOrderMonitor")]
	public async Task RunAsync([EventHubTrigger("%PlaceOrderEventHub%", Connection = "PlaceOrderConnectionString")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			OrderPlacedMessage? orderPlacedMessage = JsonSerializer.Deserialize<OrderPlacedMessage>(eventMessage.EventBody);
			if (orderPlacedMessage is not null)
			{
				_logger.LogInformation("Sending Confirmation Email for Purchase {PurchaseId}", orderPlacedMessage.PurchaseId);
				await _noticeServices.SendOrderConfirmationAsync(orderPlacedMessage);
			}
		}
	}

}