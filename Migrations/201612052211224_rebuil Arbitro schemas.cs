namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuilArbitroschemas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblArbitrosTorneos", "arbCodigoConfirmacion", c => c.String());
            AddColumn("dbo.tblArbitrosTorneos", "arbConfirmado", c => c.Boolean(nullable: false));
            DropColumn("dbo.tblArbitros", "arbCodigoConfirmacion");
            DropColumn("dbo.tblArbitros", "arbConfirmado");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblArbitros", "arbConfirmado", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblArbitros", "arbCodigoConfirmacion", c => c.String());
            DropColumn("dbo.tblArbitrosTorneos", "arbConfirmado");
            DropColumn("dbo.tblArbitrosTorneos", "arbCodigoConfirmacion");
        }
    }
}
