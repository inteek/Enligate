using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sw_EnligateWeb.Models;
using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models.HelperClasses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System.IO;

namespace sw_EnligateWeb.Controllers
{
    public class HomeController : Controller
    {
        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public HomeController()
        {
            
        }

        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        /// Llama la pantalla principal.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Index(string id, string ReturnUrl = null,string deporteS = null, string tipoTorneo=null,string action=null)
        {
            if (action!=null)
            {
                ViewBag.action = action;
            }
            if (deporteS != null && tipoTorneo !=null)
            {
                ReturnUrl = ReturnUrl + "&deporte=" + deporteS + "&tipoTorneo=" + tipoTorneo;
            }
            ViewBag.id = id;
            var user = User.Identity.IsAuthenticated;

            var model = getMainPageModel(new MainPageViewModel());
            if (id != null)
            {
                SelectListItem deporte = model.ddlDeportes.Where(d => d.Value == id).FirstOrDefault();
                if (deporte != null)
                {
                    model.ddlDeportes = new List<SelectListItem>();
                    model.ddlDeportes.Add(deporte);
                }
                else
                    ViewBag.id = null;
            }


            if(TempData["ReturnUrl"] != null)
                ViewBag.ReturnUrl = TempData["ReturnUrl"];
            else if (ReturnUrl != null)
                ViewBag.ReturnUrl = ReturnUrl;


            if (ReturnUrl != null)
            {
                if (user)
                {
                    var usuario = db.getUserById(User.Identity.GetUserId());
                    var rol = db.getRoleByName(constClass.rolCoach).Id.ToUpper();
                    if(usuario.usuRolActual.ToUpper() != rol)
                    {
                        //var rolesUser = db.getUserRoles(usuario).Where(l=> l.rolName == constClass.rolCoach).First().ToString();
                        //if (rolesUser=="")
                        //{
                        //    UserManager.AddToRole(usuario.Id, constClass.rolCoach);
                       // }
                       // db.setCurrentUserRole(usuario.Id, rol);
                    }
                    return Redirect(ReturnUrl);
                }
                ViewBag.showLogin = true;
                ModelState.AddModelError(constClass.info, "Debes iniciar sesión para poder ver la página que solicitaste.");
            }
            
            // 0311412F17AF2AFDD17BA4DE1F7B852D
            // var psw = Global_Functions.getEncriptPrivateKey("A6fg85_e5T3ew", constClass.encryptionKey);
            return View(model);
        }

        /// <summary>
        /// Carga los datos de la pantalla principal.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected MainPageViewModel getMainPageModel(MainPageViewModel model)
        {
            model.deportes = db.getDeportes_Active();
            model.ddlDeportes = model.deportes
                                     .Select(d => new SelectListItem() { 
                                            Text = d.depNombre, 
                                            Value = d.depNombre })
                                     .ToList();
            model.ddlDeportes.Insert(0, new SelectListItem() { Text = "Todos", Value = ""});

            model.ddlTipoTorneos = db.getTiposTorneo_Active()
                                     .Select(d => new SelectListItem() { 
                                            Text = d.ttoNombre, 
                                            Value = d.ttoId.ToString() })
                                     .ToList();
            model.ddlTipoTorneos.Insert(0, new SelectListItem() { Text = "Todos", Value = "0" });

            model.ddlCiudades = db.getLigaCiudades_DistinctActive()
                                  .Select(d => new SelectListItem()
                                  {
                                         Text = d.Municipio.ToUpper(),
                                         Value = d.Municipio
                                  }).ToList();
            model.ddlCiudades.Insert(0, new SelectListItem() { Text = "Todas", Value = "" });

            model.ddlZonas = db.getZonas_Active()
                               .Select(d => new SelectListItem()
                               {
                                       Text = d.zonZona,
                                       Value = d.zonId.ToString()
                                })
                                .ToList();
            model.ddlZonas.Insert(0, new SelectListItem() { Text = "Todas", Value = "0" });

            return model;
        }

        #endregion

        #region AddLeagues

        /// <summary>
        /// Obtiene la liga por metodo get para hacer el post y esconder el tipo de liga.
        /// </summary>
        /// <param name="league"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InscribirLigag(string tipoLiga)
        {
            ViewBag.tipoLiga = tipoLiga;
            //return View("InscribirLiga");
            return RedirectToAction("InscribirLiga", new { tipoLiga = tipoLiga });
        }

        /// <summary>
        /// Realiza la carga de la pantalla de solicitud
        /// </summary>
        /// <param name="tipoLiga"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InscribirLiga(string tipoLiga)
        {
            if (tipoLiga == null)
                return RedirectToAction("Index");

            LeagueRegisterViewModel model = new LeagueRegisterViewModel();
            model.lreUserProfile = getUserProfile(model.lreUserProfile, User.Identity.Name);
            model.lreAddingLeague = true;
            model.lreTaxData = getDatosFiscales(model.lreTaxData, User.Identity.GetUserId());

            //model = getAddLeagueFormPayments(model, "tipoLiga", tipoLiga, "", "");
            model.lreTipoLiga = tipoLiga;
            model.lreDdlTiposLiga = db.getLigaCategorias_Active();
            return View(model);
        }

        /// <summary>
        /// Obtiene los datos del perfil del usuario en sesión
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public UserProfileAddLeagueViewModel getUserProfile(UserProfileAddLeagueViewModel model, string username)
        {
            var usr = db.getUserByUserName(username);
            var prof = db.getUserMainProfile(usr.Id);
            if (prof == null)
                prof = new schemaUsersProfiles();
            model.usuUsername = usr.UserName;
            model.imgURL = prof.uprProfileImageURL;
            model.usuNombreCompleto = ((prof.uprNombres == "-") ? "" : prof.uprNombres.Trim()) + " " + ((prof.uprApellidos == "-") ? "" : prof.uprApellidos.Trim());
            model.usuGenero = (prof.uprGenero != null) ? prof.uprGenero : "";
            model.usuFechaNacimiento = (prof.uprFechaNacimiento != null) ? ((DateTime)prof.uprFechaNacimiento).ToString(constClass.formatDateOnly) : "";
            model.usuCiudad = (prof.uprCiudad != null) ? prof.uprCiudad.Trim() : "";
            model.usuTelefono = (prof.uprTelefono != null) ? prof.uprTelefono.Trim() : "";
            model.usuCorreo = (usr.Email != null) ? usr.Email.Trim() : "";

            return model;
        }

        /// <summary>
        /// Obtiene los ultimos datos fiscales que guardo el usuario al crear una liga
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public TaxDataViewModel getDatosFiscales(TaxDataViewModel model, string userId)
        {
            var datosFiscales = db.getLastLigaDatosFiscalesByUser(userId);
            if (datosFiscales != null)
            {
                model.tdaRFC = datosFiscales.ldfRFC;
                model.tdaRazonSocial = datosFiscales.ldfRazonSocial;
                model.tdaDomicilio = datosFiscales.ldfDomicilio;
                model.tdaNumeroExtInt = datosFiscales.ldfNumeroExtInt;
                model.tdaColonia = datosFiscales.ldfColonia;
                model.tdaMunicipio = datosFiscales.ldfMunicipio;
                model.tdaEstado = datosFiscales.ldfEstado;
                model.tdaCodigoPostal = datosFiscales.ldfCodigoPostal;
            }
            return model;
        }

        /// <summary>
        /// Obtiene los datos del tipo de liga, forma y periodicidad de pago, dependiendo de cual fue el campo que cambio.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="campoNombre"></param>
        /// <param name="lreTipoLiga"></param>
        /// <param name="lreFormaPago"></param>
        /// <param name="lrePeriodicidadPago"></param>
        /// <returns></returns>
        public LeagueRegisterViewModel getAddLeagueFormPayments(LeagueRegisterViewModel model, string campoNombre, string lreTipoLiga, string lreFormaPago, string lrePeriodicidadPago)
        {

            //model.lreDdlFormasPago = db.getTarifasFormasPagoByLigaCategoria(model.lreTipoLiga);
            model.lreTipoLiga = lreTipoLiga;
            model.lreDdlTiposLiga = db.getLigaCategorias_Active();
            //model.lreFormaPago = lreFormaPago;
            //model.lrePeriodicidadPago = lrePeriodicidadPago;
            /*
            switch (campoNombre)
            {
                case "tipoLiga":
                    model.lreFormaPago = model.lreDdlFormasPago.FirstOrDefault().tfpIdFormaPago;
                    model.lreDdlPeriodicidadesPago = db.getTarifasPeriodicidadesByFormaPago(model.lreFormaPago);
                    model.lrePeriodicidadPago = model.lreDdlPeriodicidadesPago.FirstOrDefault().tpeIdPeriodicidad;
                    break;
                case "formaPago":
                    model.lreDdlPeriodicidadesPago = db.getTarifasPeriodicidadesByFormaPago(model.lreFormaPago);
                    model.lrePeriodicidadPago = model.lreDdlPeriodicidadesPago.FirstOrDefault().tpeIdPeriodicidad;
                    break;
                case "periodicidadPago":
                    model.lreDdlPeriodicidadesPago = db.getTarifasPeriodicidadesByFormaPago(model.lreFormaPago);
                    break;
            }

            if (model.lrePeriodicidadPago == null || model.lrePeriodicidadPago == "" || model.lrePeriodicidadPago == String.Empty)
            {
                model.lrePeriodicidadPago = "UNICO";
            }

            model.lreListTarifas = db.getFeesByFormaPagoPeriodicidadPago(model.lreFormaPago, model.lrePeriodicidadPago);
            //model.tcfppId = model.lreListTarifas.Last().tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tcfppId;

    */
            return model;
        }

        /// <summary>
        /// Se ejecuta cuando cambia el tipo de liga, forma de pago o periodicidad de pago en la pantalla de registro de liga.
        /// </summary>
        /// <param name="campoNombre"></param>
        /// <param name="lreTipoLiga"></param>
        /// <param name="lreFormaPago"></param>
        /// <param name="lrePeriodicidadPago"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _InscribirLigaFormPayment(string campoNombre, string lreTipoLiga, string lreFormaPago, string lrePeriodicidadPago)
        {
            LeagueRegisterViewModel model = new LeagueRegisterViewModel();
            //model = getAddLeagueFormPayments(model, campoNombre, lreTipoLiga, lreFormaPago, lrePeriodicidadPago);
            model.lreTipoLiga = lreTipoLiga;
            model.lreDdlTiposLiga = db.getLigaCategorias_Active();
            return PartialView("Home/_InscribirLigaFormPayment", model);
        }

        /// <summary>
        /// Acción que se ejecuta cuando se guarda la solicitud de la liga.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLeague(LeagueRegisterViewModel model)
        {
            // model = getAddLeagueFormPayments(model, "periodicidadPago", model.lreTipoLiga, model.lreFormaPago, model.lrePeriodicidadPago);
                        
                if ((model.lreTaxData.tdaRFC == null || model.lreTaxData.tdaRFC.Trim() == "") &&
              (model.lreTaxData.tdaRazonSocial == null || model.lreTaxData.tdaRazonSocial.Trim() == "") &&
              (model.lreTaxData.tdaDomicilio == null || model.lreTaxData.tdaDomicilio.Trim() == "") &&
              (model.lreTaxData.tdaNumeroExtInt == null || model.lreTaxData.tdaNumeroExtInt.Trim() == "") &&
              (model.lreTaxData.tdaColonia == null || model.lreTaxData.tdaColonia.Trim() == "") &&
              (model.lreTaxData.tdaMunicipio == null || model.lreTaxData.tdaMunicipio.Trim() == "") &&
              (model.lreTaxData.tdaEstado == null || model.lreTaxData.tdaEstado.Trim() == "") &&
              (model.lreTaxData.tdaCodigoPostal == null || model.lreTaxData.tdaCodigoPostal.Trim() == ""))
                {
                    ModelState.Remove("lreTaxData.tdaRFC");
                    ModelState.Remove("lreTaxData.tdaRazonSocial");
                    ModelState.Remove("lreTaxData.tdaDomicilio");
                    ModelState.Remove("lreTaxData.tdaNumeroExtInt");
                    ModelState.Remove("lreTaxData.tdaColonia");
                    ModelState.Remove("lreTaxData.tdaMunicipio");
                    ModelState.Remove("lreTaxData.tdaEstado");
                    ModelState.Remove("lreTaxData.tdaCodigoPostal");

                    model.lreTaxData.tdaSaveTda = false;
                }

                ModelState.Remove("lreLeagueCoAdmins.lcaEmail.lcaEmail");
            if (db.getLigasByName(model.lreNombreLiga).Count <= 0)
            {
                if (ModelState.IsValid)
                {
                    //Guarda Imagenes
                    string imgProfile = model.lreUserProfile.imgURL;
                    string imgLiga = model.lreImgUrl;
                    model.lreUserProfile.imgURL = null;
                    model.lreImgUrl = null;

                    foreach (string file in Request.Files)
                    {
                        string filename = "";
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(fileContent.FileName);
                            if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                            {
                                string urlPath = Server.MapPath(constClass.urlLeaguesImages);
                                if (file == "usuFileImage")
                                    urlPath = Server.MapPath(constClass.urlProfileImages);

                                filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                                fileContent.SaveAs(filename);
                                bool savedFile = System.IO.File.Exists(filename);

                                string msgElement = "";
                                if (file == "usuFileImage")
                                {
                                    msgElement = "del Perfil";
                                    if (savedFile)
                                    {
                                        model.lreUserProfile.imgURL = filename;
                                        if (imgProfile != null && imgProfile != "" && imgProfile != filename)
                                            Global_Functions.deleteFiles(imgProfile, true);
                                    }

                                }
                                else
                                {
                                    msgElement = "de la liga";
                                    if (savedFile)
                                    {
                                        model.lreImgUrl = filename;
                                        if (imgLiga != null && imgLiga != "" && imgLiga != filename)
                                            Global_Functions.deleteFiles(imgLiga, true);
                                    }
                                }

                                if (!savedFile)
                                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen " + msgElement + ".");
                            }
                            else
                            {
                                ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                            }
                        }
                    }


                    model.lreLigaLatitud = (model.lreLigaLatitud != null && model.lreLigaLatitud != "") ? model.lreLigaLatitud : "0.0";
                    model.lreLigaLongitud = (model.lreLigaLongitud != null && model.lreLigaLongitud != "") ? model.lreLigaLongitud : "0.0";
                    if (db.setAddLeagueRequest(model))
                    {
                        string league = model.lreTipoLiga;
                        var ligaNombre = model.lreNombreLiga;
                        ModelState.Clear();

                        model = new LeagueRegisterViewModel();
                        model.lreLeagueSaved = true;
                        model.lreAddingLeague = true;
                        model.lreTipoLiga = league;
                        model.lreUserProfile = getUserProfile(model.lreUserProfile, User.Identity.Name);
                        //model = getAddLeagueFormPayments(model, "tipoLiga", league, "", "");
                        model.lreTipoLiga = league;
                        model.lreDdlTiposLiga = db.getLigaCategorias_Active();
                        model.lreTaxData = getDatosFiscales(model.lreTaxData, User.Identity.GetUserId());

                        enviarNotificacionAdministrators(ligaNombre, User.Identity.Name);

                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error guardando la liga, intenta nuevamente.");
                        if (model.lreUserProfile.imgURL == null)
                            model.lreUserProfile.imgURL = imgProfile;
                        if (model.lreImgUrl == null)
                            model.lreImgUrl = imgLiga;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Debes ingresar todos los campos obligatorios.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.info, "Lo Sentimos, pero el nombre "+model.lreNombreLiga+", ya se encuentra en uso. Favor de registrar otro Nombre de Liga.");
            }

            model.lreDdlTiposLiga = db.getLigaCategorias_Active();
            return PartialView("Home/_InscribirLigaForm", model);
        }

        public bool enviarNotificacionAdministrators(string ligName, string name)
        {
            try
            {
                // Send an email with this link
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    //string emailTo = nombreJugador+","+ jugador.tblEquipos.tblUsuarioCreador.Email;
                    var admins = db.getAdministrators();
                    string emailTo = "";
                    foreach (var item in admins)
                    {
                        emailTo += item.Email+",";
                    }
                    emailTo = emailTo.Remove(emailTo.Length - 1);
                    string body = Global_Functions.getBodyHTML("~/Emails/NotificacionSolicitudLiga.html");
                    body = body.Replace("<%= Liga %>", ligName);
                    body = body.Replace("<%= Nombre %>", name);

                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Notificion Solicitud de Liga", body,
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
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        ///// <summary>
        ///// Acción que se ejecuta cuando se agrega un co-administrador a la liga.
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult _AddLeagueCoAdmin([Bind(Include = "lreLeagueCoAdmins")] LeagueRegisterViewModel model)
        //{
        //    ModelState.Remove("lreFormaPago");
        //    ModelState.Remove("lreNombreLiga");
        //    ModelState.Remove("lreCorreoContacto");
        //    ModelState.Remove("lreTelefonoContacto");
        //    ModelState.Remove("lreDescripcion");

        //    if (ModelState.IsValid)
        //    {
        //        var emails = model.lreLeagueCoAdmins.lcaEmailList.Where(m => m.lcaEmail.ToUpper().Trim() == model.lreLeagueCoAdmins.lcaEmail.lcaEmail.ToUpper().Trim()).Count();
        //        if (emails == 0)
        //        {
        //            model.lreLeagueCoAdmins.lcaEmailList.Add(model.lreLeagueCoAdmins.lcaEmail);
        //            ModelState.AddModelError(constClass.success, "El correo fue agregado a la lista.");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(constClass.info, "El correo ya está en la lista.");
        //        }
        //        ModelState.Clear();
        //        model.lreLeagueCoAdmins.lcaEmail = new LeagueCoAdministratorsViewModel();
        //    }

        //    //LeagueRegisterViewModel modelComplete = new LeagueRegisterViewModel();
        //    //modelComplete.lreLeagueCoAdmins = model;

        //    return PartialView("Home/_InscribirLigaFormCoAdmin", model);
        //}

        ///// <summary>
        ///// Acción que se ejecuta cuando se elimina a un co-administrador de la liga.
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="lcaEmailDel"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult _AddLeagueCoAdminDelete([Bind(Include = "lreLeagueCoAdmins")] LeagueRegisterViewModel model, string lcaEmailDel)
        //{
        //    if (lcaEmailDel != null || lcaEmailDel != "")
        //    {
        //        ModelState.Clear();

        //        if (model.lreLeagueCoAdmins.lcaEmailList == null)
        //            model.lreLeagueCoAdmins.lcaEmailList = new List<LeagueCoAdministratorsViewModel>();

        //        var email = model.lreLeagueCoAdmins.lcaEmailList.Where(m => m.lcaEmail.ToUpper().Trim() == lcaEmailDel.ToUpper().Trim()).FirstOrDefault();
        //        if (email != null)
        //        {
        //            model.lreLeagueCoAdmins.lcaEmailList.Remove(email);
        //            ModelState.AddModelError(constClass.success, "El correo fue eliminado de la lista.");
        //        }
        //    }

        //    //LeagueRegisterViewModel modelComplete = new LeagueRegisterViewModel();
        //    //modelComplete.lreLeagueCoAdmins = model;

        //    return PartialView("Home/_InscribirLigaFormCoAdmin", model);
        //}

        #endregion

        #region AddTeam


        #endregion

        #region Contact

        /// <summary>
        /// Acción que se ejecuta al cargar la pagina de contacto.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Acción que se ejecuta cuando se envía el formulario de la pagina de contacto.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _ContactForm(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!sendContactEmail(model))
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error enviando el correo de contacto. Intentalo nuevamente");
                }
                else
                {
                    ModelState.Clear();
                    model = new ContactViewModel();
                    ModelState.AddModelError(constClass.success, "Gracias por ponerte en contacto con ENLIGATE, pronto nos comunicaremos contigo.");
                }
            }

            return PartialView("Home/_ContactForm", model);
        }

        /// <summary>
        /// Metodo para enviar el correo de contacto.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected bool sendContactEmail(ContactViewModel model)
        {
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/contactPage.html");
                body = body.Replace("<%= conNombre %>", model.conNombre);
                body = body.Replace("<%= conEmail %>", model.conEmail);
                body = body.Replace("<%= conDescripcion %>", model.conDescripcion);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);


                bool mailSended = Global_Functions.sendMail(siteConfig.scoContactEmails, siteConfig.scoSenderDisplayEmailName, "Forma de contacto", body,
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

        /// <summary>
        /// Acción que se ejecuta al cargar la pantalla de nosotros.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult AboutUs()
        {

            return View();
        }

        /// <summary>
        /// Accion que se ejeucta al cargar la pantalla de preguntas frecuentes.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult FAQs()
        {

            return View();
        }

        #region DetalleLiga

        /// <summary>
        /// Muestra el detalle de la liga y muestra sus torneos para su inscripción.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Liga(int id, string deporte,int tipoTorneo,string error="")
        {
            LeaguesActiveDetailViewModel model = null;
            var liga = db.getLigaById(id);
            if (liga != null)
            {
                model = db.getUserLeaguesDetailById(liga.ligId, liga.ligUserIdCreador);
            }
            if (model != null)
                model.enableEdit = false;
            else
                return View("MainLeagueError");
            ViewBag.Deporte = deporte;
            ViewBag.tipoTorneoFiltro = tipoTorneo;
            if (error != "")
            {
                ViewBag.errorHomeLiga = error;
            }
            return View(model);
        }

        #endregion

        #region LeagueDetail

        /// <summary>
        /// Accion que regresa la vista para mostrar el mapa de la liga con su dirección.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult _LeaguesMap_Modal(int Id)
        {
            LeaguesActiveDetailViewModel model = new LeaguesActiveDetailViewModel();

            var liga = db.getLigaById(Id);
            if (liga != null)
            {
                model = db.getUserLeaguesDetailById(liga.ligId, liga.ligUserIdCreador);
            }
            ViewBag.gMapId = Id;

            return PartialView("Home/_LeaguesMap_Modal", model);
        }

        #endregion
        [AllowAnonymous]
        
        public ActionResult showTorneo(int Id)
        {
            ModelState.Clear();
            var ddl = new List<SelectListItem>();
            var model = new TorneosViewModel();
            var torneo = new schemaTorneos();
                torneo = db.getTorneoById(Id);
            if (torneo != null)
            {
                model.torId = torneo.torId;
                model.torTipo = torneo.torTipo;
                model.torImgUrl = torneo.torImgUrl;
                model.torComentarios = torneo.torComentarios;
                model.torNombreTorneo = torneo.torNombreTorneo;
                model.ligId = torneo.ligId;
                model.tblLiga = torneo.tblLiga;
                model.lctId = torneo.lctId;
                model.tblCategoriaTorneo = torneo.tblCategoriaTorneo;
                model.torFechaInicio = torneo.torFechaInicio;
                model.torFechaTermino = torneo.torFechaTermino;
                model.torFechaLimiteInscripcion = torneo.torFechaLimiteInscripcion;
                model.torNumeroJuegos = torneo.torNumeroJuegos;
                model.torNumeroEquipos = torneo.torNumeroEquipos;
                model.torMaxJugadoresEquipo = torneo.torMaxJugadoresEquipo;
                model.torPuntosGanar = torneo.torPuntosGanar;
                model.torPuntosEmpatar = torneo.torPuntosEmpatar;
                model.torPuntosPerder = torneo.torPuntosPerder;
                model.tesId = torneo.tesId;
                model.torEstructuraDescripcion = torneo.tblTorneoEstructura.tcsDescripcion;
                model.tblTorneoDireccion = torneo.tblTorneoDireccion;
                model.torNumeroContacto = torneo.torNumeroContacto;
                model.torCorreoContacto = torneo.torCorreoContacto;
                model.torPrecioTorneo = torneo.torPrecioTorneo;
                model.torDiasParaPago = torneo.torDiasParaPago;
                model.tcfpptpId = (torneo.tcfpptpId != null) ? (int)torneo.tcfpptpId : 0;
                model.tblTarifasCfppTiposPago = torneo.tblTarifasCfppTiposPago;
                model.torKey = torneo.torUserIdCreador;
                model.torEstatus = torneo.torEstatus;
                model.torDeporteEnEquipo = torneo.torDeporteEnEquipo;
                model.torAprobada = torneo.torAprobada;
                model.torEsCoaching = torneo.torEsCoaching;

                var liga = db.getLigaById(model.ligId);
                if (liga != null)
                {
                    ddl.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                    model.ddlLigas = ddl;
                }
                model.ddlCategorias = db.getLigaCategoriasTorneosActivosByLigaId(model.ligId)
                     .Select(l => new SelectListItem { Text = l.depNombre.ToUpper() + " - " + l.lctNombre.ToUpper(), Value = l.lctId.ToString() })
                     .OrderBy(l => l.Text)
                     .ToList();
                if (model.ddlCategorias.Count() > 0)
                {
                    var deporte = db.getDeportaByLigaCategoriasTorneosId(model.lctId);
                    model.torDeporteEnEquipo = deporte.depEnEquipo;
                    model.listTorneoEstructuras = db.getTorneoEstructurasByTipoDeporte(model.torDeporteEnEquipo)
                     .OrderBy(l => l.tscNombre)
                     .ToList();
                }
            }
            else
            {
                return Content("<h3>No se encontro el torneo. Intenta nuevamente.</h3>");
            }
           // model = new AdminTorneosController()._TorneoNuevo_Fill _TorneoNuevo_FillPostModel(model, false, false, true,true);
            return PartialView("Home/_TorneoDetails",model);                       
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult InscribirEquag(int torId)
        {
            ViewBag.torId = torId;
            var user = db.getUserById(User.Identity.GetUserId());
            var torneo = db.getTorneoById(torId);
            if (torneo.torEsCoaching)
            {
                var jugEquipo = db.getJugadoresByTorneo_UserEmail(torId, user.Email);
                if (jugEquipo.Any())
                {
                    if (jugEquipo.Count >= 1)
                    {
                        return RedirectToAction("liga", new { id = torneo.ligId, deporte="", tipoTorneo = 0, error = "Lo sentimos, solo se permite inscribirse una ocacion por Torneo." });
                    }
                }
            }
            ViewBag.afa_LigaCategorias = db.getLigaCategorias_Active();
            var Id = 0;
            if (torId > 0)
                Id = torId;
            return RedirectToAction("InscripcionTorneo", new { torId = torId });

        }
        [HttpGet]
        public ActionResult InscripcionTorneo(int? torId = null)
        {
            if (torId == null || torId <= 0)
                return RedirectToAction("Index");
            ViewBag.UserName = db.getUserById(User.Identity.GetUserId()).Email;

            var equipoController = new AdminEquiposController();
            var model = equipoController._EquipoNuevo_OnLoadViewModel((int)torId, 0);
            model.equAdminLigaTorneos = true;
            model.usuAgregarCoadmin = false;
            ViewBag.valCoAdmin = false;
            model.equCreadorEquipoId = User.Identity.GetUserId();
            model.equCorreoAdministrador = db.getUserByUserName(User.Identity.Name).Email;
            var esCoach = model.tblTorneo.torEsCoaching;
            if (esCoach)
            {
                var equipoDefault = db.getEquipoByTorneo(model.torId).OrderByDescending(o => o.equFechaCreacionUTC).FirstOrDefault();
                if (equipoDefault != null)
                {
                    model.equId = equipoDefault.equId;
                    model.equNombre = equipoDefault.equNombreEquipo;
                    model.equCreadorEquipoId = equipoDefault.equUserIdCreador;
                    model.equCorreoAdministrador = db.getUserById(equipoDefault.equUserIdCreador).Email;
                    ViewBag.ValEquipoExist = true;                    
                }
                else
                {
                    ViewBag.ValEquipoExist = false;
                }                

                ViewBag.AdminTorneo = db.getTorneoById(model.torId).tblUserCreador.UserName;
            }


            return View(model);
        }
        public ActionResult TorneoView(int torId)
        {
            ViewBag.torId = torId;
            ViewBag.afa_LigaCategorias = db.getLigaCategorias_Active();
            return View("TorneoDetails");
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult TorneoDetails(int id)
        {
            ViewBag.torId = id;
            return View();
        }
        //Estadisticas del Torneo
        [AllowAnonymous]
        public ActionResult torEstadisticas(int torId)
        {
            var pos = 1;
            var result = db.getEstadisticasTorneo(torId).OrderByDescending(o=> o.puntos).ToList();
            result.ForEach(l => l.pos = pos++);
            return PartialView("Torneos/_TorneoEstadisticas",result);
        }
        [AllowAnonymous]
        public ActionResult DetallesEstadisticas(int torId, int equId)
        {
            var model = db.getEstadisticasTorneo(torId, equId).FirstOrDefault();
            ViewBag.TeamNameEst = db.getEquipoById(equId).equNombreEquipo;
            return PartialView("Torneos/_EquipoEstadisticas",model);
        }
        [AllowAnonymous]
        public ActionResult EquipoEstadisticas(int torId,int equId)
        {
            var model = new List<TorneoEstGoleador>();
            var equ1 = db.getEstadisticasLiderGoleo(torId).Where(l => l.equId == equId).OrderByDescending(l => l.goles).ToList();
            equ1 = equ1.GroupBy(l => l.equId)
                        .Select(cl => new TorneoEstGoleador
                        {
                            equId = cl.First().equId,
                            equNombre = cl.First().equNombre,
                            jugNombre = cl.First().jugNombre,
                            goles = cl.Sum(c => c.goles),
                            faltas = cl.Sum(c => c.faltas),
                            amarillas = cl.Sum(c => c.amarillas),
                            rojas = cl.Sum(c => c.rojas),
                            parId = cl.First().parId
                        }).ToList();

            var equipo1=db.getEquipoById(equId).equNombreEquipo;
            equ1.ForEach(f=> f.equipo = equipo1);

            model.AddRange(equ1);
            return PartialView("Torneos/_TorneoJugadorEst",model);
        }

        [AllowAnonymous]
        public ActionResult torEstGoleador(int torId)
        {
            var pos = 1;
            var result = db.getEstadisticasLiderGoleo(torId).OrderByDescending(l => l.goles).ToList();
            result = result.GroupBy(l => l.equId)
                            .Select(cl => new TorneoEstGoleador
                            {
                                equId = cl.First().equId,
                                equNombre = cl.First().equNombre,
                                jugNombre = cl.First().jugNombre,
                                goles = cl.Sum(c => c.goles),
                                faltas = cl.Sum(c => c.faltas),
                                amarillas = cl.Sum(c => c.amarillas),
                                rojas = cl.Sum(c => c.rojas),
                                parId = cl.First().parId
                            }).ToList();
            result.ForEach(l => l.pos = pos++);
            var model = result;
            return PartialView("Torneos/_TorneoEstGoleador",model);
        }
        [AllowAnonymous]
        public ActionResult torEstPartido(int parId)
        {
            var partido = db.getPartidoById(parId);
            var equ1Id = partido.equIdUno;
            var equ2Id = partido.equIdDos;
            var model = new List<TorneoEstGoleador>();

            var equ1 = db.getEstadisticasLiderGoleo(partido.torId).Where(l => l.equId == equ1Id && l.parId == parId).OrderByDescending(l => l.goles).ToList();
            equ1.ForEach(f => f.equipo = partido.equNombreEquipoUno);

            var equ2 = db.getEstadisticasLiderGoleo(partido.torId).Where(l => l.equId == equ2Id && l.parId == parId).OrderByDescending(l => l.goles).ToList();
            equ2.ForEach(f => f.equipo = partido.equNombreEquipoDos);

            model.AddRange(equ1);
            model.AddRange(equ2);

            return PartialView("Torneos/_TorneoJugadorEst", model);
        }
        [AllowAnonymous]
        public JsonResult _DetallePartido(int parId)
        {
            var partido = db.getPartidoById(parId);
            
            var json = new {
                partido.equNombreEquipoUno,
                partido.equNombreEquipoDos,
                partido.equResultadoUno,
                partido.equResultadoDos
            };
            return Json(json);
        }
    }
}