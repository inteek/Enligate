namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuildJugadorEquipo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblJugadorEquipos", "jugUserId", "dbo.AspNetUsers");
            DropIndex("dbo.tblJugadorEquipos", new[] { "jugUserId" });
            AlterColumn("dbo.tblJugadorEquipos", "jugUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.tblJugadorEquipos", "jugUserId");
            AddForeignKey("dbo.tblJugadorEquipos", "jugUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblJugadorEquipos", "jugUserId", "dbo.AspNetUsers");
            DropIndex("dbo.tblJugadorEquipos", new[] { "jugUserId" });
            AlterColumn("dbo.tblJugadorEquipos", "jugUserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.tblJugadorEquipos", "jugUserId");
            AddForeignKey("dbo.tblJugadorEquipos", "jugUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
