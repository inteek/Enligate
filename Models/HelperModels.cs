using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using sw_EnligateWeb.Models.HelperClasses;
using Newtonsoft.Json;

namespace sw_EnligateWeb.Models
{

    public enum LoginStatus { Success, LockedOut, Failed }

    public class ChatModel
    {

        /// <summary>
        /// Users that have connected to the chat
        /// </summary>
        public List<ChatUser> Users;

        /// <summary>
        /// Messages by the users
        /// </summary>
        public List<ChatMessage> ChatHistory;

        public ChatModel()
        {
            Users = new List<ChatUser>();
            ChatHistory = new List<ChatMessage>();

            ChatHistory.Add(new ChatMessage()
            {
                Message = "The chat server started at " + DateTime.Now
            });
        }

        public class ChatUser
        {
            public string NickName;
            public DateTime LoggedOnTime;
            public DateTime LastPing;
        }

        public class ChatMessage
        {
            /// <summary>
            /// If null, the message is from the server
            /// </summary>
            public ChatUser ByUser;

            public DateTime When = DateTime.Now;

            public string Message = "";

        }
    }

    public class MainPageViewModel
    {
        public List<schemaDeportes> deportes { get; set; }
        public List<SelectListItem> ddlDeportes { get; set; }
        public List<SelectListItem> ddlTipoTorneos { get; set; }
        public List<SelectListItem> ddlCiudades { get; set; }
        public List<SelectListItem> ddlZonas { get; set; }
    }

    public class LoginExternalConfirmViewModel
    {
        [Required(ErrorMessage = "Favor de ingresar el nombre.")]
        [MaxLength(50), Display(Name = "Nombre(s)")]
        public string usuNombre { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el apellido.")]
        [MaxLength(50), Display(Name = "Apellido(s)")]
        public string usuApellido { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo válida")]
        public string usuEmail { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar correo")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("usuEmail", ErrorMessage = "El correo no coincide, favor de verificarlo.")]
        public string usuEmailConfirmar { get; set; }

        public string LoginProvider { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        public string usuEmail { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string usuPassword { get; set; }

        [Display(Name = "No cerrar sesión")]
        public bool RememberMe { get; set; }

        public string scriptJS { get; set; }
    }

    public class RegisterViewModel
    {
        public string usuId { get; set; }

        //[Required(ErrorMessage = "Favor de ingresar el nombre.")]
        //[MaxLength(50), Display(Name = "Nombre")]
        //public string usuNombre { get; set; }

        //[Required(ErrorMessage = "Favor de ingresar el apellido.")]
        //[MaxLength(50), Display(Name = "Apellidos")]
        //public string usuApellido { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        [Display(Name = "Correo")]
        public string usuEmail { get; set; }

        //[EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        //[Display(Name = "Confirmar correo")]
        //[System.ComponentModel.DataAnnotations.CompareAttribute("usuEmail", ErrorMessage = "El correo electrónico no coincide.")]
        //public string usuEmailConfirm { get; set; }

        //[Required(ErrorMessage = "Favor de seleccionar un rol.")]
        //[Display(Name = "Rol")]
        //public string usuRol { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la contraseña")]
        [StringLength(100, ErrorMessage = "La longitud de la contraseña debe ser al menos de {2} caracteres.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string usuPassword { get; set; }

        public string scriptJS { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirmar Contraseña")]
        //[System.ComponentModel.DataAnnotations.CompareAttribute("usuPassword", ErrorMessage = "La contraseña y la confirmacion de contraseña no coinciden.")]
        //public string usuPasswordConfirm { get; set; }

        //[Display(Name = "Telefono")]
        //[DataType(DataType.PhoneNumber)]
        //public string usuTelefono { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        [Display(Name = "Correo")]
        public string usuEmail { get; set; }

        public string scriptJS { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Display(Name = "Correo")]
        public string usuEmail { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la contraseña")]
        [StringLength(100, ErrorMessage = "La longitud de la contraseña debe ser al menos de {2} caracteres.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string usuPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("usuPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string usuConfirmPassword { get; set; }

        public string Code { get; set; }

        public string userId { get; set; }

        public string scriptJS { get; set; }
    }

    public class ConfirmEmailViewModel
    {
        public string usuId { get; set; }

        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        [Display(Name = "Correo")]
        public string usuEmail { get; set; }

        [Required(ErrorMessage = "Favor de ingresar la contraseña")]
        [StringLength(100, ErrorMessage = "La longitud de la contraseña debe ser al menos de {2} caracteres.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string usuPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("usuPassword", ErrorMessage = "La contraseña y la confirmacion de contraseña no coinciden.")]
        public string usuConfirmPassword { get; set; }
    }

    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            _profile = new UserProfileViewModel();

        }

        public UserProfileViewModel _profile { get; set; }
        public string profileNumber { get; set; }
    }

    /// <summary>
    /// Clase utilizada para obtener la url de una imagen
    /// </summary>
    public class FileReferenceViewModel
    {
        public string fileUrl { get; set; }

        public string fileExtension { get; set; }
    }

    public class UserProfileViewModel
    {
        public UserProfileViewModel()
        {
            usuGenero = "F";
            dllContry = new List<SelectListItem>();
            dllState = new List<SelectListItem>();
            dllCity = new List<SelectListItem>();
        }
        public string codeIdPais { get; set; }
        public string codeIdEstado { get; set; }
        public string codeIdCiudad { get; set; }
        [Required(ErrorMessage = "Debes iniciar sesión.")]
        public string usuUsername { get; set; }

        public string imgURL { get; set; }

        [Display(Name = "Nombre y Apellidos")]
        public string usuNombreCompleto { get; set; }

        [Display(Name = "Genero")]
        public string usuGenero { get; set; }

        [Display(Name = "Fecha de nacimiento")]
        public string usuFechaNacimiento { get; set; }

        [Display(Name = "Pais donde reside")]
        public string usuPais { get; set; }

        [Display(Name = "Estado donde reside")]
        public string usuEstado { get; set; }

        [Display(Name = "Ciudad donde reside")]
        public string usuCiudad { get; set; }

        [Display(Name = "Codigo Postal donde reside")]
        public int usuCP { get; set; }

        [Display(Name = "Teléfono")]
        public string usuTelefono { get; set; }

        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida.")]
        [Display(Name = "Correo de contacto")]
        public string usuCorreo { get; set; }

        public Dictionary<string, string[]> modelStateErrors { get; set; }

        public string jsGetModelFunctionName { get; set; }

        public string profileNumber { get; set; }
        public string user_id { get; set; }

        public List<SelectListItem> dllContry { get; set; }
        public List<SelectListItem> dllState { get; set; }
        public List<SelectListItem> dllCity { get; set; }
    }

    public class JsonResultViewModel
    {

        public JsonResultViewModel()
        {
            booSuccess = false;
            booHasPartialView = false;
            booHasErrMessagePartialView = false;
        }

        public bool booSuccess { get; set; }

        public bool booHasPartialView { get; set; }

        public string strPartialViewString { get; set; }

        public bool booHasErrMessagePartialView { get; set; }

        public string strErrMessagePartialViewString { get; set; }

        public int equId { get; set; }
    }

    public class JsonReturn_ErrorsViewModel
    {
        public string jsScript { get; set; }
    }

    public class LeagueRegisterViewModel
    {
        public LeagueRegisterViewModel()
        {
            lreId = 0;
            lreUserProfile = new UserProfileAddLeagueViewModel();
            lreLeagueCoAdmins = new LeagueCoAdministratorsEditViewModel();
            lreTaxData = new TaxDataViewModel();
            lreBusinessAddress = new BusinessAddressViewModel();
            lreLeagueSaved = false;
            lreAllowAddAdmins = false;
            lreAddingLeague = true;
        }

        public int lreId { get; set; }

        public UserProfileAddLeagueViewModel lreUserProfile { get; set; }

        public LeagueCoAdministratorsEditViewModel lreLeagueCoAdmins { get; set; }

        [Display(Name = "Tipo de Liga")]
        [Required(ErrorMessage = "Campo obligatorio.")]
        public string lreTipoLiga { get; set; }

        public virtual List<schemaLigaCategorias> lreDdlTiposLiga { get; set; }

        //[Display(Name = "Forma de Pago")]
        //public string lreFormaPago { get; set; }

       // public virtual List<schemaTarifasFormasPago> lreDdlFormasPago { get; set; }

        //public string lrePeriodicidadPago { get; set; }

        //public virtual List<schemaTarifasPeriodicidades> lreDdlPeriodicidadesPago { get; set; }

        //public int tcfppId { get; set; }

        //public virtual List<schemaTarifas> lreListTarifas { get; set; }

        public TaxDataViewModel lreTaxData { get; set; }

        public virtual string lreImgUrl { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Nombre Liga")]
        [MaxLength(50, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string lreNombreLiga { get; set; }

        public BusinessAddressViewModel lreBusinessAddress { get; set; }

        public virtual string lreCopiarDomicilioFiscal { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa una cuenta de correo válida.")]
        [Display(Name = "E-mail")]
        public string lreCorreoContacto { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Telefono")]
        public string lreTelefonoContacto { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Descripción")]
        public string lreDescripcion { get; set; }

        public virtual bool lreLeagueSaved { get; set; }

        public virtual bool lreAllowAddAdmins { get; set; }

        public virtual bool lreAddingLeague { get; set; }

        //[Range((double)0, (double)100, ErrorMessage = "Debe ser un valor entre {1} y {2}")]
        //[DataType(DataType.Currency, ErrorMessage = "Debe ser una cantidad.")]
        //public decimal lrePorcentajeDescuento { get; set; }

        //public decimal lreTotalPagar { get; set; }

        public string lreLigaLatitud { get; set; }
        public string lreLigaLongitud { get; set; }
    }

    public class UserProfileAddLeagueViewModel
    {
        public UserProfileAddLeagueViewModel()
        {
            usuGenero = "F";
        }

        [Required(ErrorMessage = "Debes iniciar sesión.")]
        public string usuUsername { get; set; }

        public virtual string imgURL { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Nombre y Apellidos")]
        public string usuNombreCompleto { get; set; }

        [Display(Name = "Genero")]
        public string usuGenero { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Fecha de nacimiento")]
        public string usuFechaNacimiento { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Ciudad donde reside")]
        public string usuCiudad { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Teléfono")]
        public string usuTelefono { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida.")]
        [Display(Name = "Correo de contacto")]
        public string usuCorreo { get; set; }
    }

    public class LeagueCoAdministratorsViewModel
    {
        public LeagueCoAdministratorsViewModel()
        {
            lcaConfirmado = false;
        }

        public string lcaUserId { get; set; }

        public string lcaNombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo válida.")]
        public string lcaEmail { get; set; }

        public bool lcaConfirmado { get; set; }
    }

    public class LeagueCoAdministratorsEditViewModel
    {
        public LeagueCoAdministratorsEditViewModel()
        {
            lcaEmailList = new List<LeagueCoAdministratorsViewModel>();
        }

        public LeagueCoAdministratorsViewModel lcaEmail { get; set; }

        public List<LeagueCoAdministratorsViewModel> lcaEmailList { get; set; }
    }

    public class TaxDataViewModel
    {
        public TaxDataViewModel()
        {
            tdaSaveTda = true;
        }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "RFC")]
        [MaxLength(14, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaRFC { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Razón Social")]
        [MaxLength(250, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaRazonSocial { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Domicilio")]
        [MaxLength(500, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaDomicilio { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "No.")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaNumeroExtInt { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Colonia")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaColonia { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Municipio")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaMunicipio { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Estado")]
        [MaxLength(50, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaEstado { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "C.P.")]
        [MaxLength(10, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string tdaCodigoPostal { get; set; }

        public virtual bool tdaSaveTda { get; set; }
    }

    public class BusinessAddressViewModel
    {
        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Domicilio")]
        [MaxLength(500, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badDomicilio { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "No.")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badNumeroExtInt { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Colonia")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badColonia { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Municipio")]
        [MaxLength(100, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badMunicipio { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Estado")]
        [MaxLength(50, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badEstado { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "C.P.")]
        [MaxLength(10, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string badCodigoPostal { get; set; }
    }

    public class RoleModelViewModel
    {
        public string rolId { get; set; }
        public string rolName { get; set; }
    }

    public class DropDownListViewModel
    {
        public int ValueInt { get; set; }

        public string Value { get; set; }

        public string Text { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        public string usuEmail { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(100, ErrorMessage = "La longitud de la contraseña debe ser al menos de {2} caracteres.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("NewPassword", ErrorMessage = "La contraseña y la confirmacion de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public class SiteConfigGeneralViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Nombre de la aplicación")]
        public string scoAppName { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Nombre de la empresa")]
        public string scoCompanyName { get; set; }
    }

    public class SiteConfigEmailViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        [Display(Name = "Correo")]
        public string scoSenderEmail { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Nombre del correo")]
        public string scoSenderDisplayEmailName { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Contraseña")]
        public string scoSenderEmailPassword { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Servidor SMTP")]
        public string scoSenderSMTPServer { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Puerto")]
        public string scoSenderPort { get; set; }

        [NotMapped]
        public string scoPrivateKeyEncryption
        {
            get { return scoSenderEmail + scoSenderDisplayEmailName; }
        }
    }

    public class ApplicationUserViewModel
    {

        public string Id { get; set; }

        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Nombre")]
        public string usuNombre { get; set; }

        [Display(Name = "Apellido")]
        public string usuApellido { get; set; }

        [Display(Name = "Rol")]
        public string RoleName { get; set; }

        [Display(Name = "Correo confirmado")]
        public bool EmailConfirmed { get; set; }
    }

    public class MenusViewModel
    {
        public schemaMenus menu { get; set; }

        public List<schemaSubMenus> submenus { get; set; }
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "Favor de ingresar tu nombre.")]
        public string conNombre { get; set; }

        [Required(ErrorMessage = "Favor de ingresar tu correo.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo válida.")]
        public string conEmail { get; set; }

        [Required(ErrorMessage = "Favor de ingresar los comentarios o dudas.")]
        public string conDescripcion { get; set; }
    }

    public class RequestLeaguesViewModel
    {
        public RequestLeaguesViewModel()
        {

        }

        public int ligId { get; set; }

        public string ligNombreLiga { get; set; }

        public string ligTipoLiga { get; set; }

        public string ligNombreCreador { get; set; }

        public string ligCreadorId { get; set; }

        public DateTime ligFechaRegistro { get; set; }

        public string ligFormaPago { get; set; }

        public bool ligEstatus { get; set; }

        public bool ligAprobada { get; set; }

        public bool ligSolicitud { get; set; }

        public bool ligSolicitudRevisada { get; set; }
        
        public string viewStatus { get; set; }
    }

    public class DateRangePickerViewModel
    {
        public DateRangePickerViewModel()
        {
            StartDate = new DateTime(2015,1,1);
            EndDate = DateTime.Today;
        }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DateRange(StartDateEditFieldName = "Start")]
        public DateTime EndDate { get; set; }
    }

    public class RequestLeaguesFilterViewModel
    {
        public RequestLeaguesFilterViewModel()
        {
            drpvmDates = new DateRangePickerViewModel();
            reqTipo = "N";
        }

        public DateRangePickerViewModel drpvmDates { get; set; }

        public string reqTipo { get; set; }

        public int ligId { get; set; }
    }

    public class IndexAdministratorViewModel
    {
        public int iadTotalNuevasSolicitudes { get; set; }

        //public int iadTotalNewRequests { get; set; }

        //public int iadTotalNewRequests { get; set; }
    }

    public class UsersTestingViewModel
    {

        public int uteId { get; set; }

        public string uteNombre { get; set; }

        public int uteDiasRestantes { get; set; }
    }

    public class PaymentsViewModel
    {
        public int payId { get; set; }
        public int ligId { get; set; }
        public string ligNombreLiga { get; set; }
        public string payNombre { get; set; }

        public DateTime payFecha { get; set; }
    }

    public class NewsViewModel
    {
        public int newId { get; set; }

        public string newImageUrl { get; set; }

        public string newContent { get; set; }

        public string newActionUrl { get; set; }

        public string newActionUrlContent { get; set; }
    }

    public class TarifasViewModel
    {
        [Key]
        public int tarId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar el método de pago.")]
        public int tarMetodoPago { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [DataType(DataType.Currency, ErrorMessage = "Debe ser una cantidad.")]
        public decimal? tarPrecio { get; set; }

        [Required(ErrorMessage = "El porcentaje es obligatorio.")]
        [Range((double)0, (double)100, ErrorMessage = "Debe ser un valor entre {1} y {2}")]
        [DataType(DataType.Currency, ErrorMessage = "Debe ser un porcentaje.")]
        public decimal? tarPorcentajeComision { get; set; }
    }

    public class TarifasGridViewModel
    {
        [Key]
        public int tarId { get; set; }

        public string tarConcepto { get; set; }

        public string tarFormaPago { get; set; }

        public string tarPeriodicidad { get; set; }

        public string tarTipoPago { get; set; }

        public string tarMetodoPago { get; set; }

        public decimal tarCosto { get; set; }

        public bool tarEsPorcentaje { get; set; }

        public DateTime tarFechaRegistroUTC { get; set; }

    }

    #region Ligas

    public class RequestDetailViewModel
    {
        public RequestDetailViewModel()
        {
            league = new LeagueRegisterViewModel();
        }

        public LeagueRegisterViewModel league { get; set; }

        public schemaTarifas fee { get; set; }
    }

    public class RequestDetailDescuentoViewModel
    {
        public string lreTotalPagar { get; set; }
    }

    public class LeagueDetail_MainData
    {
        public LeagueDetail_MainData()
        {

        }

        public int ligId { get; set; }

        public virtual string lreImgUrl { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Nombre Liga")]
        [MaxLength(50, ErrorMessage = "Máximo de caracteres permitidos({1})")]
        public string lreNombreLiga { get; set; }

        public BusinessAddressViewModel lreBusinessAddress { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa una cuenta de correo válida.")]
        [Display(Name = "E-mail")]
        public string lreCorreoContacto { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Telefono")]
        public string lreTelefonoContacto { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [Display(Name = "Descripción")]
        public string lreDescripcion { get; set; }

        public string lreKey { get; set; }

        public bool lreStatus { get; set; }
    }

    /// <summary>
    /// Para ver el grid de las ligas
    /// </summary>
    public class LeaguesActiveLOwnerViewModel
    {
        public LeaguesActiveLOwnerViewModel()
        {
            ligMainLeague = false;
        }

        public int ligId { get; set; }

        public string ligImg { get; set; }

        public string ligNombre { get; set; }

        public string ligDomicilio { get; set; }

        public string ligContacto { get; set; }

        public string ligDescripcion { get; set; }

        public string ligCreator { get; set; }

        public string ligLatitud { get; set; }

        public string ligLongitud { get; set; }

        public bool ligMainLeague { get; set; }
    }

    /// <summary>
    /// Clase utilizada para mostrar el detalle de una liga.
    /// </summary>
    public class LeaguesActiveDetailViewModel
    {
        public LeaguesActiveDetailViewModel()
        {
            ligLiga = new LeaguesActiveLOwnerViewModel();
            ligFiles = new List<FileReferenceViewModel>();
            enableEdit = false;
        }

        public LeaguesActiveLOwnerViewModel ligLiga { get; set; }

        public List<LeagueCoAdministratorsViewModel> ligCoAdmins { get; set; }

        public List<FileReferenceViewModel> ligFiles { get; set; }

        public bool enableEdit { get; set; }

    }

    /// <summary>
    /// Clase utilizada para el file manager de las imagenes de la liga
    /// </summary>
    public class LeagueImagesEditSettingsViewModel
    {
        public LeagueImagesEditSettingsViewModel()
        {
            RootFolder = (constClass.urlLeaguesImages + "/").Replace("/", "\\");
        }

        public string RootFolder { get; set; }

        public string Model { get { return RootFolder; } }
    }

    public class LeagueGridBusquedasInicio
    {

        public int ligId { get; set; }

        public string ligCreadorId { get; set; }

        public DateTime ligFechaCreacionUTC { get; set; }

        public string ligImg { get; set; }

        public string ligNombre { get; set; }

        public string ligTipoLiga { get; set; }

        public string ligDescripcion { get; set; }

        public decimal ligCalificacion { get; set; }

        public decimal ligPrecioDesde { get; set; }

        public double ligLatitud { get; set; }

        public double ligLongitud { get; set; }

        public double ligLatLngDistancia { get; set; }

        public int ligTotalJugadores { get; set; }

    }

    #endregion

    #region Torneos

    /// <summary>
    /// Modelo de ayuda para el catalogo de categorias del torneo.
    /// </summary>
    public class TorneoCategoriasViewModel
    {
        public int lctId { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public int ligId { get; set; }

        [Required(ErrorMessage = "Campo obligatorio"), MaxLength(100)]
        public string lctNombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string depNombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public int ttoId { get; set; }

        public schemaTipoTorneos tblTipoTorneo { get; set; }

        [MaxLength(1024)]
        public string lctDescripcion { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public int? lctEdadMin { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public int? lctEdadMax { get; set; }
    }

    /// <summary>
    /// Modelo de ayuda para el catalogo de canchas de los torneos.
    /// </summary>
    
    public class CategoriaViewModel
    {
        public CategoriaViewModel()
        {
            ddlLigas = new List<SelectListItem>();
            ddlDeporte = new List<SelectListItem>();
            ddlCategorias = new List<SelectListItem>();
        }
        public int ligId { get; set; }
        public List<SelectListItem> ddlLigas { get; set; }

        public string depNombre { get; set; }
        public List<SelectListItem> ddlDeporte { get; set; }
               
        public int ttoId { get; set; }
        public virtual List<SelectListItem> ddlCategorias { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lctNombre { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lctDescripcion { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lctEdadMin { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lctEdadMax { get; set; }
    }
    public class TorneoCanchasViewModel
    {
        public TorneoCanchasViewModel()
        {
            ddlLigas = new List<SelectListItem>();
            ddlDeporte = new List<SelectListItem>();
            edit = false;
        }
        public int lcatId { get; set; }

        public int ligId { get; set; }
        public List<SelectListItem> ddlLigas { get; set; }

        public string ligaNombre { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)] 
        public string lcatNombre { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatdomicilio { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatNumExtInt { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatColonia { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatMunicipio { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatEstado { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatCodigoPostal { get; set; }

        public string depNombre { get; set; }
        public List<SelectListItem> ddlDeporte { get; set; }
        [MaxLength(1024)]
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatDescripcion { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatDireccion { get; set; }
        public string lcatLatitud { get; set; }
        public string lcatLongitud { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public string lcatDomicilio { get; set; }
        public bool edit { get; set; }
        public DateTime fechaCreacion { get; set; }
    }

    /// <summary>
    /// Modelo de ayuda para crear o editar un torneo.
    /// </summary>
    public class TorneosViewModel
    {
        public TorneosViewModel()
        {
            ddlLigas = new List<SelectListItem>();
            ddlCategorias = new List<SelectListItem>();
            tblTorneoDireccion = new schemaTorneoDireccion();
            listTarifasCfppTiposPago = new List<schemaTarifasCfppTiposPago>();

            torTipo = constClass.torTipoInterno;
            torDeporteEnEquipo = true;
            torComentarios = true;
            torEstatus = true;
            torAprobada = false;
            torPagado = false;
            torEsCoaching = false;
        }

        public int torId { get; set; }

        public string torTipo { get; set; }

        public string torImgUrl { get; set; }

        public bool torComentarios { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [MaxLength(500, ErrorMessage = constClass.maxLengthMsg)]
        public string torNombreTorneo { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int ligId { get; set; }

        [ForeignKey("ligId")]
        public virtual schemaLigas tblLiga { get; set; }

        public virtual List<SelectListItem> ddlLigas { get; set; }

        public int lctId { get; set; }

        [ForeignKey("lctId")]
        public virtual schemaLigaCategoriasTorneos tblCategoriaTorneo { get; set; }

        public virtual List<SelectListItem> ddlCategorias { get; set; }

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

        public virtual List<schemaTorneoEstructura> listTorneoEstructuras { get; set; }

        public virtual string torEstructuraDescripcion { get; set; }

        public schemaTorneoDireccion tblTorneoDireccion { get; set; }

        [MaxLength(30, ErrorMessage = constClass.maxLengthMsg)]
        public string torNumeroContacto { get; set; }

        [MaxLength(256, ErrorMessage = constClass.maxLengthMsg)]
        [EmailAddress(ErrorMessage = constClass.emailAddressMsg)]
        public string torCorreoContacto { get; set; }

        //public string torLatitud { get; set; }

        //public string torLongitud { get; set; }

        public decimal? torPrecioTorneo { get; set; }

        public int? torDiasParaPago { get; set; }

        public string torLigaFormaPago { get; set; }

        public decimal? torLigaDescuento { get; set; }

        public int tcfpptpId { get; set; }

        [ForeignKey("tcfpptpId")]
        public virtual schemaTarifasCfppTiposPago tblTarifasCfppTiposPago { get; set; }

        public virtual List<schemaTarifasCfppTiposPago> listTarifasCfppTiposPago { get; set; }

        public List<TorneosNuevoEditMetodosPagoViewModel> listTarifas { get; set; }

        public string torKey { get; set; }

        public bool torEstatus { get; set; }

        public bool torDeporteEnEquipo { get; set; }

        public bool torAprobada { get; set; }
        public bool torPagado { get; set; }
        public bool torEsCoaching { get; set; }
        public bool editForm { get; set; }
        public bool torPrivate { get; set; }
    }

    /// <summary>
    /// Clase para hacer render a la pagina de nuevos torneos y regresar los cambios.
    /// </summary>

    public class TorneosNuevoEditPostbackViewModel
    {
        public string datosPartial { get; set; }
        public string estructuraPartial { get; set; }
        public string contactoPartial { get; set; }
        public string pagosPartial { get; set; }
    }

    /// <summary>
    /// Modelo para mostrar en el formulario de un torneo los metodos de pagos.
    /// </summary>
    public class TorneosNuevoEditMetodosPagoViewModel
    {
        public int tarId { get; set; }

        public decimal tarCosto { get; set; }

        public string tmpIdMetodoPago { get; set; }

        public bool tarHabilitado { get; set; }
    }

    /// <summary>
    /// Modelo para ver los coadministradores del torneo
    /// </summary>
    public class TorneosCoAdministradoresViewModel
    {
        public TorneosCoAdministradoresViewModel()
        {
            tcaConfirmado = false;
        }

        public int tcoId { get; set; }

        public string tcaNombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo válida.")]
        public string tcaEmail { get; set; }

        public bool tcaConfirmado { get; set; }
    }

    //public class TorneosCoAdministradoresEditViewModel
    //{
    //    public TorneosCoAdministradoresEditViewModel()
    //    {
    //        tcaEmailList = new List<TorneosCoAdministradoresViewModel>();
    //    }

    //    public TorneosCoAdministradoresViewModel tcaEmail { get; set; }

    //    public List<TorneosCoAdministradoresViewModel> tcaEmailList { get; set; }
    //}

    /// <summary>
    /// Para ver el grid de los torneos
    /// </summary>
    public class TorneosGridViewModel
    {
        public TorneosGridViewModel()
        {
            torAdminTorneo = false;
            torTorneoTerminado = false;
            referee = false;
            coaching = false;
        }

        public int torId { get; set; }

        public string ligNombre { get; set; }

        public string torImg { get; set; }

        public bool torComentarios { get; set; }

        public string torNombre { get; set; }

        public schemaLigaCategoriasTorneos tblCategoria { get; set; }

        public string torDescripcion { get; set; }

        public DateTime? torFechaInicio { get; set; }

        public DateTime? torFechaFinal { get; set; }

        public DateTime? torFechaLimiteInscripcion { get; set; }

        public bool torTorneoTerminado { get; set; }

        public int? torNumeroEquipos { get; set; }

        public int? torNumeroEquiposInscritos { get; set; }

        public int? torNumeroJugadores { get; set; }

        public int? torNumeroJugadoresInscritos { get; set; }

        public decimal? torPrecioTorneo { get; set; }

        public string torCreador { get; set; }

        public bool torAdminTorneo { get; set; }
        public bool referee { get; set; }

        public bool torEstatus { get; set; }

        public bool torEnEquipo { get; set; }

        public DateTime? torFechaCreacion { get; set; }

        public decimal? torCalificacion { get; set; }

        public bool coaching { get; set; }
        public bool isPrivate { get; set; }

        public int? totalEquipo { get; set; }
    }
    public class TipoTorneoViewModel
    {
        public int ttoId { get; set; }
        [Required(ErrorMessage = "Favor de ingresar el tipo de torneo.")]
        [Display(Name = "Tipo de torneo")]
        public string ttoNombre { get; set; }

        public bool ttoEstatus { get; set; }
    }
    public class CiudadEstadoPaisViewModel
    {
        public string direccionCompleta { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
    }

    public class TorneosComentariosGridViewModel
    {
        public int tcgvId { get; set; }

        public int tcoId { get; set; }

        public int torId { get; set; }

        public string torNombre { get; set; }

        public string equNombre { get; set; }

        public string usuNombre { get; set; }

        public string tcoComentario { get; set; }

        public decimal tcoCalificacion { get; set; }

        public DateTime tcoFechaComentarioUTC { get; set; }

        public bool tcoEstatus { get; set; }
    }

    #endregion

    #region Equipos

    public class EquiposJugadoresFiltrosViewModel
    {

        public EquiposJugadoresFiltrosViewModel()
        {
            ligId = null;
            ddlLigas = new List<SelectListItem>();
            torId = null;
            ddlTorneos = new List<SelectListItem>();
            recargarImagen = false;
        }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int? ligId { get; set; }

        public List<SelectListItem> ddlLigas { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int? torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }

        public List<SelectListItem> ddlTorneos { get; set; }

        public bool mostrarDatosEquipo { get; set; }

        public bool recargarImagen { get; set; }

        public int? totalEquipo { get; set; }

        public bool coaching { get; set; }
    }

    public class EquiposJugadoresViewModel
    {
        public EquiposJugadoresViewModel()
        {
            equAdminLigaTorneos = false;
            listJugadores = new List<JugadoresViewModel>();
        }

        public int torId { get; set; }

        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }

        public int equId { get; set; }

        public string torKey { get; set; }

        public int? torEquiposRegistrados { get; set; }

        public int? torJugadoresRegistrados { get; set; }

        public int? torMaximoEquipos { get; set; }

        public int? torMaximoJugadoresEquipo { get; set; }

        public decimal? torPrecio { get; set; }

        public string equImg { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public string equNombre { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [EmailAddress(ErrorMessage = constClass.emailAddressMsg)]
        public string equCorreoAdministrador { get; set; }

        public string equCreadorEquipoId { get; set; }


        [EmailAddress(ErrorMessage = constClass.emailAddressMsg)]
        public string jugCorreo { get; set; }

        public string jugNombre { get; set; }

        public string jugUserId { get; set; }

        public List<JugadoresViewModel> listJugadores { get; set; }

        /// <summary>
        /// Indica si la persona que va a dar la alta es el admin de liga o torneos para mostrar los filtros.
        /// </summary>
        public bool equAdminLigaTorneos { get; set; }

        public bool mostrarDatosEquipo { get; set; }

        /// <summary>
        /// Indica si el usuario puede agregar coadministradores.
        /// </summary>
        public bool usuAgregarCoadmin { get; set; }

        public bool equEstatus { get; set; }

        public int equcount { get; set; }

        public bool edit { get; set; }
    }

    public class JugadorEquiposModel
    {
        public string jugCorreo { get; set; }
        public string jugNombre { get; set; }
        public string jugUserId { get; set; }
        public bool jugEstatus { get; set; }
        public bool jugConfirmado { get; set; }
        public bool jugNuevo { get; set; }
        public string jugCodigoConfirmacion { get; set; }
        public int equId { get; set; }
        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipos { get; set; }
        public int torId { get; set; }
        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneos { get; set; }
        public decimal? jugPrecioTorneo { get; set; }
        public bool jugPagado { get; set; }
        public DateTime? jugFechaVencimientoPagoUTC { get; set; }
    }

    /// <summary>
    /// Clase para hacer render a la pagina de nuevos equipos y regresar los cambios.
    /// </summary>
    public class EquiposNuevoEditPostbackViewModel
    {
        public string datosFiltros { get; set; }
        public string datosNuevoEquipo { get; set; }
        public string datosNuevoDatos { get; set; }
        public string datosAgregar { get; set; }
        public string datosParticipantes { get; set; }
        public string datosEquipoCoach { get; set; }
        public bool mostrarDatosEquipo { get; set; }
        public bool datSinTorneos { get; set; }
        public bool recargarPartialCompleto { get; set; }
        public int numequipos { get; set; }
        public bool esCoaching { get; set; }
    }

    public class JugadoresViewModel
    {
        public JugadoresViewModel()
        {
            jugEstatus = true;
            jugConfirmado = false;
            jugNuevo = false;
        }

        public string jugCorreo { get; set; }
        public string jugNombre { get; set; }
        public string jugUserId { get; set; }
        public bool jugEstatus { get; set; }
        public bool jugConfirmado { get; set; }
        public bool jugNuevo { get; set; }
        public bool jugCodConfirmacion { get; set; }
    }

    public class EquiposJugadoresTotalTorneos
    {
        public int totalEquipos { get; set; }
        public int totalJugadores { get; set; }
    }
    // view estadistas de jugadores equipo
    public class TorneoEstGoleador
    {
        public string jugId { get; set; }
        public string jugNombre { get; set; }
        public int equId { get; set; }
        public int goles { get; set; }
        public string equNombre { get; set; }
        public int faltas { get; set; }
        public int amarillas { get; set; }
        public int rojas { get; set; }
        public int pos { get; set; }
        public string equipo { get; set; }
        public int parId { get; set; }
    }
    public class TablaGeneralTorneo
    {
        public int equId { get; set; }
        public int torId { get; set; }
        public string equNombre { get; set; }
        public int puntos { get; set; }
        public int parJugados { get; set; }
        public int parGanados { get; set; }
        public int parEmpatados { get; set; }
        public int parPerdidos { get; set; }
        public int golFavor { get; set; }
        public int golContra { get; set; }
        public int difGoles { get; set; }
        public int scoreUno { get; set; }
        public int scoreDos { get; set; }
        public int partidosRest { get; set; }
        public int pos { get; set; }
    }
    public class EstadisticasTorneo
    {

    }
    public class AgregarJugadorEquipoViewModel
    {
        public AgregarJugadorEquipoViewModel()
        {
            listJugadores = new List<JugadoresViewModel>();
        }

        [Required(ErrorMessage = constClass.requiredMsg)]
        [EmailAddress(ErrorMessage = constClass.emailAddressMsg)]
        public string jugCorreo { get; set; }

        public string jugNombre { get; set; }

        public string jugUserId { get; set; }

        public List<JugadoresViewModel> listJugadores { get; set; }
    }

    public class JugadoresEquipoGridViewModel
    {
        public JugadoresEquipoGridViewModel()
        {
            jug1 = new JugadorPerfilViewModel();
            jug2 = new JugadorPerfilViewModel();
        }

        public int rowId { get; set; }
        public JugadorPerfilViewModel jug1 { get; set; }
        public JugadorPerfilViewModel jug2 { get; set; }
        public bool edit { get; set; }
    }

    public class JugadorPerfilViewModel
    {
        public string jugImg { get; set; }
        public string jugNombre { get; set; }
        public string jugGenero { get; set; }
        public DateTime? jugFechaNacimiento { get; set; }
        public string jugCiudad { get; set; }
        public string jugTelefono { get; set; }
        public string jugCorreo { get; set; }
        public bool jugConfirmado { get; set; }
        public bool jugNuevo { get; set; }
        public bool jugEstatus { get; set; }
        public bool jugCodConfirmacion { get; set; }
    }
    public class EquiposJugadoresGuardarResultado
    {
        public bool result { get; set; }
        public string errorTipo { get; set; }
        public string mensaje { get; set; }

        public int equId { get; set; }
    }

    /// <summary>
    /// Modelo para ver los coadministradores del torneo
    /// </summary>
    public class EquiposCoAdministradoresViewModel
    {
        public EquiposCoAdministradoresViewModel()
        {
            ecaConfirmado = false;
        }

        public string ecaNombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo válida.")]
        public string ecaEmail { get; set; }

        public bool ecaConfirmado { get; set; }

        public bool ecaEstatus { get; set; }
    }


    #endregion
    public class ResultadoPartido
    {
        public ResultadoPartido()
        {
            ddlJugUno = new List<schemaJugadorEquipos>();
            ddlJugDos = new List<schemaJugadorEquipos>();
        }
        public List<schemaJugadorEquipos> ddlJugUno { get; set; }
        public List<schemaJugadorEquipos> ddlJugDos { get; set; }
        public int Id { get; set; }
        public string equNombreEquipoUno { get; set; }
        public string equNombreEquipoDos { get; set; }
        public int equIdUno { get; set; }
        public int equIdDos { get; set; }
        public int goles { get; set; }
        public int asistencias { get; set; }
        public int faltas { get; set; }
        public int amarillas { get; set; }
        public int roja { get; set; }
        public int parSuspendido { get; set; }

    }
    public class PartidosViewModel
    {
        public PartidosViewModel()
        {
            ddlLigas = new List<SelectListItem>();
            ddlTorneos = new List<SelectListItem>();
            ddlCanchas = new List<SelectListItem>();
            ddlEquipoUno = new List<SelectListItem>();
            ddlEquipoDos = new List<SelectListItem>();
            ddlDeportes = new List<SelectListItem>();
            ddlArbitros = new List<SelectListItem>();
            ddlJugUno = new List<schemaJugadorEquipos>();
            ddlJugDos = new List<schemaJugadorEquipos>();
            dep = false;
            result = false;
            parEstadistica = false;
            lat = 0;
            lng = 0;
        }
        public int parId { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int ligId { get; set; }
        public List<SelectListItem> ddlLigas { get; set; }

        [Required(ErrorMessage = constClass.requiredMsg)]
        public int torId { get; set; }
        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }
        public List<SelectListItem> ddlTorneos { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int canId { get; set; }
        [ForeignKey("canId")]
        public virtual schemaLigaCanchasTorneos tblLigaCanchasTorneos { get; set; }
        public List<SelectListItem> ddlCanchas { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int equIdUno { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public int equIdDos { get; set; }
        [ForeignKey("equId")]
        public virtual schemaEquipos tblEquipo { get; set; }
        public List<SelectListItem> ddlEquipoUno { get; set; }
        public List<SelectListItem> ddlEquipoDos { get; set; }
        public string depNombre { get; set; }
        [ForeignKey("depNombre")]
        public virtual schemaDeportes tblDeportes { get; set; }
        public List<SelectListItem> ddlDeportes { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]

        public int arbId { get; set; }
        [ForeignKey("arbId")]
        public virtual schemaArbitros tblArbitros { get; set; }
        public List<SelectListItem> ddlArbitros { get; set; }
        [Required(ErrorMessage = constClass.requiredMsg)]
        public List<schemaJugadorEquipos> ddlJugUno { get; set; }
        public List<schemaJugadorEquipos> ddlJugDos { get; set; }
        public string userId { get; set; }
        public DateTime parFecha_Inicio { get; set; }
        public DateTime parFecha_Fin { get; set; }
        public int parHour { get; set; }
        public int parMinutes { get; set; }
        public string equNombreEquipoUno { get; set; }
        public string equNombreEquipoDos { get; set; }
        public string arUserId { get; set; }
        public string arbNombre { get; set; }
        public string canNombre { get; set; }
        public string titulo { get; set; }
        public string imgUno { get; set; }
        public string imgDos { get; set; }
        public string ligNombre { get; set; }
        public string torNombre { get; set; }
        public Boolean dep { get; set; }
        public Boolean result { get; set; }
        public string colDeporte { get; set; }
        public int equResultadoUno { get; set; }
        public int equResultadoDos { get; set; }
        [MaxLength(100)]
        public string parEstado { get; set; }
        public string torTipo { get; set; }
        public int Id { get; set; }
        public int numPartidos { get; set; }
        public bool parEstatus { get; set; }
        public string canchaDireccion { get; set; }
        public bool parEstadistica { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public bool parCheck {get;set;}
    }
    public class Events
    {
        public string id { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string url { get; set; }

        public bool allDay { get; set; }
    }

    public class FiltroLigasTorneosViewModel
    {
        public FiltroLigasTorneosViewModel()
        {
            ddlLigas = new List<SelectListItem>();
            ddlTorneos = new List<SelectListItem>();
        }
        public int ligId { get; set; }
        public virtual schemaLigas tblLiga { get; set; }
        public List<SelectListItem> ddlLigas { get; set; }
        public int torId { get; set; }
        [ForeignKey("torId")]
        public virtual schemaTorneos tblTorneo { get; set; }
        public List<SelectListItem> ddlTorneos { get; set; }
        public Boolean viewTor { get; set; }
    }
    public class ArbitrosViewModel {
        public int arbId { get; set; }
        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "Favor de ingresar una cuenta de correo valida")]
        public string arbCorreo { get; set; }
        [Required(ErrorMessage = "Favor de ingresar el correo.")]
        [Display(Name = "Nombre")]
        public string arbNombre { get; set; }
        public string estado { get; set; }
        public string torNombre { get; set; }
        public string ligNombre { get; set; }
        public int ligId { get; set; }
        public int parId { get; set; }
        public string partido { get; set; }
    }

    #region Pagos
    public class PagosGridViewModel
    {
        public int id { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public int ligId { get; set; }
        public string ligNombre { get; set; }
        public string total { get; set; }
        public string estado { get; set; }
        public string conceptoNombre { get; set; }
        public int conceptoId { get; set; }
        public string adminView { get; set; }
        public string path { get; set; }
        public string fechaPago { get; set; }
    }
    public class CustViewModel
    {
        public string fname { get; set; }
        public string mname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string addr { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public int zip { get; set; }

        public decimal total { get; set; }
    }
    public class PostBanwire
    {
        public string events { get; set; }
        public string status { get; set; }
        public string auth_code { get; set; }
        public string reference { get; set; }
        public string id { get; set; }
        public decimal total { get; set; }
        public string hash { get; set; }
        public string plan { get; set; }
        public string type { get; set; }
        public string no_payment { get; set; }
    }
    public class PagosViewModel
    {
        public int ligId { get; set; }
        public string userId { get; set; }
        public string ciudad { get; set; }
        public string colonia { get; set; }
        public string token { get; set; }
        public string cp { get; set; }
        public string estado { get; set; }
        public string nombre { get; set; }
        public string pais { get; set; }
        public string street { get; set; }
        public decimal total { get; set; }
        public string tel { get; set; }
        public string email { get; set; }
        public string item { get; set; }
        public string concepto { get; set; }
        public int conceptoId { get; set; }
    }
    public class DetallePagoViewModel
    {
        public int IdPago { get; set; }
        public string conceptoPago { get; set; }
        public decimal total { get; set; }
        public string IdTransaccion { get; set; }
        public string referencia { get; set; }
        public string metodoPago { get; set; }
        public string status { get; set; }
        public string ipAddress { get; set; }
        //public DateTime Fecha { get; set}
        public string concepto { get; set; }
        public string conceptoNombre { get; set; }
        public string userNombre { get; set; }
        public string fechaPago { get; set; }
    }
    #endregion
    public class data
    {
        public string id { get; set; }
        public string livemode { get; set; }
        public string created_at { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string reference_id { get; set; }
        public List<payment> payment_method { get; set; }
        public List<details> details { get; set; }
        public string amount { get; set; }
        public string paid_at { get; set; }
        public string fee { get; set; }
    }
    public class payment
    {
        public string expiry_date { get; set; }
        public string barcode { get; set; }
        public string barcode_url { get; set; }
        public string type { get; set; }
		public string expires_at { get; set; }
    }
    public class details
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public List<items> line_items { get; set; }
        public List<billing> billing_address { get; set; }
    }
    public class items
    {
        public string name { get; set; }
        public string description { get; set; }
        public string unit_price { get; set; }
        public string quantity { get; set; }
        public string sku { get; set; }
        public string category { get; set; }
    }
    public class billing
    {
        public string street1 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
    }
    public class UserAux
    {
        /// <summary>
        /// A User's username. eg: "sergiotapia, mrkibbles, matumbo"
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// A User's name. eg: "Sergio Tapia, John Cosack, Lucy McMillan"
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A User's location. eh: "Bolivia, USA, France, Italy"
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("endorsements")]
        public int Endorsements { get; set; } //Todo.

        [JsonProperty("team")]
        public string Team { get; set; } //Todo.

        /// <summary>
        /// A collection of the User's linked accounts.
        /// </summary>
        [JsonProperty("accounts")]
        public Account Accounts { get; set; }

        /// <summary>
        /// A collection of the User's awarded badges.
        /// </summary>
        [JsonProperty("badges")]
        public List<Badge> Badges { get; set; }
    }

    public class Account
    {
        public string github;
    }

    public class Badge
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("description")]
        public string Description;
        [JsonProperty("created")]
        public string Created;
        [JsonProperty("badge")]
        public string BadgeUrl;
    }
    public class ViewModelAvisos
    {
        public int aId { get; set; }
        public string aviso { get; set; }
        public DateTime fecha_Registro { get; set; }
    }

    public class ViewClients
    {
        public string roles { get; set; }
        public string userId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber{ get; set; }

        public DateTime created_at { get; set; }
    }
}