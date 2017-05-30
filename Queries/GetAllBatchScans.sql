--Get all Batch Scans
SELECT TOP(100)
(SELECT y.BatchID FROM [Batches] AS y WHERE y.BatchIndicator = x.BatchIndicator) AS BatchID,
(SELECT z.LocationID FROM Locations AS z WHERE z.ScannerIndicator = x.CurrentLocationIndicator) AS CurrentLocationID,
[Timestamp],
(SELECT w.LocationID FROM Locations AS w WHERE w.ScannerIndicator = x.IntendedLogicalDestinationIndicator) AS IntendedDestinationID,
(SELECT v.LocationID FROM Locations AS v WHERE v.ScannerIndicator = x.ActualLocationIndicator) AS ActualDestinationID
FROM
(
SELECT TOP(100) CONCAT(Mission, '~', OrderID) AS BatchIndicator, 
CAST(ScannerID AS nvarchar) AS CurrentLocationIndicator, 
[Timestamp], 
CAST(IntendedLogicalDestinationID AS nvarchar) AS IntendedLogicalDestinationIndicator, 
CAST(LogicalDestinationID AS nvarchar) AS ActualLocationIndicator
FROM MissionComplete
WHERE LEN(Mission) = 8 AND OrderID != 'None') AS x