namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_key_lcaint : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblLigaCoAdmnInit", "ligId", "dbo.tblLigas");
            DropPrimaryKey("dbo.tblLigaCoAdmnInit");
            AddColumn("dbo.tblLigaCoAdmnInit", "lcaint", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.tblLigaCoAdmnInit", "lcaint");
            AddForeignKey("dbo.tblLigaCoAdmnInit", "ligId", "dbo.tblLigas", "ligId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblLigaCoAdmnInit", "ligId", "dbo.tblLigas");
            DropPrimaryKey("dbo.tblLigaCoAdmnInit");
            DropColumn("dbo.tblLigaCoAdmnInit", "lcaint");
            AddPrimaryKey("dbo.tblLigaCoAdmnInit", "ligId");
            AddForeignKey("dbo.tblLigaCoAdmnInit", "ligId", "dbo.tblLigas", "ligId");
        }
    }
}
