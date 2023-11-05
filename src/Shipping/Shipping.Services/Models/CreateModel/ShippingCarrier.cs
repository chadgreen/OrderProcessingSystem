namespace BuildingBricks.Shipping.Models;

internal static partial class CreateModel
{

	internal static void ShippingCarrier(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ShippingCarrier>(entity =>
		{
			entity.HasKey(e => e.ShippingCarrierId).HasName("pkcShippingCarrier");

			entity.ToTable("ShippingCarrier", "Shipping", tb => tb.HasComment("Represents a shipping carrier."));

			entity.Property(e => e.ShippingCarrierId)
				.ValueGeneratedNever();

			entity.Property(e => e.ShippingCarrierName)
				.IsRequired()
				.HasMaxLength(100)
				.IsUnicode(false);
		});
	}

}