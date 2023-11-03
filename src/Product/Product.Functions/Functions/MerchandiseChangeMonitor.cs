using BuildingBricks.Product.Models;
using BuildingBricks.Product.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuildingBricks.Product.Functions;

public class MerchandiseChangeMonitor
{

	private readonly ILogger _logger;
	private readonly MerchandiseByAvailabilityServices _merchandiseByAvailabilityServices;
	private readonly MerchandiseByThemeServices _merchandiseByThemeServices;

	public MerchandiseChangeMonitor(
		ILoggerFactory loggerFactory,
		MerchandiseByAvailabilityServices merchandiseByAvailabilityServices,
		MerchandiseByThemeServices merchandiseByThemeServices)
	{
		_logger = loggerFactory.CreateLogger<MerchandiseChangeMonitor>();
		_merchandiseByAvailabilityServices = merchandiseByAvailabilityServices;
		_merchandiseByThemeServices = merchandiseByThemeServices;
	}

	[Function("MerchandiseChangeMonitor")]
	public async Task RunAsync([CosmosDBTrigger(
		databaseName: "products",
		containerName: "merchandise",
		Connection = "ConnectionString",
		LeaseContainerName = "merchandiseLeases",
		CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Merchandise> changedMerchandises)
	{
		if (changedMerchandises != null && changedMerchandises.Count > 0)
		{
			foreach (Merchandise changedMerchandise in changedMerchandises)
			{
				if (changedMerchandise.TTL > 0)
				{
					_logger.LogInformation("Deleted Merchandise: {productId} - {productName}", changedMerchandise.Id, changedMerchandise.Name);
					await _merchandiseByAvailabilityServices.DeleteAsync(changedMerchandise);
					await _merchandiseByThemeServices.DeleteAsync(changedMerchandise);
				}
				else
				{
					_logger.LogInformation("Updated Merchandise: {productId} - {productName}", changedMerchandise.Id, changedMerchandise.Name);
					await _merchandiseByAvailabilityServices.UpsertAsync(changedMerchandise);
					await _merchandiseByThemeServices.UpsertAsync(changedMerchandise);
				}
			}
		}
	}

}