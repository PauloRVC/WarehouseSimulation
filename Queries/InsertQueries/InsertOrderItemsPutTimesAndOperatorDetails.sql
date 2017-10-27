INSERT INTO OrderItems (OrderID, ItemID, PutTimestamp, OperatorID)
SELECT 

a.OrderID, a.ItemID, a.putTime, ops.OperatorID
FROM
(
SELECT
o.OrderID, i.ItemID, MAX(m.EventTime) AS putTime
FROM 
Orders AS o INNER JOIN
MissionDetailHistory AS m ON o.OrderIndicator = m.TaskID
INNER JOIN Items AS i ON i.ItemIndicator = m.SKUID
INNER JOIN [Batches] AS b ON b.BatchID = o.BatchID
WHERE o.OrderIndicator != b.BatchIndicator
AND WorkAreaID = 'PutWall'
AND [Event] = 'COMPLETED'
GROUP BY o.OrderID, i.ItemID
) 

AS a 
INNER JOIN Items AS it ON a.ItemID = it.ItemID
INNER JOIN Orders AS ord ON a.OrderID = ord.OrderID
INNER JOIN MissionDetailHistory AS h ON
ord.OrderIndicator = h.TaskID
AND it.ItemIndicator = h.SKUID
AND a.putTime = h.EventTime
LEFT OUTER JOIN Operators AS ops ON h.OperatorID = ops.OperatorID_Text