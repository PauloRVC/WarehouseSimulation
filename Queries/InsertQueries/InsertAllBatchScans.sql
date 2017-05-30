INSERT INTO WarehouseAnalyticsII..BatchScans (BatchID, CurrentLocationID, [Timestamp], IntendedDestinationID, ActualDestinationID)

--Get all Batch Scans
SELECT 
b.BatchID,
l.LocationID,
x.[Timestamp],
m.LocationID,
n.LocationID

FROM
(
SELECT  CONCAT(s.Mission, '~', s.OrderID) AS BatchIndicator, 
CAST(s.ScannerID AS NVARCHAR(23)) AS CurrentLocationIndicator, 
s.[Timestamp], 
s.IntendedLogicalDestinationID AS IntendedLogicalDestinationIndicator, 
s.LogicalDestinationID  AS ActualLocationIndicator
FROM WarehouseAnalyticsII..MissionComplete AS s
WHERE LEN(s.Mission) = 8 AND s.OrderID != 'None') AS x 
INNER JOIN WarehouseAnalyticsII..Locations AS l ON x.CurrentLocationIndicator = l.ScannerIndicator
INNER JOIN WarehouseAnalyticsII..Locations AS m ON x.IntendedLogicalDestinationIndicator = m.ScannerIndicator
INNER JOIN WarehouseAnalyticsII..Locations AS n ON x.ActualLocationIndicator = n.ScannerIndicator
INNER JOIN WarehouseAnalyticsII..[Batches] AS b ON x.BatchIndicator = b.BatchIndicator 