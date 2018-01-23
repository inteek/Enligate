namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuild : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblLigaCoAdmnInit",
                c => new
                    {
                        ligId = c.Int(nullable: false),
                        lcaConfirmacion = c.Boolean(nullable: false),
                        userEmail = c.String(),
                        lcaCodigoConfirmacion = c.String(),
                        lcaFechaConfirmacionUTC = c.DateTime(),
                    })
                .PrimaryKey(t => t.ligId)
                .ForeignKey("dbo.tblLigas", t => t.ligId)
                .Index(t => t.ligId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblLigaCoAdmnInit", "ligId", "dbo.tblLigas");
            DropIndex("dbo.tblLigaCoAdmnInit", new[] { "ligId" });
            DropTable("dbo.tblLigaCoAdmnInit");
        }
    }
}
