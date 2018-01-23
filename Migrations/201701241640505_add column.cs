namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcolumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblTorneos", "torPagado", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblTorneos", "torPagado");
        }
    }
}
