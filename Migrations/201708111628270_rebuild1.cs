namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuild1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblTorneoCoAdministradores", "tcaUserId", "dbo.AspNetUsers");
            DropIndex("dbo.tblTorneoCoAdministradores", new[] { "tcaUserId" });
            DropPrimaryKey("dbo.tblTorneoCoAdministradores");
            AddColumn("dbo.tblTorneoCoAdministradores", "tcoId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.tblTorneoCoAdministradores", "userCorreo", c => c.String());
            AddPrimaryKey("dbo.tblTorneoCoAdministradores", "tcoId");
            DropColumn("dbo.tblTorneoCoAdministradores", "tcaUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblTorneoCoAdministradores", "tcaUserId", c => c.String(nullable: false, maxLength: 128));
            DropPrimaryKey("dbo.tblTorneoCoAdministradores");
            DropColumn("dbo.tblTorneoCoAdministradores", "userCorreo");
            DropColumn("dbo.tblTorneoCoAdministradores", "tcoId");
            AddPrimaryKey("dbo.tblTorneoCoAdministradores", new[] { "torId", "tcaUserId" });
            CreateIndex("dbo.tblTorneoCoAdministradores", "tcaUserId");
            AddForeignKey("dbo.tblTorneoCoAdministradores", "tcaUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
