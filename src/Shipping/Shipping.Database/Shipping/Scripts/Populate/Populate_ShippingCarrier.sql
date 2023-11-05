SET IDENTITY_INSERT Shipping.ShippingCarrier ON
GO

MERGE INTO Shipping.ShippingCarrier AS TARGET
USING (VALUES(1, 'USPS'),
             (2, 'UPS'),
             (3, 'FedEx'))
AS SOURCE (ShippingCarrierId, ShippingCarrierName)
ON TARGET.ShippingCarrierId = SOURCE.ShippingCarrierId
WHEN MATCHED THEN UPDATE SET TARGET.ShippingCarrierName = SOURCE.ShippingCarrierName
WHEN NOT MATCHED THEN INSERT (ShippingCarrierId,
                              ShippingCarrierName)
                      VALUES (SOURCE.ShippingCarrierId,
                              SOURCE.ShippingCarrierName);

SET IDENTITY_INSERT Shipping.ShippingCarrier OFF
GO