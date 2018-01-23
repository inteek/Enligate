namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addArbitroPartido : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblArbitrosPartidos",
                c => new
                    {
                        arbId = c.Int(nullable: false),
                        parId = c.Int(nullable: false),
                        arbCodigoConfirmacion = c.String(),
                        arbConfirmado = c.Boolean(nullable: false),
                        arbRechazar = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.arbId, t.parId })
                .ForeignKey("dbo.tblArbitros", t => t.arbId, cascadeDelete: true)
                .ForeignKey("dbo.tblPartidos", t => t.parId, cascadeDelete: true)
                .Index(t => t.arbId)
                .Index(t => t.parId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblArbitrosPartidos", "parId", "dbo.tblPartidos");
            DropForeignKey("dbo.tblArbitrosPartidos", "arbId", "dbo.tblArbitros");
            DropIndex("dbo.tblArbitrosPartidos", new[] { "parId" });
            DropIndex("dbo.tblArbitrosPartidos", new[] { "arbId" });
            DropTable("dbo.tblArbitrosPartidos");
        }
    }
}
