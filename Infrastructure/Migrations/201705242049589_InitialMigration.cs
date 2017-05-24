namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BatchScans",
                c => new
                    {
                        BatchID = c.Int(nullable: false),
                        CurrentLocationID = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        IntendedDestinationID = c.Int(nullable: false),
                        ActualDestinationID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BatchID, t.CurrentLocationID, t.Timestamp })
                .ForeignKey("dbo.Locations", t => t.ActualDestinationID, cascadeDelete: false)
                .ForeignKey("dbo.Batches", t => t.BatchID, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.CurrentLocationID, cascadeDelete: false)
                .ForeignKey("dbo.Locations", t => t.IntendedDestinationID, cascadeDelete: false)
                .Index(t => t.BatchID)
                .Index(t => t.CurrentLocationID)
                .Index(t => t.IntendedDestinationID)
                .Index(t => t.ActualDestinationID);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        LocationID = c.Int(nullable: false, identity: true),
                        ScannerIndicator = c.String(),
                    })
                .PrimaryKey(t => t.LocationID);
            
            CreateTable(
                "dbo.Batches",
                c => new
                    {
                        BatchID = c.Int(nullable: false, identity: true),
                        BatchIndicator = c.String(),
                        CreationTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BatchID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        BatchID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Batches", t => t.BatchID, cascadeDelete: true)
                .Index(t => t.BatchID);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        OrderID = c.Int(nullable: false),
                        ItemID = c.Int(nullable: false),
                        PickTimestamp = c.DateTime(nullable: false),
                        PutTimestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrderID, t.ItemID })
                .ForeignKey("dbo.Items", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.Orders", t => t.OrderID, cascadeDelete: true)
                .Index(t => t.OrderID)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        ItemID = c.Int(nullable: false, identity: true),
                        ItemIndicator = c.String(),
                    })
                .PrimaryKey(t => t.ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BatchScans", "IntendedDestinationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "CurrentLocationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "BatchID", "dbo.Batches");
            DropForeignKey("dbo.Orders", "BatchID", "dbo.Batches");
            DropForeignKey("dbo.OrderItems", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "ItemID", "dbo.Items");
            DropForeignKey("dbo.BatchScans", "ActualDestinationID", "dbo.Locations");
            DropIndex("dbo.OrderItems", new[] { "ItemID" });
            DropIndex("dbo.OrderItems", new[] { "OrderID" });
            DropIndex("dbo.Orders", new[] { "BatchID" });
            DropIndex("dbo.BatchScans", new[] { "ActualDestinationID" });
            DropIndex("dbo.BatchScans", new[] { "IntendedDestinationID" });
            DropIndex("dbo.BatchScans", new[] { "CurrentLocationID" });
            DropIndex("dbo.BatchScans", new[] { "BatchID" });
            DropTable("dbo.Items");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.Batches");
            DropTable("dbo.Locations");
            DropTable("dbo.BatchScans");
        }
    }
}
