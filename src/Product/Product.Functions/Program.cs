using BuildingBricks.Core;
using BuildingBricks.Product.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")!;
string appConfigEndpoint = Environment.GetEnvironmentVariable("AppConfigEndpoint")!;
ConfigServices configServices = new(appConfigEndpoint, environment);

using CosmosClient cosmosClient = new(configServices.CosmosUri, configServices.CosmosKey);
Database database = cosmosClient.GetDatabase(configServices.ProductCosmosDatabaseId);

MerchandiseServices merchandiseServices = new(database.GetContainer(configServices.ProductMerchandiseContainerId));
MerchandiseByAvailabilityServices merchandiseByAvailabilityServices = new(configServices, database.GetContainer(configServices.ProductsByAvailabilityContainerId));
MerchandiseByThemeServices merchandiseByThemeServices = new(configServices, database.GetContainer(configServices.ProductsByThemeContainerId));
AvailabilityServices availabilityServices = new(configServices, database.GetContainer(configServices.ProductMetadataContainerId));

IHost host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(s =>
	{
		s.AddSingleton((s) => { return configServices; });
		s.AddSingleton((s) => { return merchandiseServices; });
		s.AddSingleton((s) => { return merchandiseByAvailabilityServices; });
		s.AddSingleton((s) => { return merchandiseByThemeServices; });
		s.AddSingleton((s) => { return availabilityServices; });
	})
	.Build();

host.Run();