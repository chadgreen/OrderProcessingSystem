using BuildingBricks.Product.Models;
using BuildingBricks.Product.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuildingBricks.Product.Functions;

public class MetadataChangeMonitor
{

	private readonly ILogger _logger;
	private readonly MerchandiseServices _merchandiseServices;

	public MetadataChangeMonitor(
		ILoggerFactory loggerFactory,
		MerchandiseServices merchandiseServices)
	{
		_logger = loggerFactory.CreateLogger<MetadataChangeMonitor>();
		_merchandiseServices = merchandiseServices;
	}

	[Function("MetadataChangeMonitor")]
	public async Task RunAsync([CosmosDBTrigger(
		databaseName: "products",
		containerName: "metadata",
		Connection = "ConnectionString",
		LeaseContainerName = "metadataLeases",
		CreateLeaseContainerIfNotExists = true)] IReadOnlyList<IMetadata> changedMetadata)
	{
		if (changedMetadata != null && changedMetadata.Count > 0)
		{
			foreach (IMetadata changedMetadataItem in changedMetadata)
			{
				if (changedMetadataItem.MetadataType == Constants.MetadataType.Availability)
				{
					Availability changedAvailability = (Availability)changedMetadataItem;
					_logger.LogInformation("Updated Availability: {availabilityId} - {availabilityName}", changedAvailability.Id, changedAvailability.Name);
					List<Merchandise> merchandiseToUpdate = await _merchandiseServices.GetListByAvailabilityAsync(changedAvailability.Id);
					foreach (Merchandise merchandise in merchandiseToUpdate)
					{
						if (merchandise.Availability != changedAvailability.Name)
						{
							_logger.LogInformation("Updated Merchandise (Availability): {productId} - {productName}", merchandise.Id, merchandise.Name);
							merchandise.Availability = changedAvailability.Name;
							await _merchandiseServices.UpsertAsync(merchandise);
						}
					}
				}
				else if (changedMetadataItem.MetadataType == Constants.MetadataType.Theme)
				{
					Theme changedTheme = (Theme)changedMetadataItem;
					_logger.LogInformation("Updated Theme: {themeId} - {themeName}", changedTheme.Id, changedTheme.Name);
					foreach (ThemeMerchandise themeMerchandise in changedTheme.Merchandises)
					{
						Merchandise merchandise = await _merchandiseServices.GetAsync(themeMerchandise.ItemNumber);
						if (merchandise.ThemeName != changedTheme.Name)
						{
							merchandise.ThemeName = changedTheme.Name;
							await _merchandiseServices.UpsertAsync(merchandise);
							_logger.LogInformation("Updated Merchandise (Theme): {productId} - {productName}", merchandise.Id, merchandise.Name);
						}
					}
				}
				else
				{
					_logger.LogError("MetadataType {metadataType} is not supported.", changedMetadataItem.MetadataType);
				}
			}
		}
	}

}