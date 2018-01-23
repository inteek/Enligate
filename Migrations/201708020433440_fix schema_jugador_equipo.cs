namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixschema_jugador_equipo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblJugadorEquipos", "jugId", "dbo.tblJugadores");
            DropIndex("dbo.tblJugadorEquipos", new[] { "jugId" });
            DropColumn("dbo.tblJugadorEquipos", "jugId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblJugadorEquipos", "jugId", c => c.Int(nullable: false));
            CreateIndex("dbo.tblJugadorEquipos", "jugId");
            AddForeignKey("dbo.tblJugadorEquipos", "jugId", "dbo.tblJugadores", "jugId", cascadeDelete: true);
        }
    }
}
