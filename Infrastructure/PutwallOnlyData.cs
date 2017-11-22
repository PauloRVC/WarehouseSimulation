using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;

namespace Infrastructure
{
    internal class PutwallOnlyData: WarehouseContext
    {
        internal PutwallOnlyData()
        {
            var P06 = base.Locations.Where(x => x.ScannerIndicator == "P06").First();
            var FirstArrivals = base.BatchScan.Where(x => x.CurrentLocationID == Scanner901.LocationID).GroupBy(x => x.BatchID);
            PutwallBatchIDs = FirstArrivals.Where(x => x.Any(y => y.IntendedDestination.LocationID == P06.LocationID)).Select(x => x.Key).ToList();
            //PutwallBatchIDs = PutwallBatchIDs.Intersect(base.OrderItems.Where(x => x.PutTimestamp.HasValue).Select(x => x.Order.Batch.BatchID)).ToList();
        }
        private List<int> PutwallBatchIDs;
        public override IQueryable<BatchScan> BatchScan { get => base.BatchScan.Where(x => PutwallBatchIDs.Contains(x.BatchID)); }
    }
}
