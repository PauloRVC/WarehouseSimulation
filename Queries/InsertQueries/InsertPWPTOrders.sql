INSERT INTO Orders (OrderIndicator, BatchID)

--Gets all Orders and the Batch they belong to
--ONLY WORKS FOR PT/PW
SELECT DISTINCT  
x.TaskID AS OrderIndicator, 
y.BatchID
--*
FROM MissionDetailHistory AS x INNER JOIN [Batches] AS y ON x.PickTaskID = y.BatchIndicator
WHERE x.PickTaskID IN
(
SELECT CONCAT(subQ.Mission, '~', subQ.OrderID) FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'
)