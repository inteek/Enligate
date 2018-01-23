namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprivatetorneo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblTorneos", "torPrivate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblTorneos", "torPrivate");
        }
    }
}
