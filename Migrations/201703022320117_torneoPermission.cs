namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class torneoPermission : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblTorneos", "torFechaInicio", c => c.DateTime());
            AlterColumn("dbo.tblTorneos", "torFechaLimiteInscripcion", c => c.DateTime());
            AlterColumn("dbo.tblTorneos", "torNumeroJuegos", c => c.Int());
            AlterColumn("dbo.tblTorneos", "torNumeroEquipos", c => c.Int());
            AlterColumn("dbo.tblTorneos", "torMaxJugadoresEquipo", c => c.Int());
            AlterColumn("dbo.tblTorneos", "torPuntosGanar", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torPuntosEmpatar", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torPuntosPerder", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torNumeroContacto", c => c.String());
            AlterColumn("dbo.tblTorneos", "torCorreoContacto", c => c.String());
            AlterColumn("dbo.tblTorneos", "torPrecioTorneo", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torDiasParaPago", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblTorneos", "torDiasParaPago", c => c.Int(nullable: false));
            AlterColumn("dbo.tblTorneos", "torPrecioTorneo", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torCorreoContacto", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.tblTorneos", "torNumeroContacto", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.tblTorneos", "torPuntosPerder", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torPuntosEmpatar", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torPuntosGanar", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblTorneos", "torMaxJugadoresEquipo", c => c.Int(nullable: false));
            AlterColumn("dbo.tblTorneos", "torNumeroEquipos", c => c.Int(nullable: false));
            AlterColumn("dbo.tblTorneos", "torNumeroJuegos", c => c.Int(nullable: false));
            AlterColumn("dbo.tblTorneos", "torFechaLimiteInscripcion", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tblTorneos", "torFechaInicio", c => c.DateTime(nullable: false));
        }
    }
}
