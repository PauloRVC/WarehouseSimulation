using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Infrastructure.Models;

namespace Infrastructure
{
    public class WarehouseContext : DbContext
    {
        public DbSet<BatchScan> BatchScans { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
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
}
