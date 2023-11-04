using BuildingBricks.Inventory;
using BuildingBricks.Inventory.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;

namespace Inventory.Functions;

public class InventoryUpdated
{

	private readonly ILogger _logger;
	private readonly InventoryServices _inventoryServices;

	public InventoryUpdated(ILoggerFactory loggerFactory, InventoryServices inventoryServices)
	{
		_logger = loggerFactory.CreateLogger<InventoryUpdated>();
		_inventoryServices = inventoryServices;
	}

	[Function("InventoryUpdated")]
	public async Task RunAsync(
			[SqlTrigger("[Inventory].[InventoryTransaction]", "SqlConnectionString")] IReadOnlyList<SqlChange<InventoryTransaction>> changes,
			FunctionContext context)
	{
		foreach (SqlChange<InventoryTransaction> change in changes)
		{
			_logger.LogInformation("InventoryUpdated: {ProductId}", change.Item.ProductId);
			await _inventoryServices.InventoryUpdatedAsync(change.Item.ProductId);
		}
	}

}