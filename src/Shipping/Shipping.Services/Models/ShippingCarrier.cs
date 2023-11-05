#nullable disable

namespace BuildingBricks.Shipping.Models;

public partial class ShippingCarrier
{

	public int ShippingCarrierId { get; set; }

	public int ShippingCarrierName { get; set; }

	public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

}