namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reportRebuild : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblLoginHistory",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        correoUsuario = c.String(),
                        exception = c.String(),
                        ipAddress = c.String(),
                        loginDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.tblReporteBugs",
                c => new
                    {
                        repoId = c.Int(nullable: false, identity: true),
                        correoUsuario = c.String(),
                        reporte = c.String(),
                        exception = c.String(),
                        ipAddress = c.String(),
                        repoFechaUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.repoId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblReporteBugs");
            DropTable("dbo.tblLoginHistory");
        }
    }
}
