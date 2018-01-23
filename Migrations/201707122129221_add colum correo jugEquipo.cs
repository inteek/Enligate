namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcolumcorreojugEquipo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblJugadorEquipos", "jugCorrreo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblJugadorEquipos", "jugCorrreo");
        }
    }
}
