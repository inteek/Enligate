namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddatetimearbleague : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblArbitrosLigas", "arbLigaFechaCreacionUTC", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblArbitrosLigas", "arbLigaFechaCreacionUTC");
        }
    }
}
