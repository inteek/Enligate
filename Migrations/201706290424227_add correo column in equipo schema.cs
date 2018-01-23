namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcorreocolumninequiposchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblEquipos", "equAdminCorreo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblEquipos", "equAdminCorreo");
        }
    }
}
