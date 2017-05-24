namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BatchScans", "ActualDestinationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "CurrentLocationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "IntendedDestinationID", "dbo.Locations");
            AddForeignKey("dbo.BatchScans", "ActualDestinationID", "dbo.Locations", "LocationID");
            AddForeignKey("dbo.BatchScans", "CurrentLocationID", "dbo.Locations", "LocationID");
            AddForeignKey("dbo.BatchScans", "IntendedDestinationID", "dbo.Locations", "LocationID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BatchScans", "IntendedDestinationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "CurrentLocationID", "dbo.Locations");
            DropForeignKey("dbo.BatchScans", "ActualDestinationID", "dbo.Locations");
            AddForeignKey("dbo.BatchScans", "IntendedDestinationID", "dbo.Locations", "LocationID", cascadeDelete: true);
            AddForeignKey("dbo.BatchScans", "CurrentLocationID", "dbo.Locations", "LocationID", cascadeDelete: true);
            AddForeignKey("dbo.BatchScans", "ActualDestinationID", "dbo.Locations", "LocationID", cascadeDelete: true);
        }
    }
}
