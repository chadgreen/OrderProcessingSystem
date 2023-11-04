using Azure.Messaging.ServiceBus;
using BuildingBricks.Core.EventMessages;
using BuildingBricks.Inventory;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Inventory.Functions;

public class OrderPlacedMonitor
{

	private readonly ILogger<OrderPlacedMonitor> _logger;
	private readonly InventoryServices _inventoryServices;

	public OrderPlacedMonitor(
		ILogger<OrderPlacedMonitor> logger,
		InventoryServices inventoryServices)
	{
		_logger = logger;
		_inventoryServices = inventoryServices;
	}

	[Function("Inventory-OrderPlacedMonitor")]
	public async Task RunAsync([ServiceBusTrigger("%OrderPlacedQueue%", Connection = "ServiceBusConnectionString", IsSessionsEnabled = true)] ServiceBusReceivedMessage message)
	{
		_logger.LogInformation("Message ID: {id}", message.MessageId);
		ProductPurchasedMessage productPurchasedMessage = message.Body.ToObjectFromJson<ProductPurchasedMessage>();
		await _inventoryServices.ReserveItemForOrderAsync(productPurchasedMessage);
	}

}