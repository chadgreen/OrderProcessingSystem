CREATE TABLE Shipping.ShippingCarrier
(
  ShippingCarrierId   INT           NOT NULL IDENTITY(1,1),
  ShippingCarrierName NVARCHAR(100) NOT NULL,
  CONSTRAINT pkcShipppingCarrier PRIMARY KEY CLUSTERED (ShippingCarrierId)
)
GO

EXEC sp_addextendedproperty @level0name=N'Shipping', @level1name=N'ShippingCarrier',                                     @value=N'Represents a shipping carrier.',                                               @name=N'MS_Description', @level0type=N'SCHEMA', @level1type=N'TABLE';
GO
EXEC sp_addextendedproperty @level0name=N'Shipping', @level1name=N'ShippingCarrier', @level2name=N'ShippingCarrierId',   @value=N'Identifier for the shipping carrier.',                                         @name=N'MS_Description', @level0type=N'SCHEMA', @level1type=N'TABLE', @level2type=N'COLUMN';
GO
EXEC sp_addextendedproperty @level0name=N'Shipping', @level1name=N'ShippingCarrier', @level2name=N'ShippingCarrierName', @value=N'Name of the shipping carrier.',                                                @name=N'MS_Description', @level0type=N'SCHEMA', @level1type=N'TABLE', @level2type=N'COLUMN';
GO
EXEC sp_addextendedproperty @level0name=N'Shipping', @level1name=N'ShippingCarrier', @level2name=N'pkcShipppingCarrier', @value=N'Defines the primary key for the Shipment record using the ShipmentId column.', @name=N'MS_Description', @level0type=N'SCHEMA', @level1type=N'TABLE', @level2type=N'CONSTRAINT';
GO