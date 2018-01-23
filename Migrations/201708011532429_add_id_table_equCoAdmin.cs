namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_id_table_equCoAdmin : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.tblEquiposCoAdministradores");
            AddColumn("dbo.tblEquiposCoAdministradores", "ecaId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.tblEquiposCoAdministradores", "ecaId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.tblEquiposCoAdministradores");
            DropColumn("dbo.tblEquiposCoAdministradores", "ecaId");
            AddPrimaryKey("dbo.tblEquiposCoAdministradores", new[] { "equId", "ecaCorreoId" });
        }
    }
}
