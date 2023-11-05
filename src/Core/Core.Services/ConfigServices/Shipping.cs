// Ignore Spelling: Sql

namespace BuildingBricks.Core;

public partial class ConfigServices
{

	private const string _Shipping_AzureSqlCatalog = "Shipping:AzureSql:Catalog";
	private const string _Shipping_OrderShippedEventHubConnectionString = "Shipping:EventHubs:OrderShipped:ConnectionString";


	public string ShippingAzureSqlCatalog => GetConfigValue(_Shipping_AzureSqlCatalog);

	public string ShippingOrderShippedEventHubConnectionString => GetConfigValue(_Shipping_OrderShippedEventHubConnectionString);

}