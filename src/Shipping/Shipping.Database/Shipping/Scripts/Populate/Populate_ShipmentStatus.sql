MERGE INTO Shipping.ShipmentStatus AS TARGET
USING (VALUES (1, 'Inventory'),
              (2, 'Picking'),
              (3, 'Packed'),
              (4, 'Shipped'),
              (5, 'Delivered'))
AS SOURCE (ShipmentStatusId, ShipmentStatusName)
ON TARGET.ShipmentStatusId = SOURCE.ShipmentStatusId
WHEN MATCHED THEN UPDATE SET TARGET.ShipmentStatusName = SOURCE.ShipmentStatusName
WHEN NOT MATCHED THEN INSERT (ShipmentStatusId,
                              ShipmentStatusName)
                      VALUES (SOURCE.ShipmentStatusId,
                              SOURCE.ShipmentStatusName);