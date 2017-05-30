--Gets all distinct Batches
SELECT DISTINCT CONCAT(subQ.Mission, '~', subQ.OrderID) AS BatchIndicator FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'