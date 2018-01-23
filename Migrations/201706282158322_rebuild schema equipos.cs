namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuildschemaequipos : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblEquipos", "equUserIdCreador", "dbo.AspNetUsers");
            DropIndex("dbo.tblEquipos", new[] { "equUserIdCreador" });
            AlterColumn("dbo.tblEquipos", "equUserIdCreador", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblEquipos", "equUserIdCreador", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.tblEquipos", "equUserIdCreador");
            AddForeignKey("dbo.tblEquipos", "equUserIdCreador", "dbo.AspNetUsers", "Id");
        }
    }
}
