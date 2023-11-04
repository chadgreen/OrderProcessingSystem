using BuildingBricks.Core.EventMessages;
using BuildingBricks.EventMessages;
using BuildingBricks.Inventory.Constants;
using BuildingBricks.Inventory.Models;
using System.Text.Json;

namespace BuildingBricks.Inventory;

public class InventoryServices : ServicesBase
{

	public InventoryServices(ConfigServices configServices) : base(configServices) { }

	public async Task ReserveItemForOrderAsync(ProductPurchasedMessage productPurchasedMessage)
	{
		using InventoryContext inventoryContext = new(_configServices);
		Product? product = await inventoryContext.Products.FirstOrDefaultAsync(x => x.ProductId == productPurchasedMessage.ProductId);
		if (product is not null)
		{

			// Update the inventory status
			InventoryTransaction inventoryTransaction = new()
			{
				ProductId = product.ProductId,
				InventoryActionId = InventoryActions.ReservedForOrder,
				InventoryReserve = productPurchasedMessage.Quantity,
				OrderNumber = productPurchasedMessage.PurchaseId
			};
			await inventoryContext.InventoryTransactions.AddAsync(inventoryTransaction);
			await inventoryContext.SaveChangesAsync();

			// Get the inventory status
			InventoryUpdatedMessage? inventoryStatusResponse = await GetInventoryStatusAsync(product.ProductId, inventoryContext);

			// Send the inventory reserved message
			InventoryReservedMessage inventoryReservedMessage = new()
			{
				CustomerId = productPurchasedMessage.CustomerId,
				OrderId = productPurchasedMessage.PurchaseId,
				ProductId = productPurchasedMessage.ProductId,
				ProductName = product.ProductName,
				QuantityOnHand = inventoryStatusResponse?.InventoryOnHand - inventoryStatusResponse?.InventoryReserved ?? 0,
				Backordered = inventoryStatusResponse is null || inventoryStatusResponse.InventoryAvailable < 1
			};

			await SendMessageToEventHubAsync(
				_configServices.InventoryEventHubsInventoryReservedConnectionString,
				_configServices.InventoryEventHubsInventoryReservedEventHubName,
				JsonSerializer.Serialize(inventoryReservedMessage));

		}

	}

	public async Task<InventoryUpdatedMessage?> GetInventoryStatusAsync(string productId)
	{
		using InventoryContext inventoryContext = new(_configServices);
		return await GetInventoryStatusAsync(productId, inventoryContext);
	}

	public async Task InventoryUpdatedAsync(string productId)
	{
		InventoryUpdatedMessage? inventoryUpdatedMessage = await GetInventoryStatusAsync(productId);
		if (inventoryUpdatedMessage is not null)
			await SendMessageToEventHubAsync(
				_configServices.InventoryEventHubsInventoryUpdatedConnectionString,
				_configServices.InventoryEventHubsInventoryUpdatedEventHubName,
				JsonSerializer.Serialize(inventoryUpdatedMessage));
	}
	private static async Task<InventoryUpdatedMessage?> GetInventoryStatusAsync(string productId, InventoryContext inventoryContext)
	{
		List<InventoryTransaction> inventoryTransactions = await inventoryContext.InventoryTransactions.Where(x => x.ProductId == productId).ToListAsync();
		if (inventoryTransactions.Any())
			return new InventoryUpdatedMessage()
			{
				ProductId = productId,
				InventoryOnHand = inventoryTransactions.Sum(x => x.InventoryCredit) - inventoryTransactions.Sum(x => x.InventoryDebit),
				InventoryReserved = inventoryTransactions.Sum(x => x.InventoryReserve),
				InventoryAvailable = inventoryTransactions.Sum(x => x.InventoryCredit) - (inventoryTransactions.Sum(x => x.InventoryDebit) + inventoryTransactions.Sum(x => x.InventoryReserve)),
				LastUpdate = inventoryTransactions.Max(x => x.ActionDateTime)
			};
		else
			return null;
	}

}