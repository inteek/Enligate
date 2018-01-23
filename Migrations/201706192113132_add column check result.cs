namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcolumncheckresult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblPartidos", "parCheck", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblPartidos", "parCheck");
        }
    }
}
