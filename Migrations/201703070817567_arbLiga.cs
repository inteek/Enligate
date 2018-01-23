namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class arbLiga : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblArbitrosTorneos", "arbId", "dbo.tblArbitros");
            DropForeignKey("dbo.tblArbitrosTorneos", "torId", "dbo.tblTorneos");
            DropIndex("dbo.tblArbitrosTorneos", new[] { "arbId" });
            DropIndex("dbo.tblArbitrosTorneos", new[] { "torId" });
            CreateTable(
                "dbo.tblArbitrosLigas",
                c => new
                    {
                        arbId = c.Int(nullable: false),
                        ligId = c.Int(nullable: false),
                        arbCodigoConfirmacion = c.String(),
                        arbConfirmado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.arbId, t.ligId })
                .ForeignKey("dbo.tblArbitros", t => t.arbId, cascadeDelete: true)
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .Index(t => t.arbId)
                .Index(t => t.ligId);
            
            DropTable("dbo.tblArbitrosTorneos");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.tblArbitrosTorneos",
                c => new
                    {
                        arbId = c.Int(nullable: false),
                        torId = c.Int(nullable: false),
                        arbCodigoConfirmacion = c.String(),
                        arbConfirmado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.arbId, t.torId });
            
            DropForeignKey("dbo.tblArbitrosLigas", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblArbitrosLigas", "arbId", "dbo.tblArbitros");
            DropIndex("dbo.tblArbitrosLigas", new[] { "ligId" });
            DropIndex("dbo.tblArbitrosLigas", new[] { "arbId" });
            DropTable("dbo.tblArbitrosLigas");
            CreateIndex("dbo.tblArbitrosTorneos", "torId");
            CreateIndex("dbo.tblArbitrosTorneos", "arbId");
            AddForeignKey("dbo.tblArbitrosTorneos", "torId", "dbo.tblTorneos", "torId", cascadeDelete: true);
            AddForeignKey("dbo.tblArbitrosTorneos", "arbId", "dbo.tblArbitros", "arbId", cascadeDelete: true);
        }
    }
}
