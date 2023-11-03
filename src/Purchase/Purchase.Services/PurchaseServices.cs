using BuildingBricks.Core.EventMessages;
using BuildingBricks.EventMessages;
using BuildingBricks.Purchase.Models;
using BuildingBricks.Purchase.Requests;
using System.Text.Json;

namespace BuildingBricks.Purchase;

public class PurchaseServices : ServicesBase
{

	public PurchaseServices(ConfigServices configServices) : base(configServices) { }

	public async Task<string> PlaceOrderAsync(PlaceOrderRequest placeOrderRequest)
	{

		// Create the purchase database record
		CustomerPurchase customerPurchase = await CreatePurchaseRecordAsync(placeOrderRequest);

		// Send the order placed message to the event hub
		OrderPlacedMessage orderPlacedMessage = BuildOrderPlacedMessage(customerPurchase);
		await SendMessageToEventHubAsync(
			_configServices.PurchasePlaceOrderEventHubConnectionString,
			JsonSerializer.Serialize(orderPlacedMessage));

		// Send the product purchased message to the service bus
		await SendSessionMessageBatchToServiceBusAsync(
			_configServices.PurchaseServiceBusPlaceOrderConnectionString,
			_configServices.PurchaseServiceBusPlaceOrderServiceBusQueueName,
			customerPurchase.CustomerPurchaseId,
			BuildSerializedProductPurchaseMessageList(orderPlacedMessage));

		return customerPurchase.CustomerPurchaseId;

	}

	private async Task<CustomerPurchase> CreatePurchaseRecordAsync(PlaceOrderRequest placeOrderRequest)
	{
		string purchaseId = Guid.NewGuid().ToString();
		using PurchaseContext purchaseContext = new(_configServices);
		CustomerPurchase customerPurchase = new()
		{
			CustomerPurchaseId = purchaseId,
			CustomerId = placeOrderRequest.CustomerId,
			PurchaseLineItems = BuildPurchaseItemsList(placeOrderRequest, purchaseId)
		};
		await purchaseContext.CustomerPurchases.AddAsync(customerPurchase);
		await purchaseContext.SaveChangesAsync();
		return customerPurchase;
	}

	private static List<PurchaseLineItem> BuildPurchaseItemsList(PlaceOrderRequest placeOrderRequest, string purchaseId)
	{
		List<PurchaseLineItem> purchaseLineItems = new();
		foreach (PlaceOrderItem item in placeOrderRequest.Items)
		{
			purchaseLineItems.Add(new PurchaseLineItem
			{
				CustomerPurchaseId = purchaseId,
				ProductId = item.ProductId,
				Quantity = item.Quantity
			});
		}
		return purchaseLineItems;
	}

	private static OrderPlacedMessage BuildOrderPlacedMessage(CustomerPurchase customerPurchase)
	{

		OrderPlacedMessage orderPlacedMessage = new()
		{
			PurchaseId = customerPurchase.CustomerPurchaseId,
			CustomerId = customerPurchase.CustomerId,
			Items = new List<ProductPurchasedMessage>()
		};

		foreach (PurchaseLineItem? purchaseLineItem in customerPurchase.PurchaseLineItems)
			if (purchaseLineItem is not null)
				orderPlacedMessage.Items.Add(new()
				{
					CustomerId = customerPurchase.CustomerId,
					PurchaseId = customerPurchase.CustomerPurchaseId,
					PurchaseItemId = purchaseLineItem.PurchaseLineItemId,
					ProductId = purchaseLineItem.ProductId,
					Quantity = purchaseLineItem.Quantity
				});

		return orderPlacedMessage;

	}

	private static List<string> BuildSerializedProductPurchaseMessageList(OrderPlacedMessage orderPlacedMessage)
	{
		List<string> response = new();
		foreach (ProductPurchasedMessage? productPurchased in orderPlacedMessage.Items)
			if (productPurchased is not null)
				response.Add(JsonSerializer.Serialize<ProductPurchasedMessage>(productPurchased));
		return response;
	}


}