namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuildcountry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCountry", "iso", c => c.String());
            AddColumn("dbo.tblCountry", "iso3", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCountry", "iso3");
            DropColumn("dbo.tblCountry", "iso");
        }
    }
}
