namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class debug : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblTorneoCoAdministradores", "tcaUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblTorneoCoAdministradores", "torId", "dbo.tblTorneos");
            DropIndex("dbo.tblTorneoCoAdministradores", new[] { "tcaUserId" });
            DropPrimaryKey("dbo.tblTorneoCoAdministradores");
            AddColumn("dbo.tblTorneoCoAdministradores", "userCorreo", c => c.String());
            AddPrimaryKey("dbo.tblTorneoCoAdministradores", "torId");
            AddForeignKey("dbo.tblTorneoCoAdministradores", "torId", "dbo.tblTorneos", "torId");
            DropColumn("dbo.tblTorneoCoAdministradores", "tcaUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblTorneoCoAdministradores", "tcaUserId", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.tblTorneoCoAdministradores", "torId", "dbo.tblTorneos");
            DropPrimaryKey("dbo.tblTorneoCoAdministradores");
            DropColumn("dbo.tblTorneoCoAdministradores", "userCorreo");
            AddPrimaryKey("dbo.tblTorneoCoAdministradores", new[] { "torId", "tcaUserId" });
            CreateIndex("dbo.tblTorneoCoAdministradores", "tcaUserId");
            AddForeignKey("dbo.tblTorneoCoAdministradores", "torId", "dbo.tblTorneos", "torId", cascadeDelete: true);
            AddForeignKey("dbo.tblTorneoCoAdministradores", "tcaUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
