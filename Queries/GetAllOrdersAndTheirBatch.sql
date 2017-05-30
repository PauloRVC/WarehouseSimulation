--Gets all Orders and the Batch they belong to
--ONLY WORKS FOR PT/PW
SELECT DISTINCT 
x.TaskID AS OrderIndicator, x.PickTaskID As BatchIndicator
--*
FROM MissionDetailHistory AS x
WHERE x.PickTaskID IN
(
SELECT DISTINCT CONCAT(subQ.Mission, '~', subQ.OrderID) FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'
)
