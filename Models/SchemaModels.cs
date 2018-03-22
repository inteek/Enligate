using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using sw_EnligateWeb.Models.HelperClasses;
using System.Threading.Tasks;
using System.Security.Claims;

namespace sw_EnligateWeb.Models
{

    [Table("tblReporteBugs")]
    public class schemaReporteBugs
    {
        public schemaReporteBugs()
        {
            repoFechaUTC = DateTime.Now;
        }

        [Key]
        public int repoId { get; set; }

        public string correoUsuario { get; set; }

        public string reporte { get; set; }

        public string exception { get; set; }

        public string ipAddress { get; set; }
        public string browser { get; set; }                
        public DateTime repoFechaUTC { get; set; }
    }
    [Table("tblLoginHistory")]
    public class schemaLoginHistory
    {
        public schemaLoginHistory()
        {
            loginDate = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }
        public string correoUsuario { get; set; }
        public string exception { get; set; }
        public string ipAddress { get; set; }
        public DateTime loginDate { get; set; }
    }
    [Table("tblDeportes")]
    public class schemaDeportes
    {
        public schemaDeportes()
        {
            depEstatus = true;
        }

        [Key()]
        [Required(ErrorMessage = "Favor de ingresar el nombre.")]
        [Display(Name = "Deporte")]
        public string depNombre { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el nombre del archivo del icono (futbol.png).")]
        [Display(Name = "Icono")]
        public string depIcono { get; set; }

        [Required]
        [Display(Name = "Prioridad")]
        public int depPrioridad { get; set; }

        [Required]
        public bool depEstatus { get; set; }

        public bool depEnEquipo { get; set; }

        //[Required(ErrorMessage = "Ingresa el color de fondo")]
        //[Display(Name = "Color (RGB)")]
        //public string depIcono { get; set; }
    }

    [Table("tblTipoTorneo")]
    public class schemaTipoTorneos
    {
        public schemaTipoTorneos()
        {
            ttoEstatus = true;
        }

        [Key, Required]
        public int ttoId { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el tipo de torneo.")]
        [Display(Name = "Tipo de torneo")]
        public string ttoNombre { get; set; }

        [Required]
        public bool ttoEstatus { get; set; }

        //[Required(ErrorMessage = "Ingresa el color de fondo")]
        //[Display(Name = "Color (RGB)")]
        //public string depIcono { get; set; }
    }

    [Table("tblCiudades")]
    public class schemaCiudades
    {
        public schemaCiudades()
        {
            ciuEstatus = true;
        }

        [Key, Required]
        public int ciuId { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la ciudad.")]
        [Display(Name = "Ciudad")]
        public string ciuNombre { get; set; }

        [Required]
        public bool ciuEstatus { get; set; }

        //[Required(ErrorMessage = "Ingresa el color de fondo")]
        //[Display(Name = "Color (RGB)")]
        //public string depIcono { get; set; }
    }

    [Table("tblZonas")]
    public class schemaZonas
    {
        public schemaZonas()
        {
            zonEstatus = true;
        }

        [Key, Required]
        public int zonId { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la zona.")]
        [Display(Name = "Zona")]
        public string zonZona { get; set; }

        [Required]
        public bool zonEstatus { get; set; }

        //[Required(ErrorMessage = "Ingresa el color de fondo")]
        //[Display(Name = "Color (RGB)")]
        //public string depIcono { get; set; }
    }

    [Table("tblSiteConfigs")]
    public class schemaSiteConfigs
    {
        [Key, Required]
        public int scoId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Nombre de la aplicación")]
        public string scoAppName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Nombre de la empresa")]
        public string scoCompanyName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        [Display(Name = "Correo")]
        public string scoSenderEmail { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Nombre del correo")]
        public string scoSenderDisplayEmailName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Contraseña")]
        public string scoSenderEmailPassword { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Servidor SMTP")]
        public string scoSenderSMTPServer { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Puerto")]
        public string scoSenderPort { get; set; }

        [Display(Name = "Recibir correo contacto")]
        public string scoContactEmails { get; set; }
    }

    #region Usuarios / Perfiles Usuario / Subcuentas

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            usuCuentaPrincipal = true;
            usuEstatus = true;
            created_at = DateTime.Now;
        }

        [Required]
        public bool usuEstatus { get; set; }

        public string usuEmailValidationCode { get; set; }

        public DateTime? usuEmailValidationCodeEndDateUtc { get; set; }

        public string usuPasswordRecoveryCode { get; set; }

        public DateTime? usuPasswordRecoveryCodeEndDateUtc { get; set; }

        [MaxLength(50)]
        public string usuRolActual { get; set; }

        /// <summary>
        /// Cuando se fusionan las cuentas se indica si es la cuenta principal de todas las que tiene el usuario.
        /// </summary>
        public bool usuCuentaPrincipal { get; set; }
        public DateTime created_at { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }



        //[Required]
        //[MaxLength(50)]
        //public string usuNombre { get; set; }

        //[Required]
        //[MaxLength(50)]
        //public string usuApellido { get; set; }

        //[MaxLength(1)]
        //public string usuGenero { get; set; }

        //public DateTime? usuFechaNacimiento { get; set; }

        //[MaxLength(50)]
        //public string usuCiudad { get; set; }

        //public string usuProfileImageURL { get; set; }

    }

    [Table("tblUsersProfiles")]
    public class schemaUsersProfiles
    {
        public schemaUsersProfiles()
        {
            uprNombres = "-";
            uprApellidos = "-";
        }

        [Key, Column(Order = 0), Required]
        public string userIdOwner { get; set; }

        [ForeignKey("userIdOwner")]
        public virtual ApplicationUser tblUsers { get; set; }

        [Key, Column(Order = 1)]
        public int uprId { get; set; }

        //[EmailAddress, MaxLength(250)]
        //public string uprCorreo { get; set; }

        [MaxLength(250), Required]
        public string uprNombres { get; set; }

        [MaxLength(250)]
        public string uprApellidos { get; set; }

        [MaxLength(1)]
        public string uprGenero { get; set; }

        public DateTime? uprFechaNacimiento { get; set; }

        [MaxLength(50)]
        public string uprPais { get; set; }

        public string codeIdPais { get; set; }

        [MaxLength(50)]
        public string uprCiudad { get; set; }
        public string codeIdCiudad { get; set; }

        [MaxLength(50)]
        public string uprEstado { get; set; }
        public string codeIdEstado { get; set; }

        [MaxLength(50)]
        public string uprTelefono { get; set; }

        public int cp { get; set; }

        public string direccion { get; set; }

        [MaxLength(1024)]
        public string uprProfileImageURL { get; set; }

        public bool uprPerfilPrincipal { get; set; }

        public bool uprSubPerfil { get; set; }

    }

    [Table("tblUsersMisCuentas")]
    public class schemaUsersMisCuentas
    {
        /// <summary>
        /// Es la cuenta que administra a la secundaria
        /// </summary>
        [Key, Column(Order = 0), Required]
        public string userIdAdmin { get; set; }

        [ForeignKey("userIdAdmin")]
        public virtual ApplicationUser tblUserAdmin { get; set; }

        [Key, Column(Order = 1)]
        public string userId { get; set; }

        [ForeignKey("userId")]
        public virtual ApplicationUser tblUser { get; set; }

        public bool umcCuentasFusionadas { get; set; }

        public bool umcCuentaAdministrada { get; set; }
        public bool activo { get; set; }

    }

    #endregion

    #region Menus

    [Table("tblMenus")]
    public class schemaMenus
    {
        public schemaMenus()
        {
            menEstatus = true;
        }

        [Key, MaxLength(255)]
        public string menId { get; set; }

        [Required, MaxLength(128)]
        public string rolId { get; set; }

        [ForeignKey("rolId")]
        public IdentityRole roleIdentity { get; set; }

        [Required, MaxLength(128)]
        public string menNombre { get; set; }

        [MaxLength(128)]
        public string menAction { get; set; }

        [MaxLength(128)]
        public string menController { get; set; }

        [Required]
        public int menOrden { get; set; }

        [Required]
        public bool menEstatus { get; set; }
    }

    [Table("tblSubMenus")]
    public class schemaSubMenus
    {
        public schemaSubMenus()
        {
            smeEstatus = true;
        }

        [Key, MaxLength(255)]
        public string smeId { get; set; }

        [Required, MaxLength(128)]
        public string menId { get; set; }

        [ForeignKey("menId")]
        public schemaMenus Menu { get; set; }

        [Required, MaxLength(128)]
        public string smeNombre { get; set; }

        [MaxLength(128)]
        public string smeAction { get; set; }

        [MaxLength(128)]
        public string smeController { get; set; }

        [Required]
        public int smeOrden { get; set; }

        [Required]
        public bool smeEstatus { get; set; }
    }

    #endregion

    #region Ligas

    [Table("tblLigaCategorias")]
    public class schemaLigaCategorias
    {
        public schemaLigaCategorias()
        {
            lcaEstatus = true;
        }

        [Key, Required]
        [MaxLength(20)]
        public string lcaId { get; set; }

        [MaxLength(20)]
        public string lcaCategoria { get; set; }

        [MaxLength(1024)]
        public string lcaDescripcion { get; set; }

        public bool lcaEstatus { get; set; }

        public int lcaOrden { get; set; }
    }

    [Table("tblLigaCategoriasTarifasFormasPago")]
    public class schemaLigaCategoriasTarifasFormasPago
    {
        public schemaLigaCategoriasTarifasFormasPago()
        {
            lctfpEstatus = true;
        }

        [Key, Required]
        public int lctfpId { get; set; }

        public string lcaId { get; set; }

        [ForeignKey("lcaId")]
        public virtual schemaLigaCategorias tblLigaCategorias { get; set; }

        public string tfpIdFormaPago { get; set; }

        [ForeignKey("tfpIdFormaPago")]
        public virtual schemaTarifasFormasPago tblTarifasFormasPago { get; set; }

        public bool lctfpEstatus { get; set; }
    }

    [Table("tblLigas")]
    public class schemaLigas
    {
        public schemaLigas()
        {
            ligFechaRegistroUTC = DateTime.Now;
            ligEstatus = true;
            ligAprobada = false;
            ligSolicitud = true;
            ligSolicitudRevisada = false;
            ligNotificacion = true;
        }

        [Key, Required]
        public int ligId { get; set; }

        [Required, MaxLength(128)]
        public string ligUserIdCreador { get; set; }

        [ForeignKey("ligUserIdCreador")]
        public virtual ApplicationUser tblUserCreador { get; set; }

        [Required]
        public DateTime ligFechaRegistroUTC { get; set; }

        [Required]
        public string ligTipoLiga { get; set; }

        [ForeignKey("ligTipoLiga")]
        public virtual schemaLigaCategorias tblLigaCategorias { get; set; }

        //[Required]
        //public int tcfppId { get; set; }

        //[ForeignKey("tcfppId")]
        //public virtual schemaTarifasCfpPeriodicidades tblTarifasPeriodicidad { get; set; }

        public string ligImgUrl { get; set; }

        [Required, MaxLength(500)]
        public string ligNombreLiga { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string ligCorreoContacto { get; set; }

        [Required, Display, MaxLength(30)]
        public string ligTelefonoContacto { get; set; }

        [Required, MaxLength(3000)]
        public string ligDescripcion { get; set; }

        [Required]
        public bool ligEstatus { get; set; }


        [Required]
        public bool ligAprobada { get; set; }

        [Required]
        public bool ligSolicitud { get; set; }

        [Required]
        public bool ligSolicitudRevisada { get; set; }

        [Required]
        public bool ligNotificacion { get; set; }

        public int? tarId { get; set; }

        [ForeignKey("tarId")]
        public virtual schemaTarifas tblTarifa { get; set; }

        public decimal ligPorcentajeDescuento { get; set; }

        public decimal ligTotalPagar { get; set; }

        public string statusPago { get; set; }

        public string ligLatitud { get; set; }

        public string ligLongitud { get; set; }

    }

    
    [Table("tblLigaCoAdmnInit")]
    public class schemaLigaCoAdminInit
    {
        public schemaLigaCoAdminInit()
        {
            lcaConfirmacion = false;
        }
        [Key]
        public int lcaint { get; set; }
        [Required, Column(Order = 0)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }
        public bool lcaConfirmacion { get; set; }

        public string userEmail { get; set; }
        
        public string lcaCodigoConfirmacion { get; set; }

        public DateTime? lcaFechaConfirmacionUTC { get; set; }
    }

    [Table("tblLigaCoAdministradores")]
    public class schemaLigaCoAdministradores
    {
        public schemaLigaCoAdministradores()
        {
            lcaConfirmacion = false;
        }

        [Key, Required, Column(Order = 0)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Key, Required, MaxLength(128), Column(Order = 1)]
        public string lcaUserId { get; set; }

        [ForeignKey("lcaUserId")]
        public virtual ApplicationUser tblUsuario { get; set; }

        public bool lcaConfirmacion { get; set; }

        public string lcaCodigoConfirmacion { get; set; }

        public DateTime? lcaFechaConfirmacionUTC { get; set; }
    }

    [Table("tblLigaDatosFiscales")]
    public class schemaLigaDatosFiscales
    {
        [Key, Required, Column(Order = 0)]
        public int ldfId { get; set; }

        [Key, Required, Column(Order = 1)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Required, MaxLength(14)]
        public string ldfRFC { get; set; }

        [Required, MaxLength(250)]
        public string ldfRazonSocial { get; set; }

        [Required, MaxLength(500)]
        public string ldfDomicilio { get; set; }

        [Required, MaxLength(100)]
        public string ldfNumeroExtInt { get; set; }

        [Required, MaxLength(100)]
        public string ldfColonia { get; set; }

        [Required, MaxLength(100)]
        public string ldfMunicipio { get; set; }

        [Required, MaxLength(100)]
        public string ldfEstado { get; set; }

        [Required, MaxLength(10)]
        public string ldfCodigoPostal { get; set; }
    }

    [Table("tblLigaDireccionComercial")]
    public class schemaLigaDireccionComercial
    {
        [Key, Required, Column(Order = 0)]
        public int ldcId { get; set; }

        [Key, Required, Column(Order = 1)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Required, MaxLength(500)]
        public string ldcDomicilio { get; set; }

        [Required, MaxLength(100)]
        public string ldcNumeroExtInt { get; set; }

        [Required, MaxLength(100)]
        public string ldcColonia { get; set; }

        [Required, MaxLength(100)]
        public string ldcMunicipio { get; set; }

        [Required, MaxLength(100)]
        public string ldcEstado { get; set; }

        [Required, MaxLength(10)]
        public string ldcCodigoPostal { get; set; }

    }

    [Table("tblLigaPrincipalUsuario")]
    public class schemaLigaPrincipalUsuario
    {
        [Key, Required, Column(Order = 0)]
        public string roleId { get; set; }

        [ForeignKey("roleId")]
        public virtual IdentityRole tblRole { get; set; }

        [Key, Required, Column(Order = 1)]
        public string userId { get; set; }

        [ForeignKey("userId")]
        public virtual ApplicationUser tblUser { get; set; }

        [Required]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLiga { get; set; }
    }

    #endregion

    #region Tarifas

    [Table("tblTarifas")]
    public class schemaTarifas
    {
        public schemaTarifas()
        {
            tarFechaRegistroUTC = DateTime.Now;
            tarEsPorcentaje = false;
            tarEstatus = true;
        }

        [Key]
        public int tarId { get; set; }

        [Required]
        public int tcfpptpmpId { get; set; }

        [ForeignKey("tcfpptpmpId")]
        public virtual schemaTarifasCfpptpMetodosPago tblTarifasCfpptpMetodoPago { get; set; }

        [Required]
        public decimal tarCosto { get; set; }

        [Required]
        public DateTime tarFechaRegistroUTC { get; set; }

        [Required]
        public bool tarEsPorcentaje { get; set; }

        [Required]
        public bool tarEstatus { get; set; }

    }

    [Table("tblTarifasConceptos")]
    public class schemaTarifasConceptos
    {
        public schemaTarifasConceptos()
        {
            tcoEstatus = true;
        }

        [Key, MaxLength(15)]
        public string tcoIdConcepto { get; set; }

        [Required]
        public bool tcoEstatus { get; set; }
    }

    [Table("tblTarifasFormasPago")]
    public class schemaTarifasFormasPago
    {
        public schemaTarifasFormasPago()
        {
            tfpEstatus = true;
        }

        [Key, MaxLength(15)]
        public string tfpIdFormaPago { get; set; }

        [MaxLength(1024)]
        public string tfpDescripcion { get; set; }

        [Required]
        public bool tfpEstatus { get; set; }
    }

    [Table("tblTarifasConceptosFormasPago")]
    public class schemaTarifasConceptosFormasPago
    {
        public schemaTarifasConceptosFormasPago()
        {
            tcfpEstatus = true;
        }

        [Key, Required]
        public int tcfpId { get; set; }

        [Required]
        public string tcoIdConcepto { get; set; }

        [ForeignKey("tcoIdConcepto")]
        public virtual schemaTarifasConceptos tblTarifasConcepto { get; set; }

        [Required]
        public string tfpIdFormaPago { get; set; }

        [ForeignKey("tfpIdFormaPago")]
        public virtual schemaTarifasFormasPago tblTarifasFormaPago { get; set; }

        [Required]
        public bool tcfpEstatus { get; set; }
    }

    [Table("tblTarifasPeriodicidades")]
    public class schemaTarifasPeriodicidades
    {
        public schemaTarifasPeriodicidades()
        {
            tpeEstatus = true;
        }

        [Key, MaxLength(15)]
        public string tpeIdPeriodicidad { get; set; }

        [MaxLength(1024)]
        public string tpeDescripcion { get; set; }

        [Required]
        public bool tpeEstatus { get; set; }
    }

    [Table("tblTarifasCfpPeriodicidades")]
    public class schemaTarifasCfpPeriodicidades
    {
        public schemaTarifasCfpPeriodicidades()
        {
            tcfppEstatus = true;
        }

        [Key, Required]
        public int tcfppId { get; set; }

        [Required]
        public int tcfpId { get; set; }

        [ForeignKey("tcfpId")]
        public virtual schemaTarifasConceptosFormasPago tblTarifasConceptoFormaPago { get; set; }

        [Required]
        public string tpeIdPeriodicidad { get; set; }

        [ForeignKey("tpeIdPeriodicidad")]
        public virtual schemaTarifasPeriodicidades tblTarifasPeriodicidad { get; set; }

        [Required]
        public bool tcfppEstatus { get; set; }
    }

    [Table("tblTarifasTiposPago")]
    public class schemaTarifasTiposPago
    {
        public schemaTarifasTiposPago()
        {
            ttpEstatus = true;
        }

        [Key, MaxLength(20)]
        public string ttpIdTipoPago { get; set; }

        [Required]
        public bool ttpEstatus { get; set; }
    }

    [Table("tblTarifasCfppTiposPago")]
    public class schemaTarifasCfppTiposPago
    {
        public schemaTarifasCfppTiposPago()
        {
            tcfpptpEstatus = true;
        }

        [Key, Required]
        public int tcfpptpId { get; set; }

        [Required]
        public int tcfppId { get; set; }

        [ForeignKey("tcfppId")]
        public virtual schemaTarifasCfpPeriodicidades tblTarifasCfpPeriodicidad { get; set; }

        [Required]
        public string ttpIdTipoPago { get; set; }

        [ForeignKey("ttpIdTipoPago")]
        public virtual schemaTarifasTiposPago tblTarifasTipoPago { get; set; }

        [Required]
        public bool tcfpptpEstatus { get; set; }
    }

    [Table("tblTarifasMetodosPago")]
    public class schemaTarifasMetodosPago
    {
        public schemaTarifasMetodosPago()
        {
            tmpEstatus = true;
        }

        [Key, MaxLength(20)]
        public string tmpIdMetodoPago { get; set; }

        [Required]
        public bool tmpEstatus { get; set; }
    }

    [Table("tblTarifasCfpptpMetodosPago")]
    public class schemaTarifasCfpptpMetodosPago
    {
        public schemaTarifasCfpptpMetodosPago()
        {
            tcfpptpmpEstatus = true;
        }

        [Key, Required]
        public int tcfpptpmpId { get; set; }

        [Required]
        public int tcfpptpId { get; set; }

        [ForeignKey("tcfpptpId")]
        public virtual schemaTarifasCfppTiposPago tblTarifasCfppTipoPago { get; set; }

        [Required]
        public string tmpIdMetodoPago { get; set; }

        [ForeignKey("tmpIdMetodoPago")]
        public virtual schemaTarifasMetodosPago tblTarifasMetodoPago { get; set; }

        [Required]
        public bool tcfpptpmpEstatus { get; set; }
    }

    #endregion

    #region Torneos

    [Table("tblLigaCategoriasTorneos")]
    public class schemaLigaCategoriasTorneos
    {
        public schemaLigaCategoriasTorneos()
        {
            lctEstatus = true;
        }

        [Key, Required]
        public int lctId { get; set; }

        [Required]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Required, MaxLength(100)]
        public string lctNombre { get; set; }

        [Required]
        public string depNombre { get; set; }

        [ForeignKey("depNombre")]
        public virtual schemaDeportes tblDeporte { get; set; }

        [Required]
        public int ttoId { get; set; }

        [ForeignKey("ttoId")]
        public virtual schemaTipoTorneos tblTipoTorneo { get; set; }

        [MaxLength(1024)]
        public string lctDescripcion { get; set; }

        public int? lctEdadMin { get; set; }

        public int? lctEdadMax { get; set; }

        public bool lctEstatus { get; set; }
    }

    [Table("tblLigaCanchasTorneos")]
    public class schemaLigaCanchasTorneos
    {
        public schemaLigaCanchasTorneos()
        {
            lcatEstatus = true;
            torFechaCreacionUTC = DateTime.Now;
        }

        [Key, Required]
        public int lcatId { get; set; }

        [Required]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Required, MaxLength(100)]
        public string lcatNombre { get; set; }
        public string lcatdomicilio { get; set; }
        
        public string lcatNumExtInt { get; set; }
        
        public string lcatColonia { get; set; }
        
        public string lcatMunicipio { get; set; }
        
        public string lcatEstado { get; set; }
       
        public string lcatCodigoPostal { get; set; }

        public string lcatLatitud { get; set; }

        public string lcatLongitud { get; set; }
              
        [MaxLength(1024)]
        public string lcatDescripcion { get; set; }

        public bool lcatEstatus { get; set; }

        public DateTime torFechaCreacionUTC { get; set; }
    }

    [Table("tblTorneoEstructura")]
    public class schemaTorneoEstructura
    {
        public schemaTorneoEstructura()
        {
            tcsEstatus = true;
            tcsDeporteEnEquipo = true;
        }

        [Key]
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int tscId { get; set; }

        public string tscNombre { get; set; }

        public string tcsDescripcion { get; set; }

        public bool tcsEstatus { get; set; }

        public bool tcsDeporteEnEquipo { get; set; }

        public int tcsOrden { get; set; }
    }

    [Table("tblTorneos")]
    public class schemaTorneos
    {
        public schemaTorneos()
        {
            torComentarios = true;
            torFechaCreacionUTC = DateTime.Now;
            torEstatus = true;
            torAprobada = false;
            torPagado = false;
            torEsCoaching = false;
            torPrivate = false;
        }

        [Key, Required(ErrorMessage = constClass.requiredMsg)]
        public int torId { get; set; }

        public string torTipo { get; set; }

        public string torImgUrl { get; set; }

        public bool torComentarios { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg), MaxLength(500)]
        public string torNombreTorneo { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLiga { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int lctId { get; set; }

        [ForeignKey("lctId")]
        public virtual schemaLigaCategoriasTorneos tblCategoriaTorneo { get; set; }

        public DateTime? torFechaInicio { get; set; }

        public DateTime? torFechaTermino { get; set; }

        public DateTime? torFechaLimiteInscripcion { get; set; }

        public int? torNumeroJuegos { get; set; }

        public int? torNumeroEquipos { get; set; }

        public int? torMaxJugadoresEquipo { get; set; }

        public decimal? torPuntosGanar { get; set; }

        public decimal? torPuntosEmpatar { get; set; }

        public decimal? torPuntosPerder { get; set; }

        public int tesId { get; set; }

        [ForeignKey("tesId")]
        public virtual schemaTorneoEstructura tblTorneoEstructura { get; set; }

        public virtual schemaTorneoDireccion tblTorneoDireccion { get; set; }

        public string torNumeroContacto { get; set; }

        public string torCorreoContacto { get; set; }

        public decimal? torPrecioTorneo { get; set; }

        public int? torDiasParaPago { get; set; }

        /// <summary>
        /// Tipo de pago (Anticipo o Pago Total)
        /// </summary>
        public int? tcfpptpId { get; set; }

        [ForeignKey("tcfpptpId")]
        public virtual schemaTarifasCfppTiposPago tblTarifasCfppTiposPago { get; set; }

        public virtual List<schemaTorneoTarifas> tblTorneoTarifas { get; set; }

        [MaxLength(128)]
        public string torUserIdCreador { get; set; }

        [ForeignKey("torUserIdCreador")]
        public virtual ApplicationUser tblUserCreador { get; set; }

        public DateTime torFechaCreacionUTC { get; set; }

        public bool torEstatus { get; set; }

        public bool torAprobada { get; set; }
        public bool torPagado { get; set; }
        public string torLatitud { get; set; }

        public string torLongitud { get; set; }

        public bool torEsCoaching { get; set; }

        public bool torDeporteEnEquipo { get; set; }

        public bool torPrivate { get; set; }
    }


    [Table("tblTorneoDireccion")]
    public class schemaTorneoDireccion
    {
        public int torId { get; set; }

        public virtual schemaTorneos tblTorneo { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(500, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcCalle { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(100, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcNumeroExtInt { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(100, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcColonia { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(100, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcMunicipio { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(100, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcEstado { get; set; }

        [MaxLength(10, ErrorMessage = constClass.maxLengthMsg)]
        public string ldcCodigoPostal { get; set; }

    }

    [Table("tblTorneoTarifas")]
    public class schemaTorneoTarifas
    {
        [Key, Column(Order = 0)]
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneos { get; set; }

        [Key, Column(Order = 1)]
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int tarId { get; set; }

        [ForeignKey("tarId")]
        public virtual schemaTarifas tblTarifas { get; set; }

        public bool ttaHabilitado { get; set; }
    }

    [Table("tblTorneoCoAdministradores")]
    public class schemaTorneoCoAdministradores
    {
        public schemaTorneoCoAdministradores()
        {
            tcaConfirmacion = false;
        }
        [Key]
        public int tcoId { get; set; }
        [Required, Column(Order = 0)]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }

        public string userCorreo { get; set; }
        //[Key, Required, MaxLength(128), Column(Order = 1)]
        //public string tcaUserId { get; set; }

        //[ForeignKey("tcaUserId")]
        //public virtual ApplicationUser tblUsuario { get; set; }

        public bool tcaConfirmacion { get; set; }

        public string tcaCodigoConfirmacion { get; set; }

        public DateTime? tcaFechaConfirmacionUTC { get; set; }
    }

    [Table("tblTorneoComentarios")]
    public class schemaTorneoComentarios
    {
        public schemaTorneoComentarios()
        {
            tcoEstatus = true;
        }

        [Key, Required, Column(Order = 0)]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }

        [Key, Required, Column(Order = 1)]
        public int tcoId { get; set; }

        [Required, MaxLength(128)]
        public string tcoUserIdComenta { get; set; }

        [ForeignKey("tcoUserIdComenta")]
        public virtual ApplicationUser tblUsuarioComenta { get; set; }

        public int? equId { get; set; }

        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipo { get; set; }

        public decimal tcoCalificacion { get; set; }

        [MaxLength(1000)]
        public string tcoComentario { get; set; }

        public DateTime tcoFechaComentarioUTC { get; set; }

        public bool tcoEstatus { get; set; }

    }

    #endregion

    #region Equipos

    [Table("tblEquipos")]
    public class schemaEquipos
    {
        public schemaEquipos()
        {
            equEstatus = true;
            equDelete = false;
            equPagado = false;
            equFechaCreacionUTC = DateTime.Now;
        }

        [Key, Required]
        public int equId { get; set; }

        [Required]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneos { get; set; }

        public string equImgUrl { get; set; }

        [Required, MaxLength(100)]
        public string equNombreEquipo { get; set; }

        public DateTime equFechaCreacionUTC { get; set; }

        public bool equEstatus { get; set; }
        public bool equDelete { get; set; }
        
        public DateTime? equFechaVencimientoPagoUTC { get; set; }

        public bool equPagado { get; set; }

        public decimal? equPrecioTorneo { get; set; }

        public virtual ICollection<schemaJugadorEquipos> listJugadores { get; set; }

        public string equAdminCorreo { get; set; }
        /// <summary>
        /// Es el administrador del torneo.
        /// </summary>
        [MaxLength(128)]
        public string equUserIdCreador { get; set; }
        
    }

    [Table("tblEquiposCoAdministradores")]
    public class schemaEquiposCoAdministradores
    {
        public schemaEquiposCoAdministradores()
        {
        }
        [Key]
        public int ecaId { get; set; }

        [Required, Column(Order = 0)]
        public int equId { get; set; }

        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipo { get; set; }

        [Required, MaxLength(250), Column(Order = 1)]
        public string ecaCorreoId { get; set; }

        public string equUserId { get; set; }

        [ForeignKey("equUserId")]
        public virtual ApplicationUser tblUsuario { get; set; }

        public bool equConfirmado { get; set; }

        public string equCodigoConfirmacion { get; set; }

        public DateTime? equFechaConfirmacionUTC { get; set; }

        public bool equEstatus { get; set; }
    }

    #endregion

    #region Jugadores

    [Table("tblJugadores")]
    public class schemaJugadores
    {
        public schemaJugadores()
        {
            jugEstatus = true;
            jugFechaCreacionUTC = DateTime.Now;
        }
        [Key, Column(Order = 0),Required]
        public int jugId { get; set; }        

        [MaxLength(250),Required]
        public string jugCorreo { get; set; }

        public string jugUserId { get; set; }

        [ForeignKey("jugUserId")]
        public virtual ApplicationUser tblUsuario { get; set; }

        public string jugNombre { get; set; }

        public DateTime jugFechaCreacionUTC { get; set; }

        public bool jugEstatus { get; set; } 
    }

    [Table("tblJugadorEquipos")]
    public class schemaJugadorEquipos
    {
        public schemaJugadorEquipos()
        {
            jugEstatus = true;
            jugFechaCreacionUTC = DateTime.Now;
        }
        [Key, Column(Order = 0), Required]
        public int jugEquId { get; set; }
        
      //  [Column(Order = 1)]
      //  public int jugId { get; set; }

      //  [Key, ForeignKey("jugId")]
      //  public virtual schemaJugadores tblJugadores { get; set; }

        public string jugUserId { get; set; }

        [ForeignKey("jugUserId")]
        public virtual ApplicationUser tblUsuario { get; set; }

        public int? torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneos { get; set; }

        /// <summary>
        /// Este campo es igual a equId, solo que no me permitia que fuera nulo.
        /// Donde 0 = NULL y otro valor = equId.
        /// </summary>
        [Column(Order = 2)]
        public int equIdRef { get; set; }

        public int? equId { get; set; }

        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipos { get; set; }

        public string jugCorrreo { get; set; }

        public DateTime jugFechaCreacionUTC { get; set; }

        public bool jugEstatus { get; set; }

        /// <summary>
        /// Este campo es igual a equId, solo que no me permitia que fuera nulo.
        /// Donde 0 = NULL y otro valor = equId.
        /// </summary>
       
        public string jugCodigoConfirmacion { get; set; }

        public bool jugConfirmado { get; set; }

        public DateTime? jugFechaVencimientoPagoUTC { get; set; }

        public bool jugPagado { get; set; }

        public decimal? jugPrecioTorneo { get; set; }
    }
    #endregion

    #region Arbitros
    [Table("tblArbitros")]
    public class schemaArbitros
    {
        public schemaArbitros()
        {
            arbEstatus = true;
            arbFechaCreacionUTC = DateTime.Now;
        }
        [Key, Column(Order = 0), Required]
        public int arbId { get; set; }

        public string arbCorreo { get; set; }
        
        public string arbUserId { get; set; }
        [ForeignKey("arbUserId")]
        public virtual ApplicationUser tblUsuario { get; set; }

        public string arbNombre { get; set; }

        public DateTime arbFechaCreacionUTC { get; set; }
                
        public bool arbEstatus { get; set; }
    }
    #endregion
    [Table("tblArbitrosLigas")]
    public class schemaArbitrosLigas
    {
        public schemaArbitrosLigas(){
            arbConfirmado = false;
            arbLigaFechaCreacionUTC = DateTime.Now;
        }
        [Key , Column(Order =0) , Required]
        public int arbId { get; set; }

        [ForeignKey("arbId")]
        public virtual schemaArbitros tblArbitros { get; set; }

        [Key , Column(Order =1) , Required]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        public string arbCodigoConfirmacion { get; set; }

        public bool arbConfirmado { get; set; }

        public DateTime arbLigaFechaCreacionUTC { get; set; }
    }

    [Table("tblArbitrosPartidos")]
    public class schemaArbitrosPartidos
    {
        public schemaArbitrosPartidos()
        {
            arbConfirmado = false;
            arbRechazar = false;
        }
        [Key, Column(Order = 0), Required]
        public int arbId { get; set; }

        [ForeignKey("arbId")]
        public virtual schemaArbitros tblArbitros { get; set; }

        [Key, Column(Order = 1), Required]
        public int parId { get; set; }

        [ForeignKey("parId")]
        public virtual schemaPartidos tblPartidos { get; set; }

        public string arbCodigoConfirmacion { get; set; }

        public bool arbConfirmado { get; set; }

        public bool arbRechazar { get; set; }
    }

    [Table("tblCountry")]
    public class schemaCountry
    {
        [Key, Column(Order = 0), Required]
        public int id { get; set; }
        public string iso { get; set; }
        public string name { get; set; }
        public string nicename { get; set; }
        public string iso3 { get; set; }
        public int numcode { get; set; }
        public int phonecode { get; set; }
    }


    #region Partidos Estadisticas

    #region Futbol

    [Table("tblJuegosFutbol")]
    public class schemaJuegosFutbol
    {
        public schemaJuegosFutbol()
        {
            jfuEstatus = true;
        }

        [Key, Required]
        public int jfuId { get; set; }

        [Required]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }

        [Required]
        public int equId1 { get; set; }

        [ForeignKey("equId1")]
        public virtual schemaEquipos tblEquipo1 { get; set; }

        [Required]
        public int equId2 { get; set; }

        [ForeignKey("equId2")]
        public virtual schemaEquipos tblEquipo2 { get; set; }

        [Required]
        public DateTime jfuFechaInicio { get; set; }

        [Required]
        public DateTime jfuFechaTermino { get; set; }

        public int? lcatId { get; set; }

        [ForeignKey("lcatId")]
        public virtual schemaLigaCanchasTorneos tblCancha { get; set; }

        public bool jfuEstatus { get; set; }
    }

    [Table("tblJuegosFutbolEstadisticasJugador")]
    public class schemaJuegosFutbolEstadisticasJugador
    {
        public schemaJuegosFutbolEstadisticasJugador()
        {
        }

        [Key, Required, Column(Order = 0)]
        public int parId { get; set; }

        [ForeignKey("parId")]
        public virtual schemaPartidos tblPartidos { get; set; }

        [Key, Required, Column(Order = 1)]
        public int equId { get; set; }

        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipo { get; set; }

        [Key, Required, Column(Order = 2)]
        public string UserIdJugador { get; set; }

        [ForeignKey("UserIdJugador")]
        public virtual ApplicationUser tblUserJugador { get; set; }

        [Required]
        public int jfejGoles { get; set; }

        [Required]
        public int jfejAsistencias { get; set; }

        [Required]
        public int jfejFaltas { get; set; }

        [Required]
        public int jfejTarjetasAmarillas { get; set; }

        [Required]
        public int jfejTarjetasRojas { get; set; }

        [Required]
        public int jfejPartidosSuspendidos { get; set; }
    }

    [Table("tblJuegosFutbolArbitros")]
    public class schemaJuegosFutbolArbitros
    {

    }

    [Table("tblPartidos")]
    public class schemaPartidos
    {
        public schemaPartidos()
        {
            parEstatus = true;
        }
        [Key, Required]
        public int parId { get; set; }

        [Required]
        public int ligId { get; set; }
        
        public int lcatId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLigas { get; set; }

        [Required]
        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneos { get; set; }

        public string arbNombre { get; set; }

        public int arbId { get; set; }
        //[ForeignKey("arbId")]
//        public virtual schemaArbitros tblArbitros { get; set; }

        [Required]
        public int equIdUno { get; set; }

        [Required]
        public string equNombreEquipoUno { get; set; }

        public int equResultadoUno { get; set; }
        
        public int equIdDos { get; set; }
            
        public string equNombreEquipoDos { get; set; }

        public int equResultadoDos { get; set; }

        public string imgDos { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public DateTime parFecha_Inicio { get; set; }

        public DateTime parFecha_Fin { get; set; }

        [MaxLength(100)]
        public string parEstado { get; set; }

        public bool parEstatus { get; set; }

        public bool parCheck { get; set; }
    }

    #endregion

    #region Basketball

    #endregion

    #region Volleyball

    #endregion

    #endregion

    #region Pagos
    [Table("tblPagos")]
    public class schemaPagos
    {
        public schemaPagos()
        {
            pagoFechaCreacionUTC = DateTime.Now;
        }
        [Key, Required]
        public int IdPago { get; set; }
        [Required]
        public string userId { get; set; }
        [ForeignKey("userId")]
        public virtual ApplicationUser tblUsers { get; set; }
        public string concepto { get; set; }
        public int conceptoId { get; set; }
        //[ForeignKey("ligId")]
        //public virtual schemaLigas tblLiga { get; set; }

        public string IdTransaccion { get; set; }
       // public virtual schemaDetallesPago tblDetallesPago { get; set; }

        public DateTime pagoFechaCreacionUTC { get; set; }                
    }

    [Table("tblDetallesPago")]
    public class schemaDetallesPago
    {
        public schemaDetallesPago()
        {
            FechaCreacionUTC = DateTime.Now;
        }
        [Key, Required]
        public int Id { get; set; }
        [Required]
        public int IdPago { get; set; }
        [ForeignKey("IdPago")]
        public virtual schemaPagos tblPagos { get; set; }
        public string conceptoPago { get; set; }
        [Required]
        public decimal total { get; set; }
        [Required]
        public string IdTransaccion { get; set; }
        public string referencia { get; set; }
        public string metodoPago { get; set; }
        [Required]
        public string status { get; set; }
        public string ipAddress { get; set; }
        public DateTime FechaCreacionUTC { get; set; }
    }

    public class auxiliar
    {
        public schemaTorneos torneo;
        public int? totalEquipos;
        public int? espacios;
    }

   /* [Table("tblDatosTarjeta")]
    public class schemaDatosTarjeta
    {
        [Key,Required]
        public int Id { get; set; }

        [Required]
        public string userId { get; set; }
        [ForeignKey("userId")]
        public virtual ApplicationUser tblUsers { get; set; }

        public string numeroTarjeta { get; set; }
        public string nombrePropietario { get; set; }
        public int mes { get; set; }
        public int year { get; set; }
        public int cvv { get; set; }
        public int cp { get; set; }
        public string calle { get; set; }
        public string numero { get; set; }
        public string propietario { get; set; }
    }*/
    #endregion


}