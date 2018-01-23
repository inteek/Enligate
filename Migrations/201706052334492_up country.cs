namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class upcountry : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblCountry",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        nicename = c.String(),
                        numcode = c.Int(nullable: false),
                        phonecode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblCountry");
        }
    }
}
