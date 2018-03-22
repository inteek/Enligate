using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sw_EnligateWeb.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using sw_EnligateWeb.Models.HelperClasses;
using sw_EnligateWeb.Engine;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using DevExpress.Web.Mvc;

namespace sw_EnligateWeb.Controllers
{
    //[Authorize(Roles=constClass.rolAdmin)]
    public class AdministratorController : Controller
    {
        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdministratorController()
        {

        }

        public AdministratorController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #endregion

        #region Index

        /// <summary>
        /// Acción que regresa la vista del dashboard del administrador de enligate.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var model = new IndexAdministratorViewModel();
            model.iadTotalNuevasSolicitudes = db.getLeaguesRequestNew().Count(); 

            return View(model);
        }

        #region Solicitudes de Liga

        /// <summary>
        /// Acción para cargar la pantalla con el grid de las ultimas solicitudes.
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexLastRequestsGrid()
        {
            return PartialView("Administrator/_IndexLastRequestsGrid");
        }

        /// <summary>
        /// Acción que se llama para cargar el grid de las ultimas solicitudes de liga.s
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _IndexLastRequestsGrid()
        {
            List<RequestLeaguesViewModel> model = new List<RequestLeaguesViewModel>();
            model = db.getLeaguesRequestNew().ToList();

            return PartialView("Administrator/_IndexLastRequestsGrid", model);
        }

        #endregion

        #region Usuarios en pruebas

        /// <summary>
        /// Acción para cargar la pantalla con el grid de los ultimos usuarios en prueba.
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexLastUsersTestingGrid()
        {
            return PartialView("Administrator/_IndexLastUsersTestingGrid");
        }

        /// <summary>
        /// Acción que se llama para cargar el grid de los ultimos usuarios en prueba
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _IndexLastUsersTestingGrid()
        {
            List<UsersTestingViewModel> model = new List<UsersTestingViewModel>();
            string[] nombres = {"Humberto Cortes","Ricardo Bahena","Claudia Jimenez","Miguel Lopez",
                                    "Jose Gutierrez","Luis Fernandez","Daniel Gonzalez","Raul Bonilla",
                                    "Fernada Perez","Marcela Garcia","Jose Marquez","Martin Torres",
                                    "Monica Salinas","Diego Cardona","Fernando Granados","Mario Barbena"};
            int count = 1;
            int rep = 3;
            while (rep > 0)
            {
                Random rnd = new Random();
                foreach (string nombre in nombres)
                {
                    int dias = rnd.Next(0, 13);
                    var item = new UsersTestingViewModel();
                    item.uteId = count;
                    item.uteNombre = nombre;
                    item.uteDiasRestantes = dias;
                    count++;
                    model.Add(item);
                }
                rep--;
            }


            return PartialView("Administrator/_IndexLastUsersTestingGrid", model);
        }

        #endregion

        #region Ultimos pagos

        /// <summary>
        /// Acción para cargar la pantalla con el grid de los ultimos pagos.
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexLastPaymentsGrid()
        {
            return PartialView("Administrator/_IndexLastPaymentsGrid");
        }

        /// <summary>
        /// Acción que se llama para cargar el grid de los ultimos pagos.
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _IndexLastPaymentsGrid()
        {
            List<PaymentsViewModel> model = new List<PaymentsViewModel>();
            
            model = db.getUltimosPagos();
            return PartialView("Administrator/_IndexLastPaymentsGrid", model.OrderByDescending(l => l.payFecha).ToList());
        }

        #endregion

        #region Ultimas noticias

        /// <summary>
        /// Acción para cargar la pantalla con el grid de las ultimas noticias
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexLastNewsGrid()
        {
            return PartialView("Administrator/_IndexLastNewsGrid");
        }

        /// <summary>
        /// Acción que se llama para cargar el grid de los ultimas noticias.
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _IndexLastNewsGrid()
        {
            List<NewsViewModel> model = new List<NewsViewModel>();

            string[] contenidos = {"Finalizo torneo varonil de la Liga Bayern","La escuela mty ha registrado su primer torneo",
                                       "Finalizo torneo infantil de la Liga Bayern","La liga León ha registrado su primer torneo"};
            int count = 1;
            int rep = 1;
            while (rep > 0)
            {
                foreach (string contenido in contenidos)
                {
                    var item = new NewsViewModel();
                    item.newId = count;
                    item.newImageUrl = null;
                    item.newContent = contenido;
                    item.newActionUrl = "#";
                    item.newActionUrlContent = ((count % 2) == 0) ? "Felicitalos" : "Ver Resultados";
                    count++;
                    model.Add(item);
                }
                rep--;
            }

            return PartialView("Administrator/_IndexLastNewsGrid",model);
        }

        #endregion

        #endregion

        #region Perfil

        /// <summary>
        /// Realiza la carga de la página del perfil del administrador
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Perfil()
        {
            var model = new AdminController().getPerfilUsuario(User.Identity.Name);

            return View("Perfil", model);
        }

        #endregion
        #region TipoTorneo
        [HttpGet]
        public ActionResult TipoTorneo()
        {
            
            //var model = new AdminController().getPerfilUsuario(User.Identity.Name);

            return View("Catalog");
        }
        [ValidateInput(false)]
        public ActionResult _TorneoTiposGridEdit_Callback()
        {
            var model = db.getTiposTorneo_Active();

            return PartialView("Torneos/_TorneoTipoGrid",model);
        }
        protected TipoTorneoViewModel clearGridCommasTipoTorneos(TipoTorneoViewModel item)
        {
            item.ttoNombre = (item.ttoNombre != null) ? item.ttoNombre.Replace("\"", "").Trim() : null;
            return item;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _TorneoTiposGridEdit_AddNewPartial(TipoTorneoViewModel item)
        {
            item = clearGridCommasTipoTorneos(item);
            if (ModelState.IsValid)
            {
                if (db.setTipoTorneos_Add(item))
                {
                    ModelState.Clear();
                }
                else
                {
                    ViewData["EditError"] = "Hubo un error guardando la categoria";
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
            }

            var model = db.getTiposTorneo_Active();

            return PartialView("Torneos/_TorneoTipoGrid", model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _TorneoTiposGridEdit_UpdatePartial(TipoTorneoViewModel item)
        {

           if (ModelState.IsValid)
            {
                item = clearGridCommasTipoTorneos(item);
                if (db.setTipoTorneos_Edit(item))
                {
                    ModelState.Clear();
                }
                else
                {
                    ViewData["EditError"] = "Hubo un error guardando la categoria";
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
            }

            var model = db.getTiposTorneo_Active();

            return PartialView("Torneos/_TorneoTipoGrid", model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _TorneoTiposGridEdit_Delete(int ttoId)
        {
            if (db.setTipoTorneos_Delete(ttoId))
            {
                ModelState.Clear();
                //ViewData["gvtCategoriasCallback"] = "ok";
            }
            else
            {
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
            }

            var model = db.getTiposTorneo_Active();

            return PartialView("Torneos/_TorneoTipoGrid", model);
        }

        #endregion
        #region Leagues

        /// <summary>
        /// Regresa la vista para la pantalla de ligas inscritas
        /// </summary>
        /// <returns></returns>
        public ActionResult Leagues()
        {
            return View(new RequestLeaguesFilterViewModel());
        }

        /// <summary>
        /// Regresa la vista del grid de las ligas inscritas
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaguesGrid()
        {
            return PartialView("Administrator/_LeaguesGrid");
        }

        /// <summary>
        /// Acción que regresa los registros del grid de ligas inscritas
        /// </summary>
        /// <param name="leaFechaIni"></param>
        /// <param name="leaFechaFin"></param>
        /// <param name="leaTipoLiga"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _LeaguesGrid(string leaFechaIni, string leaFechaFin, string leaTipoLiga)
        {
            List<RequestLeaguesViewModel> model = new List<RequestLeaguesViewModel>();

            DateTime? dtStart = Global_Functions.stringToDate(leaFechaIni);
            DateTime? dtEnd = Global_Functions.stringToDate(leaFechaFin, true);

            if (dtStart != null && dtEnd != null)
            {
                model = db.getLeaguesRegistered((DateTime)dtStart, (DateTime)dtEnd, leaTipoLiga);
            }

            return PartialView("Administrator/_LeaguesGrid", model);
        }

        #endregion

        /// <summary>
        /// Accion que devuelve la pantalla de publicidad.
        /// Esta pantalla aun no se diseña por lo que esta oculta.
        /// </summary>
        /// <returns></returns>
        public ActionResult Advertising()
        {
            return View();
        }

        #region Solicitudes

        /// <summary>
        /// Acción que muestra la pantalla de Requests para cargar el grid con las solicitudes de ligas.
        /// </summary>
        /// <returns></returns>
        public ActionResult Requests()
        {
            return View(new RequestLeaguesFilterViewModel());
        }

        /// <summary>
        /// Accion que muestra la pantalla de Solicitudes de liga desde otra pantalla por metodo post y carga el detalle de dicha solicitud.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Requests(int Id)
        {
            var model = new RequestLeaguesFilterViewModel();
            model.ligId = Id;
            return View(model);
        }

        /// <summary>
        /// Acción que regresa la vista donde se encuentra el grid de las solicitudes
        /// </summary>
        /// <returns></returns>
        public ActionResult RequestsGrid()
        {
            return PartialView("Administrator/_RequestsGrid");
        }
        public ActionResult ClientsGrid()
        {
            return PartialView("Administrator/_ClientsGrid");
        }
        public ActionResult _ClientsGrid(string rol, string date_start, string date_end, string search_user)
        {
            var model = db.getClients().Where(l => l.roles.ToUpper().Equals(rol.ToUpper())).ToList();

            if (date_start != null){
                model=model.Where(l => l.created_at <= DateTime.Parse(date_start) && l.created_at >= DateTime.Parse(date_end)).ToList();
            }
            /*
            if (search_user!="")
            {
                model = model.Where(l => l.Email.ToUpper() == search_user.ToUpper()).ToList();
            }*/
            //var date = DateTime.Parse(date_start);
            return PartialView("Administrator/_ClientsGrid",model);

        }
        /// <summary>
        /// Acción que se ejecuta cada vez que se llenan datos en el grid de solicitudes de liga.
        /// </summary>
        /// <param name="reqFechaIni"></param>
        /// <param name="reqFechaFin"></param>
        /// <param name="reqSolicitudTipoFiltro"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _RequestsGrid(string reqFechaIni, string reqFechaFin, string reqSolicitudTipoFiltro)
        {
            List<RequestLeaguesViewModel> model = new List<RequestLeaguesViewModel>();

            int reqEstatus;
            switch(reqSolicitudTipoFiltro)
            {
                case "N":
                    reqEstatus = 0;
                    break;
                case "A":
                    reqEstatus = 1;
                    break;
                case "R":
                    reqEstatus = 3;
                    break;
                default:
                    reqEstatus = 2;
                    break;
            }

            DateTime? dtStart = Global_Functions.stringToDate(reqFechaIni);
            DateTime? dtEnd = Global_Functions.stringToDate(reqFechaFin,true);
            if (dtStart != null && dtEnd != null)
            {
                model = db.getLeaguesRequest((DateTime)dtStart, (DateTime)dtEnd, reqEstatus);
            }
            
            return PartialView("Administrator/_RequestsGrid", model);
        }

        /// <summary>
        /// Acción que se llama para mostrar el detalle de una solicitud.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _RequestsDetail(int Id)
        {
            ViewBag.afa_LigaCategorias = db.getLigaCategorias_Active();

            var model = new RequestDetailViewModel();
            model = db.getLeagueRequestById(Id);
            model.league.lreAllowAddAdmins = false;
            model.league.lreAddingLeague = false;

            // model.league = new HomeController().getAddLeagueFormPayments(model.league, "periodicidadPago", model.league.lreTipoLiga, model.league.lreFormaPago, model.league.lrePeriodicidadPago);
            model.league.lreTipoLiga = model.league.lreTipoLiga;
            model.league .lreDdlTiposLiga = db.getLigaCategorias_Active();
            return PartialView("Administrator/_RequestsDetail", model);
        }

        /// <summary>
        /// Accion que aplica el descuento de la liga en revisión.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="descuento"></param>
        /// <returns></returns>
        public JsonResult _RequestDetail_Descuento(int ligId, int tarId, decimal descuento)
        {
            var league = db.getLigaById(ligId);
            var fee = db.getFeeById(tarId);

            decimal rdeCostoTotal = calculaCostoTotalDescuento(fee.tarCosto, descuento);

            var result = new RequestDetailDescuentoViewModel();
            if (fee.tarEsPorcentaje)
                result.lreTotalPagar = rdeCostoTotal.ToString() + " % ";
            else
                result.lreTotalPagar = rdeCostoTotal.ToString("C");

            return Json(result);
        }

        /// <summary>
        /// Calcula el total a pagar, después del descuento.
        /// </summary>
        /// <param name="tarCosto"></param>
        /// <param name="descuento"></param>
        /// <returns></returns>
        public decimal calculaCostoTotalDescuento(decimal tarCosto, decimal descuento)
        {
            decimal rdeCostoTotal = tarCosto;
            if (descuento != 0)
            {
                decimal importe = (tarCosto * descuento) / 100;
                rdeCostoTotal = rdeCostoTotal + importe;
            }

            return rdeCostoTotal;
        }

        /// <summary>
        /// Acción que se ejecuta para aprobar una solicitud de liga.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _RequestAccept(RequestDetailViewModel model)
        {
            //if (model.league.lreId > 0 && model.fee.tarId > 0)
            if (model.league.lreId > 0 )
            {
                //var fee = db.getFeeById(model.fee.tarId);
                //model.league.lreTotalPagar = calculaCostoTotalDescuento(fee.tarCosto, model.league.lrePorcentajeDescuento);

                if (db.setLeagueRequestAccept(UserManager, model))
                {
                    enviarEmailAceptacionSolicitudLiga(db.getLigaById(model.league.lreId));
                    ModelState.AddModelError(constClass.success, "La solicitud ha sido aceptada.");
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error aceptado la solicitud. Intenta nuevamente.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.info, "Hubo un error, intente recargando la página.");
            }

            return PartialView("_ModalState_Errors");
        }

        /// <summary>
        /// Acción que se ejecuta para rechazar una solicitud de liga
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _RequestReject(RequestDetailViewModel model)
        {
            if(db.setLeagueRequestReject(model.league.lreId))
            {
                enviarEmailRechazoSolicitudLiga(db.getLigaById(model.league.lreId));
                ModelState.AddModelError(constClass.success, "La solicitud ha sido rechazada.");
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error rechazando la solicitud. Intenta nuevamente.");
            }

            return PartialView("_ModalState_Errors");
        }

        /// <summary>
        /// Envia los correos de aceptacion de solicitud de liga al dueño de la liga.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailAceptacionSolicitudLiga(schemaLigas liga)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var user = liga.tblUserCreador;
                var prof = db.getUserMainProfile(user.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = user.Email;
                string body = Global_Functions.getBodyHTML("~/Emails/SolicitudLigaAceptacion.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(user.Email, siteConfig.scoSenderDisplayEmailName, "Aceptación de Solicitud de Liga", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                if (mailSended)
                    return true;
            }


            return false;
        }

        /// <summary>
        /// Envia los correos de rechazo de solicitud de liga al dueño de la liga.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailRechazoSolicitudLiga(schemaLigas liga)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var user = liga.tblUserCreador;
                var prof = db.getUserMainProfile(user.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = user.Email;
                string body = Global_Functions.getBodyHTML("~/Emails/SolicitudLigaRechazo.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(user.Email, siteConfig.scoSenderDisplayEmailName, "Rechazo de Solicitud de Liga", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                if (mailSended)
                    return true;
            }


            return false;
        }

        #endregion

        #region Fees

        /// <summary>
        /// Acción que carga la vista de Tarifas
        /// </summary>
        /// <returns></returns>
        public ActionResult Fees()
        {
            getFeesDropdownlists();
            return View();
        }

        /// <summary>
        /// Metodo para poder cargar los Dropdownlist de la pantalla de tarifas
        /// </summary>
        protected void getFeesDropdownlists()
        {
            var ddlConcepto = getFeesDdlConcepto();
            
            string concepto = ddlConcepto.FirstOrDefault() != null ? ddlConcepto.FirstOrDefault().Value : "";
            var ddlFormaPago = getFeesDdlFormaPago(concepto);

            int tcfpId = ddlFormaPago.FirstOrDefault() != null ? int.Parse(ddlFormaPago.FirstOrDefault().Value) : 0;
            var ddlPeriodicidad = getFeesDdlPeriodicidad(tcfpId);

            int tcfppId = ddlPeriodicidad.FirstOrDefault() != null ? int.Parse(ddlPeriodicidad.FirstOrDefault().Value) : 0;
            var ddlTipoPago = getFeesDdlTipoPago(tcfppId);

            int tcfpptpId = ddlTipoPago.FirstOrDefault() != null ? int.Parse(ddlTipoPago.FirstOrDefault().Value) : 0;
            var ddlMetodoPago = getFeesDdlMetodoPago(tcfpptpId);

            ViewBag.ddlConcepto = ddlConcepto;
            ViewBag.ddlFormaPago = ddlFormaPago;
            ViewBag.ddlPeriodicidad = ddlPeriodicidad;
            ViewBag.ddlTipoPago = ddlTipoPago;
            ViewBag.ddlMetodoPago = ddlMetodoPago;
        }

        /// <summary>
        /// Metodo que regresa la lista de los conceptos
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> getFeesDdlConcepto()
        {
            return db.getFeesConcepto();
        }

        /// <summary>
        /// Metodo que regresa la lista de las formas de pago que pertenecen al concepto
        /// </summary>
        /// <param name="concepto"></param>
        /// <returns></returns>
        protected List<SelectListItem> getFeesDdlFormaPago(string concepto)
        {
            //string concepto = ddlConcepto.FirstOrDefault() != null ? ddlConcepto.FirstOrDefault().Value : "";
            return db.getFeesFormaPago(concepto);
        }

        /// <summary>
        /// Metodo que trae la lista de la periodicidad de la forma de pago
        /// </summary>
        /// <param name="tcfpId"></param>
        /// <returns></returns>
        protected List<SelectListItem> getFeesDdlPeriodicidad(int tcfpId)
        {
            //int tcfpId = ddlFormaPago.FirstOrDefault() != null ? int.Parse(ddlFormaPago.FirstOrDefault().Value) : 0;
            return db.getFeesPeriodicidad(tcfpId);
        }

        /// <summary>
        /// Metodo que regresa los tipos de pagos dependiendo de la periodicidad
        /// </summary>
        /// <param name="tcfppId"></param>
        /// <returns></returns>
        protected List<SelectListItem> getFeesDdlTipoPago(int tcfppId)
        {
            //int tcfppId = ddlPeriodicidad.FirstOrDefault() != null ? int.Parse(ddlPeriodicidad.FirstOrDefault().Value) : 0;
            return db.getFeesTipoPago(tcfppId);
        }

        /// <summary>
        /// Metodo que regresa la lista de los metodos de pago para el tipo de pago seleccionado.
        /// </summary>
        /// <param name="tcfpptpId"></param>
        /// <returns></returns>
        protected List<SelectListItem> getFeesDdlMetodoPago(int tcfpptpId)
        {
            //int tcfpptpId = ddlTipoPago.FirstOrDefault() != null ? int.Parse(ddlTipoPago.FirstOrDefault().Value) : 0;
            return db.getFeesMetodoPago(tcfpptpId);
        }

        /// <summary>
        /// Acción que se ejecuta cuando se cambia la seleccion de un Dropdownlist en la vista de tarifas.
        /// Regresa los datos en cascada de los demás campos.
        /// </summary>
        /// <param name="ddlCampoNombre"></param>
        /// <param name="ddlValor"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _FeesFormOnChangeDdl(string ddlCampoNombre, string ddlValor)
        {
            List<SelectListItem> ddlFormaPago, ddlPeriodicidad, ddlTipoPago, ddlMetodoPago;
            int tcfpId, tcfppId, tcfpptpId;

            switch(ddlCampoNombre)
            {
                case "concepto":
                    ddlFormaPago = getFeesDdlFormaPago(ddlValor);

                    tcfpId = (ddlFormaPago.FirstOrDefault() != null) ? int.Parse(ddlFormaPago.FirstOrDefault().Value) : 0;
                    ddlPeriodicidad = getFeesDdlPeriodicidad(tcfpId);

                    tcfppId = (ddlPeriodicidad.FirstOrDefault() != null) ? int.Parse(ddlPeriodicidad.FirstOrDefault().Value) : 0;
                    ddlTipoPago = getFeesDdlTipoPago(tcfppId);

                    tcfpptpId = (ddlTipoPago.FirstOrDefault()) != null ? int.Parse(ddlTipoPago.FirstOrDefault().Value) : 0;
                    ddlMetodoPago = getFeesDdlMetodoPago(tcfpptpId);

                    return Json(new
                    {
                        formaPago = ddlFormaPago,
                        periodicidad = ddlPeriodicidad,
                        tipoPago = ddlTipoPago,
                        metodoPago = ddlMetodoPago
                    });
                case "formaPago":
                    ddlPeriodicidad = getFeesDdlPeriodicidad(int.Parse(ddlValor));

                    tcfppId = (ddlPeriodicidad.FirstOrDefault() != null) ? int.Parse(ddlPeriodicidad.FirstOrDefault().Value) : 0;
                    ddlTipoPago = getFeesDdlTipoPago(tcfppId);

                    tcfpptpId = (ddlTipoPago.FirstOrDefault()) != null ? int.Parse(ddlTipoPago.FirstOrDefault().Value) : 0;
                    ddlMetodoPago = getFeesDdlMetodoPago(tcfpptpId);
                    return Json(new
                    {
                        periodicidad = ddlPeriodicidad,
                        tipoPago = ddlTipoPago,
                        metodoPago = ddlMetodoPago
                    });
                case "periodicidad":
                    ddlTipoPago = getFeesDdlTipoPago(int.Parse(ddlValor));

                    tcfpptpId = (ddlTipoPago.FirstOrDefault()) != null ? int.Parse(ddlTipoPago.FirstOrDefault().Value) : 0;
                    ddlMetodoPago = getFeesDdlMetodoPago(tcfpptpId);
                    return Json(new
                    {
                        tipoPago = ddlTipoPago,
                        metodoPago = ddlMetodoPago
                    });
                case "tipoPago":
                    ddlMetodoPago = getFeesDdlMetodoPago(int.Parse(ddlValor));
                    return Json(new
                    {
                        metodoPago = ddlMetodoPago
                    });
                case "metodoPago":
                    break;
            }

            return null;
        }

        /// <summary>
        /// Acción que se ejecuta en el boton de guardar de la vista de tarifas
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FeesForm(TarifasViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(db.setFees(model))
                    ModelState.AddModelError(constClass.success, "Se guardo la tarifa.");
                else
                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la Tarifa. Intenta nuevamente.");
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Favor de validar los campos obligatorios.");
            }

            return PartialView("_ModalState_Errors");
        }

        /// <summary>
        /// Acción que regresa la vista del grid de las tarifas guardadas.
        /// </summary>
        /// <returns></returns>
        public ActionResult FeesGrid()
        {
            return PartialView("Administrator/_FeesGrid");
        }

        /// <summary>
        /// Acción que se ejecuta para llenar el grid de las tarifas
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _FeesGrid()
        {
            List<TarifasGridViewModel> model = new List<TarifasGridViewModel>();
            if (Request.IsAjaxRequest())
            {
                model = db.getFees()
                          .OrderBy(c => c.tarConcepto)
                          .ThenBy(fp => fp.tarFormaPago)
                          .ThenBy(p => p.tarPeriodicidad)
                          .ThenBy(tp => tp.tarTipoPago)
                          .ThenBy(mp => mp.tarMetodoPago)
                          .ToList();
            }

            return PartialView("Administrator/_FeesGrid", model);
        }

        #endregion


        public ActionResult Client()
        {
            return View();
        }

        /// <summary>
        /// Acción que devuelve la vista de la pantalla de historial de pagos
        /// </summary>
        /// <returns></returns>
        public ActionResult FeesPayments()
        {
            return View();
        }

        /// <summary>
        /// Acción que devuelve la vista de la pantalla de pagos de publicidad
        /// </summary>
        /// <returns></returns>
        public ActionResult FeesAdvertising()
        {
            return View();
        }

        public int Obtener()
        {
            //int total = db.getClients().Where(l => l.roles.ToUpper() != "12d92a00-4392-4a0a-aca5-1a979a86fb8e".ToUpper()).Count();
            int total = db.ObtenerUsers();
            return total;
        }

        public int ObtenerSumas(string id)
        {
            int total = db.ObtenerClientes(id);
            return total;
        }
    }
}