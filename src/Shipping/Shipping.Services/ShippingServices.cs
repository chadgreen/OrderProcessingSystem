using BuildingBricks.Core.EventMessages;
using BuildingBricks.EventMessages;
using BuildingBricks.Shipping.Constants;
using BuildingBricks.Shipping.Models;
using BuildingBricks.Shipping.Requests;

namespace BuildingBricks.Shipping;

public class ShippingServices : ServicesBase
{

	public ShippingServices(ConfigServices configServices) : base(configServices) { }

	public async Task<int> InitializeShipmentAsync(OrderPlacedMessage orderPlacedMessage)
	{
		using ShippingContext shippingContext = new(_configServices);
		CustomerPurchase customerPurchase = await CreateCustomerPurchaseAsync(orderPlacedMessage, shippingContext);
		return await CreateShipmentAsync(customerPurchase, shippingContext);
	}

	public async Task UpdateShipmentStatusAsync(
		int shipmentId,
		UpdateShipmentStatusRequest updateShipmentStatusRequest)
	{

		using ShippingContext shippingContext = new(_configServices);

		// Retrieve the shipment to be updated
		Shipment? shipment = await shippingContext.Shipments.FirstOrDefaultAsync(x => x.ShipmentId == shipmentId)
			?? throw new ArgumentNullException(nameof(shipmentId));

		// Validate the specified shipment status
		ShipmentStatus? shipmentStatus = await shippingContext.ShipmentStatuses.FirstOrDefaultAsync(x => x.ShipmentStatusId == updateShipmentStatusRequest.ShipmentStatusId)
			?? throw new ArgumentOutOfRangeException(nameof(updateShipmentStatusRequest), "Invalid shipping status specified.");

		// Validate the specified shipping carrier
		ShippingCarrier? carrier = null;
		if (updateShipmentStatusRequest.CarrierId is not null)
		{
			carrier = await shippingContext.ShippingCarriers.FirstOrDefaultAsync(x => x.ShippingCarrierId == updateShipmentStatusRequest.CarrierId)
				?? throw new ArgumentOutOfRangeException(nameof(updateShipmentStatusRequest), "Invalid shipping carrier specified.");
		}

		// Update the shipment record
		shipment.ShipmentStatusId = shipmentStatus.ShipmentStatusId;
		shipment.ShippingCarrierId = carrier?.ShippingCarrierId ?? null;
		shipment.TrackingNumber = updateShipmentStatusRequest.TrackingNumber;
		await shippingContext.SaveChangesAsync();

	}

	public async Task StartPickingOrderAsync(string orderId)
	{
		using ShippingContext shippingContext = new(_configServices);
		CustomerPurchase? customerPurchase = await shippingContext.CustomerPurchases
			.Include(x => x.Shipments)
			.FirstOrDefaultAsync(x => x.CustomerPurchaseId == orderId);
		if (customerPurchase is not null && customerPurchase.Shipments.Any())
		{
			foreach (Shipment shipment in customerPurchase.Shipments)
				shipment.ShipmentStatusId = ShipmentStatuses.Picking;
			await shippingContext.SaveChangesAsync();
		}
	}

	private static async Task<CustomerPurchase> CreateCustomerPurchaseAsync(OrderPlacedMessage orderPlacedMessage, ShippingContext shippingContext)
	{

		CustomerPurchase customerPurchase = new()
		{
			CustomerPurchaseId = orderPlacedMessage.PurchaseId,
			CustomerId = orderPlacedMessage.CustomerId
		};
		await shippingContext.CustomerPurchases.AddAsync(customerPurchase);
		await shippingContext.SaveChangesAsync();

		foreach (ProductPurchasedMessage item in orderPlacedMessage.Items)
			customerPurchase.OrderItems.Add(new()
			{
				OrderItemId = item.PurchaseItemId,
				CustomerOrderId = item.PurchaseId,
				ProductId = item.ProductId,
				Quantity = item.Quantity
			});
		await shippingContext.SaveChangesAsync();

		return customerPurchase;

	}

	private static async Task<int> CreateShipmentAsync(CustomerPurchase customerPurchase, ShippingContext shippingContext)
	{
		customerPurchase.Shipments.Add(new()
		{
			ShipmentStatusId = ShipmentStatuses.Inventory
		});
		await shippingContext.SaveChangesAsync();
		return customerPurchase.Shipments.First().ShipmentId;
	}

}