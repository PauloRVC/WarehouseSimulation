namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullablePickPutTimes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderItems", "PickTimestamp", c => c.DateTime());
            AlterColumn("dbo.OrderItems", "PutTimestamp", c => c.DateTime());
        }
        
        public override void Down()
        {
            AddColumn("dbo.Batches", "CreationTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.OrderItems", "PutTimestamp", c => c.DateTime(nullable: false));
            AlterColumn("dbo.OrderItems", "PickTimestamp", c => c.DateTime(nullable: false));
        }
    }
}
