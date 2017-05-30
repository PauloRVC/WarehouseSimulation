
INSERT INTO Items (ItemIndicator, ItemDescription)

SELECT   m1.SKUID,
MAX(m1.SKUDescription)
 FROM MissionDetailHistory AS m1
WHERE m1.SKUDescription != 'PACKING LIST'
GROUP BY m1.SKUID;