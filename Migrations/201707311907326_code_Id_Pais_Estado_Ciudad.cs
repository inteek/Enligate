namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class code_Id_Pais_Estado_Ciudad : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblUsersProfiles", "codeIdPais", c => c.String());
            AddColumn("dbo.tblUsersProfiles", "codeIdCiudad", c => c.String());
            AddColumn("dbo.tblUsersProfiles", "codeIdEstado", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblUsersProfiles", "codeIdEstado");
            DropColumn("dbo.tblUsersProfiles", "codeIdCiudad");
            DropColumn("dbo.tblUsersProfiles", "codeIdPais");
        }
    }
}
