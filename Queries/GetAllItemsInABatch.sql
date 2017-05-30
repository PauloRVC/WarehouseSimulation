
--Get all items and the batch they belong to for non PW/PT batches
SELECT 
x.TaskID, x.PickTaskID, x.SKUID, x.SKUDescription, x.EventTime, x.[Event], x.TaskIDTimeCreated, x.LineItemTimeCreated
FROM MissionDetailHistory AS x
WHERE x.TaskID IN
(
SELECT DISTINCT CONCAT(subQ.Mission, '~', subQ.OrderID) FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'
AND SUBSTRING(subQ.OrderID,1,2) != 'PT'
AND SUBSTRING(subQ.OrderID,1,2) != 'PW'
)
AND x.OperatorID = 'HOSTCOMM'