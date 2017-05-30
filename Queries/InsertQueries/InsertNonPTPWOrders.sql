INSERT INTO Orders (OrderIndicator, BatchID)

SELECT x.a, (SELECT BatchID FROM [Batches] AS y WHERE y.BatchIndicator = x.a) FROM
(
SELECT DISTINCT CONCAT(subQ.Mission, '~', subQ.OrderID) AS a
FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'
AND SUBSTRING(subQ.OrderID,1,2) != 'PT'
AND SUBSTRING(subQ.OrderID,1,2) != 'PW'
) AS x