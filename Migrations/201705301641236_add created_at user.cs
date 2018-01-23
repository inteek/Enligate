namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcreated_atuser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "created_at", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "created_at");
        }
    }
}
