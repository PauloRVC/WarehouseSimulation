using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Infrastructure.Models;

namespace Infrastructure
{
    internal class WarehouseContext : DbContext
    {
        public DbSet<BatchScan> BatchScans { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Location> Locations { get; set; }
        public Location Scanner901
        {
            get { return Locations.Where(x => x.ScannerIndicator == "901").First(); }
        }
        public WarehouseContext() : base("WarehouseAnalytics")
        {
            Database.SetInitializer<WarehouseContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasMany(x => x.OrderItems).WithRequired(y => y.Item).HasForeignKey(y => y.ItemID);
            modelBuilder.Entity<Order>().HasMany(x => x.OrderItems).WithRequired(y => y.Order).HasForeignKey(y => y.OrderID);

            modelBuilder.Entity<Batch>().HasMany(x => x.Orders).WithRequired(y => y.Batch).HasForeignKey(y => y.BatchID);

            modelBuilder.Entity<BatchScan>().HasRequired(x => x.Batch).WithMany(y => y.BatchScans).HasForeignKey(x => x.BatchID);

            modelBuilder.Entity<BatchScan>().HasRequired(x => x.CurrentLocation).WithMany(y => y.CurrentLocationBatchScans).HasForeignKey(x => x.CurrentLocationID).WillCascadeOnDelete(false);
            modelBuilder.Entity<BatchScan>().HasRequired(x => x.IntendedDestination).WithMany(y => y.IntendedDestinationBatchScans).HasForeignKey(x => x.IntendedDestinationID).WillCascadeOnDelete(false);
            modelBuilder.Entity<BatchScan>().HasRequired(x => x.ActualDestination).WithMany(y => y.ActualDestinationBatchScans).HasForeignKey(x => x.ActualDestinationID).WillCascadeOnDelete(false);


            modelBuilder.Entity<OrderItem>().HasKey(x => new { x.OrderID, x.ItemID });
            modelBuilder.Entity<BatchScan>().HasKey(x => new { x.BatchID, x.CurrentLocationID, x.Timestamp });
                
        }

    }
    public class WarehouseData
    {
        private WarehouseContext db = new WarehouseContext();
        public ILogger Logger { get; set; } = new NullLogger();
        public Location Scanner901
        {
            get
            {
               return db.Locations.Where(x => x.ScannerIndicator == "901").First();
            }
        }
        public Location P06
        {
            get
            {
                return db.Locations.Where(x => x.ScannerIndicator == "P06").First();
            }
        }
        public List<BatchScan> FirstArrivals(Location location, DateTime day)
        {
            var daysBatchScans = db.BatchScans.Where(x => DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                                                && x.CurrentLocation.LocationID == location.LocationID);

            //Keep only the first arrival for each batch
            return daysBatchScans.GroupBy(x => x.BatchID).Select(x => x.OrderBy(y => y.Timestamp).FirstOrDefault()).OrderBy(x => x.Timestamp).
                Include(x => x.ActualDestination).Include(x => x.CurrentLocation).Include(x => x.IntendedDestination).ToList();
        }
        public  List<BatchScan> LastArrivals(Location location, DateTime day)
        {
            var daysBatchScans = db.BatchScans.Where(x => DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                                                && x.CurrentLocation.LocationID == location.LocationID);

            //Keep only the first arrival for each batch
            return daysBatchScans.GroupBy(x => x.BatchID).Select(x => x.OrderByDescending(y => y.Timestamp).FirstOrDefault()).OrderBy(x => x.Timestamp).ToList();
        }
        public List<Tuple<int, DateTime>> FirstPutTimes(DateTime day, List<int> batchIDs)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                                && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                                && batchIDs.Contains(x.Order.BatchID));


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderBy(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
        public  List<Tuple<int, DateTime>> LastPutTimes(DateTime day, List<int> batchIDs)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue 
                                                && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                                && batchIDs.Contains(x.Order.BatchID));


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderByDescending(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
        public  List<DateTime> GetBatchScanAvailability()
        {
            return db.BatchScans.Select(x => DbFunctions.TruncateTime(x.Timestamp)).Distinct().ToList().Select(x => ((DateTime)x).Date).ToList();
        }
        public  List<DateTime> GetPutTimeAvailability()
        {
            return db.OrderItems.Where(x => x.PutTimestamp.HasValue).Select(x => DbFunctions.TruncateTime(x.PutTimestamp)).Distinct().ToList().Select(x => ((DateTime)x).Date).ToList();
        }
        public  List<DateTime> GetOverallAvailability()
        {
            return GetBatchScanAvailability().Intersect(GetPutTimeAvailability()).ToList();
        }
        public Dictionary<int, int> GetPutsPerHour(DateTime day)
        {
            var lastPuts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                               && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date).
                                                GroupBy(x => x.Order.BatchID).Select(x => x.OrderByDescending(y => y.PutTimestamp).FirstOrDefault()).
                                                GroupBy(x => x.PutTimestamp.Value.Hour).
                                                Select(x => new { hour = x.Key, count = x.Count() });
            var pph = new Dictionary<int, int>();

            foreach(var a in lastPuts)
            {
                pph.Add(a.hour, a.count);
            }

            for(int i = 0; i < 24; i++)
            {
                if (!pph.ContainsKey(i))
                {
                    pph.Add(i, 0);
                }
            }
            return pph;
        }
        public List<int> GetRecircTimes(DateTime day)
        {
            var res = db.BatchScans.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                               DbFunctions.TruncateTime(x.Timestamp) == day.Date).
                                               GroupBy(x => x.BatchID).Where(x => x.Count() > 1).
                                               Select(x => x.Select(y => y.Timestamp).OrderBy(y => y).ToList());

            List<int> recircTimes = new List<int>();

            foreach(var group in res)
            {
                for(int i = 1; i < group.Count; i++)
                {
                    recircTimes.Add((int)group[i].Subtract(group[i-1]).TotalSeconds);
                }
            }

            return recircTimes;
        }
    }
}
