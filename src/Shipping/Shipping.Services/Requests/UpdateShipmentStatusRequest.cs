namespace BuildingBricks.Shipping.Requests;

public class UpdateShipmentStatusRequest
{
	public int ShipmentStatusId { get; set; }
	public int? CarrierId { get; set; }
	public string? TrackingNumber { get; set; }
}