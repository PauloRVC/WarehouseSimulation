
INSERT INTO OrderItems (OrderID, ItemID, PutTimestamp)
SELECT 
o.OrderID, i.ItemID, MAX(m.EventTime)
FROM 
Orders AS o INNER JOIN
MissionDetailHistory AS m ON o.OrderIndicator = m.TaskID
INNER JOIN Items AS i ON i.ItemIndicator = m.SKUID
INNER JOIN [Batches] AS b ON b.BatchID = o.BatchID
WHERE o.OrderIndicator != b.BatchIndicator
AND WorkAreaID = 'PutWall'
AND [Event] = 'COMPLETED'
GROUP BY o.OrderID, i.ItemID