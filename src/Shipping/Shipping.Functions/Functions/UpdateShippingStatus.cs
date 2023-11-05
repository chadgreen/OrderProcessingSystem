using BuildingBricks.Shipping;
using BuildingBricks.Shipping.Requests;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaleLearnCode;

namespace Shipping.Functions;

public class UpdateShippingStatus
{

	private readonly ILogger _logger;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ShippingServices _shippingServices;

	public UpdateShippingStatus(
		ILoggerFactory loggerFactory,
		JsonSerializerOptions jsonSerializerOptions,
		ShippingServices shippingServices)
	{
		_logger = loggerFactory.CreateLogger<UpdateShippingStatus>();
		_jsonSerializerOptions = jsonSerializerOptions;
		_shippingServices = shippingServices;
	}

	[Function("UpdateShippingStatus")]
	public async Task<HttpResponseData> RunAsync(
		[HttpTrigger(AuthorizationLevel.Function, "put", Route = "shipments/{shipmentId}")] HttpRequestData request,
		int shipmentId)
	{
		try
		{
			UpdateShipmentStatusRequest updateShipmentStatusRequest = await request.GetRequestParametersAsync<UpdateShipmentStatusRequest>(_jsonSerializerOptions);
			await _shippingServices.UpdateShipmentStatusAsync(shipmentId, updateShipmentStatusRequest);
			return request.CreateResponse(HttpStatusCode.NoContent);
		}
		catch (Exception ex) when (ex is ArgumentNullException)
		{
			return request.CreateResponse(HttpStatusCode.NotFound);
		}
		catch (Exception ex) when (ex is ArgumentOutOfRangeException)
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