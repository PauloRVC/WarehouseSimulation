INSERT INTO OrderItems (OrderID, ItemID, PickTimestamp)
SELECT DISTINCT
o.OrderID,
i.ItemID,
pickTimes.PickTime
FROM

(
SELECT 
 x.TaskID, x.SKUID, MAX(x.EventTime) AS PickTime
FROM MissionDetailHistory AS x
WHERE x.TaskID IN
(
SELECT DISTINCT CONCAT(subQ.Mission, '~', subQ.OrderID) FROM MissionComplete as subQ
WHERE LEN(subQ.Mission) = 8 AND subQ.OrderID != 'None'
AND SUBSTRING(subQ.OrderID,1,2) != 'PT'
AND SUBSTRING(subQ.OrderID,1,2) != 'PW'
)
AND x.OperatorID  != 'HOSTCOMM'
GROUP BY  x.TaskID, x.SKUID
) 
AS pickTimes INNER JOIN Orders AS o ON pickTImes.TaskID = o.OrderIndicator
INNER JOIN Items AS i ON pickTimes.SKUID = i.ItemIndicator