INSERT INTO Locations (ScannerIndicator)
SELECT DISTINCT LogicalDestinationID  AS LocationIndicator FROM MissionComplete
UNION SELECT DISTINCT IntendedLogicalDestinationID AS LocationIndicator FROM MissionComplete
UNION SELECT DISTINCT CAST(ScannerID AS nvarchar) AS LocationIndicator FROM MissionComplete