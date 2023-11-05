using Azure.Messaging.EventHubs;
using BuildingBricks.EventMessages;
using BuildingBricks.Shipping;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shipping.Functions;

public class OrderPlacedMonitor
{

	private readonly ILogger<OrderPlacedMonitor> _logger;
	private readonly ShippingServices _shippingServices;

	public OrderPlacedMonitor(
		ILogger<OrderPlacedMonitor> logger,
		ShippingServices shippingServices)
	{
		_logger = logger;
		_shippingServices = shippingServices;
	}

	[Function(nameof(OrderPlacedMonitor))]
	public async Task RunAsync([EventHubTrigger("%OrderPlacedEventHub%", Connection = "OrderPlacedConnectionString", ConsumerGroup = "%OrderPlacedConsumerGroup%")] EventData[] eventMessages)
	{
		foreach (EventData eventMessage in eventMessages)
		{
			OrderPlacedMessage? orderPlacedMessage = JsonSerializer.Deserialize<OrderPlacedMessage>(eventMessage.EventBody);
			if (orderPlacedMessage is not null)
			{
				_logger.LogInformation("Initializing shipment for order #{OrderNumber}", orderPlacedMessage.PurchaseId);
				await _shippingServices.InitializeShipmentAsync(orderPlacedMessage);
			}
		}
	}

}