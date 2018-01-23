namespace sw_EnligateWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class jugequipo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.tblArbitros",
                c => new
                    {
                        arbId = c.Int(nullable: false, identity: true),
                        arbCorreo = c.String(),
                        arbUserId = c.String(maxLength: 128),
                        arbNombre = c.String(),
                        arbFechaCreacionUTC = c.DateTime(nullable: false),
                        arbEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.arbId)
                .ForeignKey("dbo.AspNetUsers", t => t.arbUserId)
                .Index(t => t.arbUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        usuEstatus = c.Boolean(nullable: false),
                        usuEmailValidationCode = c.String(),
                        usuEmailValidationCodeEndDateUtc = c.DateTime(),
                        usuPasswordRecoveryCode = c.String(),
                        usuPasswordRecoveryCodeEndDateUtc = c.DateTime(),
                        usuRolActual = c.String(maxLength: 50),
                        usuCuentaPrincipal = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.tblArbitrosLigas",
                c => new
                    {
                        arbId = c.Int(nullable: false),
                        ligId = c.Int(nullable: false),
                        arbCodigoConfirmacion = c.String(),
                        arbConfirmado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.arbId, t.ligId })
                .ForeignKey("dbo.tblArbitros", t => t.arbId, cascadeDelete: true)
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .Index(t => t.arbId)
                .Index(t => t.ligId);
            
            CreateTable(
                "dbo.tblLigas",
                c => new
                    {
                        ligId = c.Int(nullable: false, identity: true),
                        ligUserIdCreador = c.String(nullable: false, maxLength: 128),
                        ligFechaRegistroUTC = c.DateTime(nullable: false),
                        ligTipoLiga = c.String(nullable: false, maxLength: 20),
                        tcfppId = c.Int(nullable: false),
                        ligImgUrl = c.String(),
                        ligNombreLiga = c.String(nullable: false, maxLength: 500),
                        ligCorreoContacto = c.String(nullable: false, maxLength: 256),
                        ligTelefonoContacto = c.String(nullable: false, maxLength: 30),
                        ligDescripcion = c.String(nullable: false, maxLength: 3000),
                        ligEstatus = c.Boolean(nullable: false),
                        ligAprobada = c.Boolean(nullable: false),
                        ligSolicitud = c.Boolean(nullable: false),
                        ligSolicitudRevisada = c.Boolean(nullable: false),
                        ligNotificacion = c.Boolean(nullable: false),
                        tarId = c.Int(),
                        ligPorcentajeDescuento = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ligTotalPagar = c.Decimal(nullable: false, precision: 18, scale: 2),
                        statusPago = c.String(),
                        ligLatitud = c.String(),
                        ligLongitud = c.String(),
                    })
                .PrimaryKey(t => t.ligId)
                .ForeignKey("dbo.tblLigaCategorias", t => t.ligTipoLiga, cascadeDelete: true)
                .ForeignKey("dbo.tblTarifas", t => t.tarId)
                .ForeignKey("dbo.tblTarifasCfpPeriodicidades", t => t.tcfppId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ligUserIdCreador, cascadeDelete: true)
                .Index(t => t.ligUserIdCreador)
                .Index(t => t.ligTipoLiga)
                .Index(t => t.tcfppId)
                .Index(t => t.tarId);
            
            CreateTable(
                "dbo.tblLigaCategorias",
                c => new
                    {
                        lcaId = c.String(nullable: false, maxLength: 20),
                        lcaCategoria = c.String(maxLength: 20),
                        lcaDescripcion = c.String(maxLength: 1024),
                        lcaEstatus = c.Boolean(nullable: false),
                        lcaOrden = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.lcaId);
            
            CreateTable(
                "dbo.tblTarifas",
                c => new
                    {
                        tarId = c.Int(nullable: false, identity: true),
                        tcfpptpmpId = c.Int(nullable: false),
                        tarCosto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        tarFechaRegistroUTC = c.DateTime(nullable: false),
                        tarEsPorcentaje = c.Boolean(nullable: false),
                        tarEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tarId)
                .ForeignKey("dbo.tblTarifasCfpptpMetodosPago", t => t.tcfpptpmpId, cascadeDelete: true)
                .Index(t => t.tcfpptpmpId);
            
            CreateTable(
                "dbo.tblTarifasCfpptpMetodosPago",
                c => new
                    {
                        tcfpptpmpId = c.Int(nullable: false, identity: true),
                        tcfpptpId = c.Int(nullable: false),
                        tmpIdMetodoPago = c.String(nullable: false, maxLength: 20),
                        tcfpptpmpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tcfpptpmpId)
                .ForeignKey("dbo.tblTarifasCfppTiposPago", t => t.tcfpptpId, cascadeDelete: true)
                .ForeignKey("dbo.tblTarifasMetodosPago", t => t.tmpIdMetodoPago, cascadeDelete: true)
                .Index(t => t.tcfpptpId)
                .Index(t => t.tmpIdMetodoPago);
            
            CreateTable(
                "dbo.tblTarifasCfppTiposPago",
                c => new
                    {
                        tcfpptpId = c.Int(nullable: false, identity: true),
                        tcfppId = c.Int(nullable: false),
                        ttpIdTipoPago = c.String(nullable: false, maxLength: 20),
                        tcfpptpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tcfpptpId)
                .ForeignKey("dbo.tblTarifasCfpPeriodicidades", t => t.tcfppId, cascadeDelete: true)
                .ForeignKey("dbo.tblTarifasTiposPago", t => t.ttpIdTipoPago, cascadeDelete: true)
                .Index(t => t.tcfppId)
                .Index(t => t.ttpIdTipoPago);
            
            CreateTable(
                "dbo.tblTarifasCfpPeriodicidades",
                c => new
                    {
                        tcfppId = c.Int(nullable: false, identity: true),
                        tcfpId = c.Int(nullable: false),
                        tpeIdPeriodicidad = c.String(nullable: false, maxLength: 15),
                        tcfppEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tcfppId)
                .ForeignKey("dbo.tblTarifasConceptosFormasPago", t => t.tcfpId, cascadeDelete: true)
                .ForeignKey("dbo.tblTarifasPeriodicidades", t => t.tpeIdPeriodicidad, cascadeDelete: true)
                .Index(t => t.tcfpId)
                .Index(t => t.tpeIdPeriodicidad);
            
            CreateTable(
                "dbo.tblTarifasConceptosFormasPago",
                c => new
                    {
                        tcfpId = c.Int(nullable: false, identity: true),
                        tcoIdConcepto = c.String(nullable: false, maxLength: 15),
                        tfpIdFormaPago = c.String(nullable: false, maxLength: 15),
                        tcfpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tcfpId)
                .ForeignKey("dbo.tblTarifasConceptos", t => t.tcoIdConcepto, cascadeDelete: true)
                .ForeignKey("dbo.tblTarifasFormasPago", t => t.tfpIdFormaPago, cascadeDelete: true)
                .Index(t => t.tcoIdConcepto)
                .Index(t => t.tfpIdFormaPago);
            
            CreateTable(
                "dbo.tblTarifasConceptos",
                c => new
                    {
                        tcoIdConcepto = c.String(nullable: false, maxLength: 15),
                        tcoEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tcoIdConcepto);
            
            CreateTable(
                "dbo.tblTarifasFormasPago",
                c => new
                    {
                        tfpIdFormaPago = c.String(nullable: false, maxLength: 15),
                        tfpDescripcion = c.String(maxLength: 1024),
                        tfpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tfpIdFormaPago);
            
            CreateTable(
                "dbo.tblTarifasPeriodicidades",
                c => new
                    {
                        tpeIdPeriodicidad = c.String(nullable: false, maxLength: 15),
                        tpeDescripcion = c.String(maxLength: 1024),
                        tpeEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tpeIdPeriodicidad);
            
            CreateTable(
                "dbo.tblTarifasTiposPago",
                c => new
                    {
                        ttpIdTipoPago = c.String(nullable: false, maxLength: 20),
                        ttpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ttpIdTipoPago);
            
            CreateTable(
                "dbo.tblTarifasMetodosPago",
                c => new
                    {
                        tmpIdMetodoPago = c.String(nullable: false, maxLength: 20),
                        tmpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.tmpIdMetodoPago);
            
            CreateTable(
                "dbo.tblArbitrosPartidos",
                c => new
                    {
                        arbId = c.Int(nullable: false),
                        parId = c.Int(nullable: false),
                        arbCodigoConfirmacion = c.String(),
                        arbConfirmado = c.Boolean(nullable: false),
                        arbRechazar = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.arbId, t.parId })
                .ForeignKey("dbo.tblArbitros", t => t.arbId, cascadeDelete: true)
                .ForeignKey("dbo.tblPartidos", t => t.parId, cascadeDelete: true)
                .Index(t => t.arbId)
                .Index(t => t.parId);
            
            CreateTable(
                "dbo.tblPartidos",
                c => new
                    {
                        parId = c.Int(nullable: false, identity: true),
                        ligId = c.Int(nullable: false),
                        lcatId = c.Int(nullable: false),
                        torId = c.Int(nullable: false),
                        arbNombre = c.String(),
                        arbId = c.Int(nullable: false),
                        equIdUno = c.Int(nullable: false),
                        equNombreEquipoUno = c.String(nullable: false),
                        equResultadoUno = c.Int(nullable: false),
                        equIdDos = c.Int(nullable: false),
                        equNombreEquipoDos = c.String(),
                        equResultadoDos = c.Int(nullable: false),
                        imgDos = c.String(),
                        parFecha_Inicio = c.DateTime(nullable: false),
                        parFecha_Fin = c.DateTime(nullable: false),
                        parEstado = c.String(maxLength: 100),
                        parEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.parId)
                .ForeignKey("dbo.tblLigas", t => t.ligId)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .Index(t => t.ligId)
                .Index(t => t.torId);
            
            CreateTable(
                "dbo.tblTorneos",
                c => new
                    {
                        torId = c.Int(nullable: false, identity: true),
                        torTipo = c.String(),
                        torImgUrl = c.String(),
                        torComentarios = c.Boolean(nullable: false),
                        torNombreTorneo = c.String(nullable: false, maxLength: 500),
                        ligId = c.Int(nullable: false),
                        lctId = c.Int(nullable: false),
                        torFechaInicio = c.DateTime(),
                        torFechaTermino = c.DateTime(),
                        torFechaLimiteInscripcion = c.DateTime(),
                        torNumeroJuegos = c.Int(),
                        torNumeroEquipos = c.Int(),
                        torMaxJugadoresEquipo = c.Int(),
                        torPuntosGanar = c.Decimal(precision: 18, scale: 2),
                        torPuntosEmpatar = c.Decimal(precision: 18, scale: 2),
                        torPuntosPerder = c.Decimal(precision: 18, scale: 2),
                        tesId = c.Int(nullable: false),
                        torNumeroContacto = c.String(),
                        torCorreoContacto = c.String(),
                        torPrecioTorneo = c.Decimal(precision: 18, scale: 2),
                        torDiasParaPago = c.Int(),
                        tcfpptpId = c.Int(),
                        torUserIdCreador = c.String(nullable: false, maxLength: 128),
                        torFechaCreacionUTC = c.DateTime(nullable: false),
                        torEstatus = c.Boolean(nullable: false),
                        torAprobada = c.Boolean(nullable: false),
                        torPagado = c.Boolean(nullable: false),
                        torLatitud = c.String(),
                        torLongitud = c.String(),
                        torEsCoaching = c.Boolean(nullable: false),
                        torDeporteEnEquipo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.torId)
                .ForeignKey("dbo.tblLigaCategoriasTorneos", t => t.lctId, cascadeDelete: true)
                .ForeignKey("dbo.tblLigas", t => t.ligId)
                .ForeignKey("dbo.tblTarifasCfppTiposPago", t => t.tcfpptpId)
                .ForeignKey("dbo.tblTorneoEstructura", t => t.tesId)
                .ForeignKey("dbo.AspNetUsers", t => t.torUserIdCreador)
                .Index(t => t.ligId)
                .Index(t => t.lctId)
                .Index(t => t.tesId)
                .Index(t => t.tcfpptpId)
                .Index(t => t.torUserIdCreador);
            
            CreateTable(
                "dbo.tblLigaCategoriasTorneos",
                c => new
                    {
                        lctId = c.Int(nullable: false, identity: true),
                        ligId = c.Int(nullable: false),
                        lctNombre = c.String(nullable: false, maxLength: 100),
                        depNombre = c.String(nullable: false, maxLength: 128),
                        ttoId = c.Int(nullable: false),
                        lctDescripcion = c.String(maxLength: 1024),
                        lctEdadMin = c.Int(),
                        lctEdadMax = c.Int(),
                        lctEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.lctId)
                .ForeignKey("dbo.tblDeportes", t => t.depNombre, cascadeDelete: true)
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .ForeignKey("dbo.tblTipoTorneo", t => t.ttoId, cascadeDelete: true)
                .Index(t => t.ligId)
                .Index(t => t.depNombre)
                .Index(t => t.ttoId);
            
            CreateTable(
                "dbo.tblDeportes",
                c => new
                    {
                        depNombre = c.String(nullable: false, maxLength: 128),
                        depIcono = c.String(nullable: false),
                        depPrioridad = c.Int(nullable: false),
                        depEstatus = c.Boolean(nullable: false),
                        depEnEquipo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.depNombre);
            
            CreateTable(
                "dbo.tblTipoTorneo",
                c => new
                    {
                        ttoId = c.Int(nullable: false, identity: true),
                        ttoNombre = c.String(nullable: false),
                        ttoEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ttoId);
            
            CreateTable(
                "dbo.tblTorneoDireccion",
                c => new
                    {
                        torId = c.Int(nullable: false),
                        ldcCalle = c.String(nullable: false, maxLength: 500),
                        ldcNumeroExtInt = c.String(nullable: false, maxLength: 100),
                        ldcColonia = c.String(nullable: false, maxLength: 100),
                        ldcMunicipio = c.String(nullable: false, maxLength: 100),
                        ldcEstado = c.String(nullable: false, maxLength: 100),
                        ldcCodigoPostal = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.torId)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .Index(t => t.torId);
            
            CreateTable(
                "dbo.tblTorneoEstructura",
                c => new
                    {
                        tscId = c.Int(nullable: false, identity: true),
                        tscNombre = c.String(),
                        tcsDescripcion = c.String(),
                        tcsEstatus = c.Boolean(nullable: false),
                        tcsDeporteEnEquipo = c.Boolean(nullable: false),
                        tcsOrden = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.tscId);
            
            CreateTable(
                "dbo.tblTorneoTarifas",
                c => new
                    {
                        torId = c.Int(nullable: false),
                        tarId = c.Int(nullable: false),
                        ttaHabilitado = c.Boolean(nullable: false),
                        schemaTorneos_torId = c.Int(),
                    })
                .PrimaryKey(t => new { t.torId, t.tarId })
                .ForeignKey("dbo.tblTarifas", t => t.tarId, cascadeDelete: true)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .ForeignKey("dbo.tblTorneos", t => t.schemaTorneos_torId)
                .Index(t => t.torId)
                .Index(t => t.tarId)
                .Index(t => t.schemaTorneos_torId);
            
            CreateTable(
                "dbo.tblCiudades",
                c => new
                    {
                        ciuId = c.Int(nullable: false, identity: true),
                        ciuNombre = c.String(nullable: false),
                        ciuEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ciuId);
            
            CreateTable(
                "dbo.tblDetallesPago",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdPago = c.Int(nullable: false),
                        conceptoPago = c.String(),
                        total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IdTransaccion = c.String(nullable: false),
                        referencia = c.String(),
                        metodoPago = c.String(),
                        status = c.String(nullable: false),
                        ipAddress = c.String(),
                        FechaCreacionUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblPagos", t => t.IdPago)
                .Index(t => t.IdPago);
            
            CreateTable(
                "dbo.tblPagos",
                c => new
                    {
                        IdPago = c.Int(nullable: false, identity: true),
                        userId = c.String(nullable: false, maxLength: 128),
                        concepto = c.String(),
                        conceptoId = c.Int(nullable: false),
                        IdTransaccion = c.String(),
                        pagoFechaCreacionUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdPago)
                .ForeignKey("dbo.AspNetUsers", t => t.userId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.tblEquipos",
                c => new
                    {
                        equId = c.Int(nullable: false, identity: true),
                        torId = c.Int(nullable: false),
                        equImgUrl = c.String(),
                        equNombreEquipo = c.String(nullable: false, maxLength: 100),
                        equFechaCreacionUTC = c.DateTime(nullable: false),
                        equEstatus = c.Boolean(nullable: false),
                        equFechaVencimientoPagoUTC = c.DateTime(),
                        equPagado = c.Boolean(nullable: false),
                        equPrecioTorneo = c.Decimal(precision: 18, scale: 2),
                        equUserIdCreador = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.equId)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .ForeignKey("dbo.AspNetUsers", t => t.equUserIdCreador)
                .Index(t => t.torId)
                .Index(t => t.equUserIdCreador);
            
            CreateTable(
                "dbo.tblJugadores",
                c => new
                    {
                        jugCorreo = c.String(nullable: false, maxLength: 250),
                        jugUserId = c.String(maxLength: 128),
                        jugNombre = c.String(),
                        jugFechaCreacionUTC = c.DateTime(nullable: false),
                        jugEstatus = c.Boolean(nullable: false),
                        schemaEquipos_equId = c.Int(),
                    })
                .PrimaryKey(t => t.jugCorreo)
                .ForeignKey("dbo.AspNetUsers", t => t.jugUserId)
                .ForeignKey("dbo.tblEquipos", t => t.schemaEquipos_equId)
                .Index(t => t.jugUserId)
                .Index(t => t.schemaEquipos_equId);
            
            CreateTable(
                "dbo.tblEquiposCoAdministradores",
                c => new
                    {
                        equId = c.Int(nullable: false),
                        ecaCorreoId = c.String(nullable: false, maxLength: 250),
                        equUserId = c.String(maxLength: 128),
                        equConfirmado = c.Boolean(nullable: false),
                        equCodigoConfirmacion = c.String(),
                        equFechaConfirmacionUTC = c.DateTime(),
                        equEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.equId, t.ecaCorreoId })
                .ForeignKey("dbo.tblEquipos", t => t.equId)
                .ForeignKey("dbo.AspNetUsers", t => t.equUserId)
                .Index(t => t.equId)
                .Index(t => t.equUserId);
            
            CreateTable(
                "dbo.tblJuegosFutbolEstadisticasJugador",
                c => new
                    {
                        parId = c.Int(nullable: false),
                        equId = c.Int(nullable: false),
                        UserIdJugador = c.String(nullable: false, maxLength: 128),
                        jfejGoles = c.Int(nullable: false),
                        jfejAsistencias = c.Int(nullable: false),
                        jfejFaltas = c.Int(nullable: false),
                        jfejTarjetasAmarillas = c.Int(nullable: false),
                        jfejTarjetasRojas = c.Int(nullable: false),
                        jfejPartidosSuspendidos = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.parId, t.equId, t.UserIdJugador })
                .ForeignKey("dbo.tblEquipos", t => t.equId, cascadeDelete: true)
                .ForeignKey("dbo.tblPartidos", t => t.parId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserIdJugador, cascadeDelete: true)
                .Index(t => t.parId)
                .Index(t => t.equId)
                .Index(t => t.UserIdJugador);
            
            CreateTable(
                "dbo.tblJugadorEquipos",
                c => new
                    {
                        torId = c.Int(nullable: false),
                        jugUserId = c.String(maxLength: 128),
                        jugFechaCreacionUTC = c.DateTime(nullable: false),
                        jugEstatus = c.Boolean(nullable: false),
                        equId = c.Int(),
                        jugCodigoConfirmacion = c.String(),
                        jugConfirmado = c.Boolean(nullable: false),
                        jugFechaVencimientoPagoUTC = c.DateTime(),
                        jugPagado = c.Boolean(nullable: false),
                        jugPrecioTorneo = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.torId)
                .ForeignKey("dbo.tblEquipos", t => t.equId)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .ForeignKey("dbo.AspNetUsers", t => t.jugUserId)
                .Index(t => t.torId)
                .Index(t => t.jugUserId)
                .Index(t => t.equId);
            
            CreateTable(
                "dbo.tblLigaCanchasTorneos",
                c => new
                    {
                        lcatId = c.Int(nullable: false, identity: true),
                        ligId = c.Int(nullable: false),
                        lcatNombre = c.String(nullable: false, maxLength: 100),
                        lcatdomicilio = c.String(),
                        lcatNumExtInt = c.String(),
                        lcatColonia = c.String(),
                        lcatMunicipio = c.String(),
                        lcatEstado = c.String(),
                        lcatCodigoPostal = c.String(),
                        lcatLatitud = c.String(),
                        lcatLongitud = c.String(),
                        lcatDescripcion = c.String(maxLength: 1024),
                        lcatEstatus = c.Boolean(nullable: false),
                        torFechaCreacionUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.lcatId)
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .Index(t => t.ligId);
            
            CreateTable(
                "dbo.tblLigaCategoriasTarifasFormasPago",
                c => new
                    {
                        lctfpId = c.Int(nullable: false, identity: true),
                        lcaId = c.String(maxLength: 20),
                        tfpIdFormaPago = c.String(maxLength: 15),
                        lctfpEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.lctfpId)
                .ForeignKey("dbo.tblLigaCategorias", t => t.lcaId)
                .ForeignKey("dbo.tblTarifasFormasPago", t => t.tfpIdFormaPago)
                .Index(t => t.lcaId)
                .Index(t => t.tfpIdFormaPago);
            
            CreateTable(
                "dbo.tblLigaCoAdministradores",
                c => new
                    {
                        ligId = c.Int(nullable: false),
                        lcaUserId = c.String(nullable: false, maxLength: 128),
                        lcaConfirmacion = c.Boolean(nullable: false),
                        lcaCodigoConfirmacion = c.String(),
                        lcaFechaConfirmacionUTC = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.ligId, t.lcaUserId })
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.lcaUserId)
                .Index(t => t.ligId)
                .Index(t => t.lcaUserId);
            
            CreateTable(
                "dbo.tblLigaDatosFiscales",
                c => new
                    {
                        ldfId = c.Int(nullable: false),
                        ligId = c.Int(nullable: false),
                        ldfRFC = c.String(nullable: false, maxLength: 14),
                        ldfRazonSocial = c.String(nullable: false, maxLength: 250),
                        ldfDomicilio = c.String(nullable: false, maxLength: 500),
                        ldfNumeroExtInt = c.String(nullable: false, maxLength: 100),
                        ldfColonia = c.String(nullable: false, maxLength: 100),
                        ldfMunicipio = c.String(nullable: false, maxLength: 100),
                        ldfEstado = c.String(nullable: false, maxLength: 100),
                        ldfCodigoPostal = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => new { t.ldfId, t.ligId })
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .Index(t => t.ligId);
            
            CreateTable(
                "dbo.tblLigaDireccionComercial",
                c => new
                    {
                        ldcId = c.Int(nullable: false),
                        ligId = c.Int(nullable: false),
                        ldcDomicilio = c.String(nullable: false, maxLength: 500),
                        ldcNumeroExtInt = c.String(nullable: false, maxLength: 100),
                        ldcColonia = c.String(nullable: false, maxLength: 100),
                        ldcMunicipio = c.String(nullable: false, maxLength: 100),
                        ldcEstado = c.String(nullable: false, maxLength: 100),
                        ldcCodigoPostal = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => new { t.ldcId, t.ligId })
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .Index(t => t.ligId);
            
            CreateTable(
                "dbo.tblLigaPrincipalUsuario",
                c => new
                    {
                        roleId = c.String(nullable: false, maxLength: 128),
                        userId = c.String(nullable: false, maxLength: 128),
                        ligId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.roleId, t.userId })
                .ForeignKey("dbo.tblLigas", t => t.ligId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.roleId)
                .ForeignKey("dbo.AspNetUsers", t => t.userId)
                .Index(t => t.roleId)
                .Index(t => t.userId)
                .Index(t => t.ligId);
            
            CreateTable(
                "dbo.tblMenus",
                c => new
                    {
                        menId = c.String(nullable: false, maxLength: 255),
                        rolId = c.String(nullable: false, maxLength: 128),
                        menNombre = c.String(nullable: false, maxLength: 128),
                        menAction = c.String(maxLength: 128),
                        menController = c.String(maxLength: 128),
                        menOrden = c.Int(nullable: false),
                        menEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.menId)
                .ForeignKey("dbo.AspNetRoles", t => t.rolId, cascadeDelete: true)
                .Index(t => t.rolId);
            
            CreateTable(
                "dbo.tblSiteConfigs",
                c => new
                    {
                        scoId = c.Int(nullable: false, identity: true),
                        scoAppName = c.String(nullable: false),
                        scoCompanyName = c.String(nullable: false),
                        scoSenderEmail = c.String(nullable: false),
                        scoSenderDisplayEmailName = c.String(nullable: false),
                        scoSenderEmailPassword = c.String(nullable: false),
                        scoSenderSMTPServer = c.String(nullable: false),
                        scoSenderPort = c.String(nullable: false),
                        scoContactEmails = c.String(),
                    })
                .PrimaryKey(t => t.scoId);
            
            CreateTable(
                "dbo.tblSubMenus",
                c => new
                    {
                        smeId = c.String(nullable: false, maxLength: 255),
                        menId = c.String(nullable: false, maxLength: 255),
                        smeNombre = c.String(nullable: false, maxLength: 128),
                        smeAction = c.String(maxLength: 128),
                        smeController = c.String(maxLength: 128),
                        smeOrden = c.Int(nullable: false),
                        smeEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.smeId)
                .ForeignKey("dbo.tblMenus", t => t.menId, cascadeDelete: true)
                .Index(t => t.menId);
            
            CreateTable(
                "dbo.tblTorneoCoAdministradores",
                c => new
                    {
                        torId = c.Int(nullable: false),
                        tcaUserId = c.String(nullable: false, maxLength: 128),
                        tcaConfirmacion = c.Boolean(nullable: false),
                        tcaCodigoConfirmacion = c.String(),
                        tcaFechaConfirmacionUTC = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.torId, t.tcaUserId })
                .ForeignKey("dbo.tblTorneos", t => t.torId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.tcaUserId)
                .Index(t => t.torId)
                .Index(t => t.tcaUserId);
            
            CreateTable(
                "dbo.tblTorneoComentarios",
                c => new
                    {
                        torId = c.Int(nullable: false),
                        tcoId = c.Int(nullable: false),
                        tcoUserIdComenta = c.String(nullable: false, maxLength: 128),
                        equId = c.Int(),
                        tcoCalificacion = c.Decimal(nullable: false, precision: 18, scale: 2),
                        tcoComentario = c.String(maxLength: 1000),
                        tcoFechaComentarioUTC = c.DateTime(nullable: false),
                        tcoEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.torId, t.tcoId })
                .ForeignKey("dbo.tblEquipos", t => t.equId)
                .ForeignKey("dbo.tblTorneos", t => t.torId)
                .ForeignKey("dbo.AspNetUsers", t => t.tcoUserIdComenta)
                .Index(t => t.torId)
                .Index(t => t.tcoUserIdComenta)
                .Index(t => t.equId);
            
            CreateTable(
                "dbo.tblUsersMisCuentas",
                c => new
                    {
                        userIdAdmin = c.String(nullable: false, maxLength: 128),
                        userId = c.String(nullable: false, maxLength: 128),
                        umcCuentasFusionadas = c.Boolean(nullable: false),
                        umcCuentaAdministrada = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.userIdAdmin, t.userId })
                .ForeignKey("dbo.AspNetUsers", t => t.userId)
                .ForeignKey("dbo.AspNetUsers", t => t.userIdAdmin)
                .Index(t => t.userIdAdmin)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.tblUsersProfiles",
                c => new
                    {
                        userIdOwner = c.String(nullable: false, maxLength: 128),
                        uprId = c.Int(nullable: false),
                        uprNombres = c.String(nullable: false, maxLength: 250),
                        uprApellidos = c.String(maxLength: 250),
                        uprGenero = c.String(maxLength: 1),
                        uprFechaNacimiento = c.DateTime(),
                        uprPais = c.String(maxLength: 50),
                        uprCiudad = c.String(maxLength: 50),
                        uprEstado = c.String(maxLength: 50),
                        uprTelefono = c.String(maxLength: 50),
                        cp = c.Int(nullable: false),
                        direccion = c.String(),
                        uprProfileImageURL = c.String(maxLength: 1024),
                        uprPerfilPrincipal = c.Boolean(nullable: false),
                        uprSubPerfil = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.userIdOwner, t.uprId })
                .ForeignKey("dbo.AspNetUsers", t => t.userIdOwner)
                .Index(t => t.userIdOwner);
            
            CreateTable(
                "dbo.tblZonas",
                c => new
                    {
                        zonId = c.Int(nullable: false, identity: true),
                        zonZona = c.String(nullable: false),
                        zonEstatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.zonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblUsersProfiles", "userIdOwner", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblUsersMisCuentas", "userIdAdmin", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblUsersMisCuentas", "userId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblTorneoComentarios", "tcoUserIdComenta", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblTorneoComentarios", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblTorneoComentarios", "equId", "dbo.tblEquipos");
            DropForeignKey("dbo.tblTorneoCoAdministradores", "tcaUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblTorneoCoAdministradores", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblSubMenus", "menId", "dbo.tblMenus");
            DropForeignKey("dbo.tblMenus", "rolId", "dbo.AspNetRoles");
            DropForeignKey("dbo.tblLigaPrincipalUsuario", "userId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblLigaPrincipalUsuario", "roleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.tblLigaPrincipalUsuario", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigaDireccionComercial", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigaDatosFiscales", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigaCoAdministradores", "lcaUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblLigaCoAdministradores", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigaCategoriasTarifasFormasPago", "tfpIdFormaPago", "dbo.tblTarifasFormasPago");
            DropForeignKey("dbo.tblLigaCategoriasTarifasFormasPago", "lcaId", "dbo.tblLigaCategorias");
            DropForeignKey("dbo.tblLigaCanchasTorneos", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblJugadorEquipos", "jugUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblJugadorEquipos", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblJugadorEquipos", "equId", "dbo.tblEquipos");
            DropForeignKey("dbo.tblJuegosFutbolEstadisticasJugador", "UserIdJugador", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblJuegosFutbolEstadisticasJugador", "parId", "dbo.tblPartidos");
            DropForeignKey("dbo.tblJuegosFutbolEstadisticasJugador", "equId", "dbo.tblEquipos");
            DropForeignKey("dbo.tblEquiposCoAdministradores", "equUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblEquiposCoAdministradores", "equId", "dbo.tblEquipos");
            DropForeignKey("dbo.tblEquipos", "equUserIdCreador", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblEquipos", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblJugadores", "schemaEquipos_equId", "dbo.tblEquipos");
            DropForeignKey("dbo.tblJugadores", "jugUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblDetallesPago", "IdPago", "dbo.tblPagos");
            DropForeignKey("dbo.tblPagos", "userId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblArbitrosPartidos", "parId", "dbo.tblPartidos");
            DropForeignKey("dbo.tblPartidos", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblTorneos", "torUserIdCreador", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblTorneoTarifas", "schemaTorneos_torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblTorneoTarifas", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblTorneoTarifas", "tarId", "dbo.tblTarifas");
            DropForeignKey("dbo.tblTorneos", "tesId", "dbo.tblTorneoEstructura");
            DropForeignKey("dbo.tblTorneoDireccion", "torId", "dbo.tblTorneos");
            DropForeignKey("dbo.tblTorneos", "tcfpptpId", "dbo.tblTarifasCfppTiposPago");
            DropForeignKey("dbo.tblTorneos", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblTorneos", "lctId", "dbo.tblLigaCategoriasTorneos");
            DropForeignKey("dbo.tblLigaCategoriasTorneos", "ttoId", "dbo.tblTipoTorneo");
            DropForeignKey("dbo.tblLigaCategoriasTorneos", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigaCategoriasTorneos", "depNombre", "dbo.tblDeportes");
            DropForeignKey("dbo.tblPartidos", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblArbitrosPartidos", "arbId", "dbo.tblArbitros");
            DropForeignKey("dbo.tblArbitrosLigas", "ligId", "dbo.tblLigas");
            DropForeignKey("dbo.tblLigas", "ligUserIdCreador", "dbo.AspNetUsers");
            DropForeignKey("dbo.tblLigas", "tcfppId", "dbo.tblTarifasCfpPeriodicidades");
            DropForeignKey("dbo.tblLigas", "tarId", "dbo.tblTarifas");
            DropForeignKey("dbo.tblTarifas", "tcfpptpmpId", "dbo.tblTarifasCfpptpMetodosPago");
            DropForeignKey("dbo.tblTarifasCfpptpMetodosPago", "tmpIdMetodoPago", "dbo.tblTarifasMetodosPago");
            DropForeignKey("dbo.tblTarifasCfpptpMetodosPago", "tcfpptpId", "dbo.tblTarifasCfppTiposPago");
            DropForeignKey("dbo.tblTarifasCfppTiposPago", "ttpIdTipoPago", "dbo.tblTarifasTiposPago");
            DropForeignKey("dbo.tblTarifasCfppTiposPago", "tcfppId", "dbo.tblTarifasCfpPeriodicidades");
            DropForeignKey("dbo.tblTarifasCfpPeriodicidades", "tpeIdPeriodicidad", "dbo.tblTarifasPeriodicidades");
            DropForeignKey("dbo.tblTarifasCfpPeriodicidades", "tcfpId", "dbo.tblTarifasConceptosFormasPago");
            DropForeignKey("dbo.tblTarifasConceptosFormasPago", "tfpIdFormaPago", "dbo.tblTarifasFormasPago");
            DropForeignKey("dbo.tblTarifasConceptosFormasPago", "tcoIdConcepto", "dbo.tblTarifasConceptos");
            DropForeignKey("dbo.tblLigas", "ligTipoLiga", "dbo.tblLigaCategorias");
            DropForeignKey("dbo.tblArbitrosLigas", "arbId", "dbo.tblArbitros");
            DropForeignKey("dbo.tblArbitros", "arbUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.tblUsersProfiles", new[] { "userIdOwner" });
            DropIndex("dbo.tblUsersMisCuentas", new[] { "userId" });
            DropIndex("dbo.tblUsersMisCuentas", new[] { "userIdAdmin" });
            DropIndex("dbo.tblTorneoComentarios", new[] { "equId" });
            DropIndex("dbo.tblTorneoComentarios", new[] { "tcoUserIdComenta" });
            DropIndex("dbo.tblTorneoComentarios", new[] { "torId" });
            DropIndex("dbo.tblTorneoCoAdministradores", new[] { "tcaUserId" });
            DropIndex("dbo.tblTorneoCoAdministradores", new[] { "torId" });
            DropIndex("dbo.tblSubMenus", new[] { "menId" });
            DropIndex("dbo.tblMenus", new[] { "rolId" });
            DropIndex("dbo.tblLigaPrincipalUsuario", new[] { "ligId" });
            DropIndex("dbo.tblLigaPrincipalUsuario", new[] { "userId" });
            DropIndex("dbo.tblLigaPrincipalUsuario", new[] { "roleId" });
            DropIndex("dbo.tblLigaDireccionComercial", new[] { "ligId" });
            DropIndex("dbo.tblLigaDatosFiscales", new[] { "ligId" });
            DropIndex("dbo.tblLigaCoAdministradores", new[] { "lcaUserId" });
            DropIndex("dbo.tblLigaCoAdministradores", new[] { "ligId" });
            DropIndex("dbo.tblLigaCategoriasTarifasFormasPago", new[] { "tfpIdFormaPago" });
            DropIndex("dbo.tblLigaCategoriasTarifasFormasPago", new[] { "lcaId" });
            DropIndex("dbo.tblLigaCanchasTorneos", new[] { "ligId" });
            DropIndex("dbo.tblJugadorEquipos", new[] { "equId" });
            DropIndex("dbo.tblJugadorEquipos", new[] { "jugUserId" });
            DropIndex("dbo.tblJugadorEquipos", new[] { "torId" });
            DropIndex("dbo.tblJuegosFutbolEstadisticasJugador", new[] { "UserIdJugador" });
            DropIndex("dbo.tblJuegosFutbolEstadisticasJugador", new[] { "equId" });
            DropIndex("dbo.tblJuegosFutbolEstadisticasJugador", new[] { "parId" });
            DropIndex("dbo.tblEquiposCoAdministradores", new[] { "equUserId" });
            DropIndex("dbo.tblEquiposCoAdministradores", new[] { "equId" });
            DropIndex("dbo.tblJugadores", new[] { "schemaEquipos_equId" });
            DropIndex("dbo.tblJugadores", new[] { "jugUserId" });
            DropIndex("dbo.tblEquipos", new[] { "equUserIdCreador" });
            DropIndex("dbo.tblEquipos", new[] { "torId" });
            DropIndex("dbo.tblPagos", new[] { "userId" });
            DropIndex("dbo.tblDetallesPago", new[] { "IdPago" });
            DropIndex("dbo.tblTorneoTarifas", new[] { "schemaTorneos_torId" });
            DropIndex("dbo.tblTorneoTarifas", new[] { "tarId" });
            DropIndex("dbo.tblTorneoTarifas", new[] { "torId" });
            DropIndex("dbo.tblTorneoDireccion", new[] { "torId" });
            DropIndex("dbo.tblLigaCategoriasTorneos", new[] { "ttoId" });
            DropIndex("dbo.tblLigaCategoriasTorneos", new[] { "depNombre" });
            DropIndex("dbo.tblLigaCategoriasTorneos", new[] { "ligId" });
            DropIndex("dbo.tblTorneos", new[] { "torUserIdCreador" });
            DropIndex("dbo.tblTorneos", new[] { "tcfpptpId" });
            DropIndex("dbo.tblTorneos", new[] { "tesId" });
            DropIndex("dbo.tblTorneos", new[] { "lctId" });
            DropIndex("dbo.tblTorneos", new[] { "ligId" });
            DropIndex("dbo.tblPartidos", new[] { "torId" });
            DropIndex("dbo.tblPartidos", new[] { "ligId" });
            DropIndex("dbo.tblArbitrosPartidos", new[] { "parId" });
            DropIndex("dbo.tblArbitrosPartidos", new[] { "arbId" });
            DropIndex("dbo.tblTarifasConceptosFormasPago", new[] { "tfpIdFormaPago" });
            DropIndex("dbo.tblTarifasConceptosFormasPago", new[] { "tcoIdConcepto" });
            DropIndex("dbo.tblTarifasCfpPeriodicidades", new[] { "tpeIdPeriodicidad" });
            DropIndex("dbo.tblTarifasCfpPeriodicidades", new[] { "tcfpId" });
            DropIndex("dbo.tblTarifasCfppTiposPago", new[] { "ttpIdTipoPago" });
            DropIndex("dbo.tblTarifasCfppTiposPago", new[] { "tcfppId" });
            DropIndex("dbo.tblTarifasCfpptpMetodosPago", new[] { "tmpIdMetodoPago" });
            DropIndex("dbo.tblTarifasCfpptpMetodosPago", new[] { "tcfpptpId" });
            DropIndex("dbo.tblTarifas", new[] { "tcfpptpmpId" });
            DropIndex("dbo.tblLigas", new[] { "tarId" });
            DropIndex("dbo.tblLigas", new[] { "tcfppId" });
            DropIndex("dbo.tblLigas", new[] { "ligTipoLiga" });
            DropIndex("dbo.tblLigas", new[] { "ligUserIdCreador" });
            DropIndex("dbo.tblArbitrosLigas", new[] { "ligId" });
            DropIndex("dbo.tblArbitrosLigas", new[] { "arbId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.tblArbitros", new[] { "arbUserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.tblZonas");
            DropTable("dbo.tblUsersProfiles");
            DropTable("dbo.tblUsersMisCuentas");
            DropTable("dbo.tblTorneoComentarios");
            DropTable("dbo.tblTorneoCoAdministradores");
            DropTable("dbo.tblSubMenus");
            DropTable("dbo.tblSiteConfigs");
            DropTable("dbo.tblMenus");
            DropTable("dbo.tblLigaPrincipalUsuario");
            DropTable("dbo.tblLigaDireccionComercial");
            DropTable("dbo.tblLigaDatosFiscales");
            DropTable("dbo.tblLigaCoAdministradores");
            DropTable("dbo.tblLigaCategoriasTarifasFormasPago");
            DropTable("dbo.tblLigaCanchasTorneos");
            DropTable("dbo.tblJugadorEquipos");
            DropTable("dbo.tblJuegosFutbolEstadisticasJugador");
            DropTable("dbo.tblEquiposCoAdministradores");
            DropTable("dbo.tblJugadores");
            DropTable("dbo.tblEquipos");
            DropTable("dbo.tblPagos");
            DropTable("dbo.tblDetallesPago");
            DropTable("dbo.tblCiudades");
            DropTable("dbo.tblTorneoTarifas");
            DropTable("dbo.tblTorneoEstructura");
            DropTable("dbo.tblTorneoDireccion");
            DropTable("dbo.tblTipoTorneo");
            DropTable("dbo.tblDeportes");
            DropTable("dbo.tblLigaCategoriasTorneos");
            DropTable("dbo.tblTorneos");
            DropTable("dbo.tblPartidos");
            DropTable("dbo.tblArbitrosPartidos");
            DropTable("dbo.tblTarifasMetodosPago");
            DropTable("dbo.tblTarifasTiposPago");
            DropTable("dbo.tblTarifasPeriodicidades");
            DropTable("dbo.tblTarifasFormasPago");
            DropTable("dbo.tblTarifasConceptos");
            DropTable("dbo.tblTarifasConceptosFormasPago");
            DropTable("dbo.tblTarifasCfpPeriodicidades");
            DropTable("dbo.tblTarifasCfppTiposPago");
            DropTable("dbo.tblTarifasCfpptpMetodosPago");
            DropTable("dbo.tblTarifas");
            DropTable("dbo.tblLigaCategorias");
            DropTable("dbo.tblLigas");
            DropTable("dbo.tblArbitrosLigas");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.tblArbitros");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}
