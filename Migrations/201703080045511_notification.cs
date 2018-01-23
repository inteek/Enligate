namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblLigas", "ligNotificacion", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblLigas", "ligNotificacion");
        }
    }
}
