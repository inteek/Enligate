namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class quitartarifasdeschemaligas : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblLigas", "tcfppId", "dbo.tblTarifasCfpPeriodicidades");
            DropIndex("dbo.tblLigas", new[] { "tcfppId" });
            DropColumn("dbo.tblLigas", "tcfppId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblLigas", "tcfppId", c => c.Int(nullable: false));
            CreateIndex("dbo.tblLigas", "tcfppId");
            AddForeignKey("dbo.tblLigas", "tcfppId", "dbo.tblTarifasCfpPeriodicidades", "tcfppId", cascadeDelete: true);
        }
    }
}
