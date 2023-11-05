namespace BuildingBricks.Core.EventMessages;

public class OrderShippedMessage
{
	public string OrderId { get; set; } = null!;
	public string Carrier { get; set; } = null!;
	public string TrackingNumber { get; set; } = null!;
}