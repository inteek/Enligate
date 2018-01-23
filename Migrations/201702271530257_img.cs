namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class img : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblPartidos", "imgDos", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblPartidos", "imgDos");
        }
    }
}
