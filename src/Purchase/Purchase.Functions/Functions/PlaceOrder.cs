using BuildingBricks.Purchase.Requests;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaleLearnCode;

namespace BuildingBricks.Purchase.Functions;

public class PlaceOrder
{

	private readonly ILogger _logger;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly PurchaseServices _purchaseServices;

	public PlaceOrder(
		ILoggerFactory loggerFactory,
		JsonSerializerOptions jsonSerializerOptions,
		PurchaseServices purchaseServices1)
	{
		_logger = loggerFactory.CreateLogger<PlaceOrder>();
		_jsonSerializerOptions = jsonSerializerOptions;
		_purchaseServices = purchaseServices1;
	}

	[Function("PlaceOrder")]
	public async Task<HttpResponseData> RunAsync(
		[HttpTrigger(AuthorizationLevel.Function, "post", Route = "purchases")] HttpRequestData request)
	{
		try
		{
			PlaceOrderRequest placeOrderRequest = await request.GetRequestParametersAsync<PlaceOrderRequest>(_jsonSerializerOptions);
			string orderId = await _purchaseServices.PlaceOrderAsync(placeOrderRequest);
			return request.CreateCreatedResponse($"purchases/{orderId}");
		}
		catch (Exception ex) when (ex is ArgumentNullException)
		{
			return request.CreateBadRequestResponse(ex);
		}
		catch (Exception ex)
		{
			_logger.LogError("Unexpected exception: {ExceptionMessage}", ex.Message);
			return request.CreateErrorResponse(ex);
		}
	}

}