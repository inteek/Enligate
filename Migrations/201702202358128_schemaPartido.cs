namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schemaPartido : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblPartidos", "arbId", "dbo.tblArbitros");
            DropIndex("dbo.tblPartidos", new[] { "arbId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.tblPartidos", "arbId");
            AddForeignKey("dbo.tblPartidos", "arbId", "dbo.tblArbitros", "arbId");
        }
    }
}
namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schemaPartido : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblPartidos", "arbId", "dbo.tblArbitros");
            DropIndex("dbo.tblPartidos", new[] { "arbId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.tblPartidos", "arbId");
            AddForeignKey("dbo.tblPartidos", "arbId", "dbo.tblArbitros", "arbId");
        }
    }
}

