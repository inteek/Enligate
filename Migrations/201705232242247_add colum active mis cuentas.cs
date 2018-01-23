namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcolumactivemiscuentas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblUsersMisCuentas", "activo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblUsersMisCuentas", "activo");
        }
    }
}
