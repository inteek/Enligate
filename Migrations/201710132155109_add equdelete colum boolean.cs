namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addequdeletecolumboolean : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblEquipos", "equDelete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblEquipos", "equDelete");
        }
    }
}
