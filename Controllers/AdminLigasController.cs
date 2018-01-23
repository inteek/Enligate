using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models;
using sw_EnligateWeb.Models.HelperClasses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.IO;
using conekta;
using Newtonsoft.Json;
using NUnit;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;   
using Microsoft.AspNet.Identity.EntityFramework;

using Microsoft.Owin.Security;
using System.Diagnostics;

namespace sw_EnligateWeb.Controllers
{
    public class AdminLigasController : MyBaseController
    {


        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminLigasController()
        {

        }

        public AdminLigasController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        /// Regresa la vista de la pantalla de dashboard del dueño de ligas
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            var total = db.getLeagueGridStatus().Where(l => l.ligCreadorId.ToUpper() == user.Id.ToUpper()).Count();
            
            ViewBag.numNotifications = total;
            return View();
            //return RedirectToAction("MainLeague");
        }

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

        #region Ligas Busqueda Principal

        #region Grid Busquedas

        /// <summary>
        /// Muestra 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult LigasBusquedasGrid()
        {
            return PartialView("Ligas/_LigasBusquedasGrid");
        }

        /// <summary>
        /// Accion que se ejecuta cuando se realiza el callback del grid de la portada o se realiza una busqueda
        /// </summary>
        /// <param name="deporte"></param>
        /// <param name="tipoTorneo"></param>
        /// <param name="ciudad"></param>
        /// <param name="filtro">
        /// 0 - Popularidad - Mayor a Menor
        /// 1 - Calificación - Mayor a Menor
        /// 2 - Precio - Menor a Mayor
        /// 3 - Distancia - Menor a Mayor
        /// </param>
        /// <param name="reverse">Cambia el orden en como regresan las cosas.</param>
        /// <returns></returns>
        [ValidateInput(false)]
        [AllowAnonymous]
        public ActionResult _LigasBusquedasGrid_Callback(string deporte, int tipoTorneo, string ciudad, int filtro, bool reverse, double latitud, double longitud,bool showMessage=true)
        {
            var model = db.getLigas_BusquedaInicio(deporte, tipoTorneo, ciudad);
            
            switch(filtro)
            {
                //Popularidad
                case 0:
                    if(!reverse)
                        model = model.OrderByDescending(t => t.ligTotalJugadores).ToList();
                    else
                        model = model.OrderBy(t => t.ligTotalJugadores).ToList();
                    break;
                //Calificación
                case 1:
                    if (!reverse)
                        model = model.OrderByDescending(t => t.ligCalificacion).ToList();
                    else
                        model = model.OrderBy(t => t.ligCalificacion).ToList();
                    break;
                //Precio
                case 2:
                    if (!reverse)
                        model = model.OrderBy(t => t.ligPrecioDesde).ToList();
                    else
                        model = model.OrderByDescending(t => t.ligPrecioDesde).ToList();
                    break;
                //Distancia
                case 3:
                    //Realiza la formula para calcular la distancia entre los puntos.
                    model.ForEach(l => l.ligLatLngDistancia = 
                     (3956 * 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((latitud - Math.Abs(l.ligLatitud)) * Math.PI/180 / 2),2) + 
                                Math.Cos(latitud * Math.PI/180 ) * Math.Cos(Math.Abs(l.ligLatitud) * 
                                Math.PI/180) * Math.Pow(Math.Sin(((longitud) - (l.ligLongitud)) *  Math.PI/180 / 2), 2)))));
                    if (!reverse)
                        model = model.OrderBy(t => t.ligLatLngDistancia).ToList();
                    else
                        model = model.OrderByDescending(t => t.ligLatLngDistancia).ToList();
                    break;
            }

            //Regresa el mensaje que retroalimenta al usuario.
            if (showMessage==true)
            {
                if (model.Count > 0)
                {
                    ModelState.AddModelError(constClass.success, "Se encontraron " + model.Count.ToString() + " resultados.");
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "La búsqueda no obtuvo ningun resultado.");
                }
            }
            ViewBag.deporteFiltro = deporte;
            ViewBag.tipoTorneoFiltro = tipoTorneo;
            ViewData["lbgModelError"] = RenderPartialViewToString("_ModalState_Errors");
            return PartialView("Ligas/_LigasBusquedasGrid",model);
        }

        #endregion

        #endregion

        #region Leagues

        /// <summary>
        /// Regresa la vista de la liga principal
        /// </summary>
        /// <returns></returns>
        public ActionResult MainLeague()
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getMainLeague(user.Id, user.usuRolActual);
            if (model != null)
                model.enableEdit = true;
            else
                return View("MainLeagueError");
            return View(model);
        }

        /// <summary>
        /// Muestra la pantalla para crear una nueva solicitud de liga.
        /// </summary>
        /// <returns></returns>
        public ActionResult AddLeague()
        {
            var ligaTipos = db.getLigaCategorias_Active();
            var homeController = new HomeController();

            LeagueRegisterViewModel model = new LeagueRegisterViewModel();
            model.lreUserProfile = homeController.getUserProfile(model.lreUserProfile, User.Identity.Name);
            model.lreAddingLeague = true;
            model.lreTaxData = homeController.getDatosFiscales(model.lreTaxData, User.Identity.GetUserId());
            //model = homeController.getAddLeagueFormPayments(model, "tipoLiga", ligaTipos.First().lcaId, "", "");
            model.lreTipoLiga = ligaTipos.First().lcaId;
            model.lreDdlTiposLiga = db.getLigaCategorias_Active();
            return View(model);
        }

        /// <summary>
        /// Regresa la vista para la pantalla de ligas activas
        /// </summary>
        /// <returns></returns>
        public ActionResult Leagues()
        {
            return View();
        }

        /// <summary>
        /// Regresa la vista del grid de ligas activas
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaguesGrid()
        {
            return PartialView("Ligas/_LeaguesGrid");
        }

        /// <summary>
        /// Realiza la busqueda para llenar el grid
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _LeaguesGrid()
        {
            List<LeaguesActiveLOwnerViewModel> model = new List<LeaguesActiveLOwnerViewModel>();
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            model = db.getUserLeagues(user.Id, user.usuRolActual);
            
            return PartialView("Ligas/_LeaguesGrid", model);
        }
        [ValidateInput(false)]
        public ActionResult _LeaguesGridView()
        {
            List<LeaguesActiveLOwnerViewModel> model = new List<LeaguesActiveLOwnerViewModel>();
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            model = db.getRefereeLeagues(user.Id);

            return PartialView("Ligas/_LeaguesGridView", model);
        }
        
        /// <summary>
        /// Establece la liga como la principal del usuario.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _MainLeagueSet(int id, string token)
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var liga = db.getLigaById(id);

            if (liga != null && liga.ligUserIdCreador == token)
            {
                if (db.setMainLeague(id, user.Id, user.usuRolActual))
                {
                    ModelState.AddModelError(constClass.success, "Se ha guardado la liga principal.");
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la liga principal. Intenta nuevamente.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error guardando la liga principal. Intenta nuevamente.");
            }

            return PartialView("_ModalState_Errors");
        }

        /// <summary>
        /// Regresa la vista de la liga para mostrar el detalle.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="key">Es el userId del creador de la liga para evitar que solo sea por el id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail(int Id, string key)
        {
            var model = db.getUserLeaguesDetailById(Id, key);
            model.enableEdit = true;
            return PartialView("Ligas/_LeaguesDetail", model);
        }
        [HttpPost]
        public ActionResult _RefereeLeaguesDetail(int Id, string key)
        {
            var model = db.getUserLeaguesDetailById(Id, key);
            model.enableEdit = false;
            return PartialView("Ligas/_LeaguesDetail", model);
        }

        /// <summary>
        /// Regresa la vista de la liga para mostrar la edicion de los datos principales.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_MainDataEdit(int Id, string key)
        {
            LeagueDetail_MainData model = db.getLeagueMainDataById(Id, key);
            return PartialView("Ligas/_LeaguesDetail_MainDataEdit", model);
        }

        /// <summary>
        /// Regresa la vista de los datos principales de la ligas.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_MainDataEdit_Cancel(int Id, string key)
        {
            var model = db.getUserLeaguesDetailById(Id, key);
            model.enableEdit = true;
            return PartialView("Ligas/_LeaguesDetail_MainData", model);
        }

        /// <summary>
        /// Guarda los datos principales de la liga.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _LeaguesDetail_MainDataEdit_Save(LeagueDetail_MainData model)
        {            
            var resultJson = new JsonResultViewModel();
            resultJson.booHasPartialView = true;
            var userId = User.Identity.GetUserId();
            var edit = false;
            var ligasDB = db.getLigasByName(model.lreNombreLiga);
            if (ligasDB.Count > 0)
            {
                var ligCreator = ligasDB.FirstOrDefault().ligUserIdCreador.ToUpper() == userId.ToUpper();
                if (ligCreator)
                {
                    edit = true;
                }
                else
                {
                    var ligCoAdm = db.getCoAdminLigasByLigIg(model.ligId).Where(l => l.lcaUserId.ToUpper() == userId.ToUpper());
                    if (ligCoAdm.Any())
                    {
                        edit = true;
                    }
                }
            }
            else {
                edit = true;
            }
            if (edit)
            {
                if (ModelState.IsValid)
                {
                    //Guarda Imagen
                    string lreImgUrl = model.lreImgUrl;
                    model.lreImgUrl = null;
                    foreach (string file in Request.Files)
                    {
                        string filename = "";
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            string urlPath = Server.MapPath(constClass.urlLeaguesImages);
                            var extension = Path.GetExtension(fileContent.FileName);
                            if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                            {
                                filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                                fileContent.SaveAs(filename);
                                bool savedFile = System.IO.File.Exists(filename);
                                if (savedFile)
                                {
                                    model.lreImgUrl = filename;
                                    if (lreImgUrl != null && lreImgUrl != "" && lreImgUrl != filename)
                                        Global_Functions.deleteFiles(lreImgUrl, true);
                                }

                                if (!savedFile)
                                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen de la liga.");
                            }
                            else
                            {
                                ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                            }
                        }
                    }

                    if (db.setLeagueMainDataById(model))
                    {
                        ModelState.AddModelError(constClass.success, "Los datos de la liga se han guardado.");
                        resultJson.booSuccess = true;
                        resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error guardando los datos. Intenta nuevamente.");
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Favor de llenar los campos obligatorios.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Lo Sentimos, el nombre "+ model.lreNombreLiga +", ya esta en uso, Favor de cambiar el nombre de liga .");
            }


            if (!resultJson.booSuccess)
            {
                var Id = model.ligId;
                var key = db.getLigaById(model.ligId).ligUserIdCreador;
                LeagueDetail_MainData model2 = db.getLeagueMainDataById(Id, key);
                resultJson.strPartialViewString = RenderPartialViewToString("Ligas/_LeaguesDetail_MainDataEdit", model2);
            }
                
            
            return Json(resultJson);
        }

        /// <summary>
        /// Regresa la vista de los coadministradores de la liga
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaguesDetail_CoadminsGrid()
        {
            return PartialView("Ligas/_LeaguesDetail_CoadminsGrid");
        }

        /// <summary>
        /// Regresa la vista con el grid con los datos de los coadministradores de la liga
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _LeaguesDetail_CoadminsGrid(int ligId, string key)
        {
            var model = db.getLeagueCoadministratorsById(ligId, key);
            return PartialView("Ligas/_LeaguesDetail_CoadminsGrid", model);
        }

        /// <summary>
        /// Regresa la vista de los coadministradores de la liga para editarlos
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaguesDetail_CoadminsGridEdit()
        {
            return PartialView("Ligas/_LeaguesDetail_CoadminsGridEdit");
        }

        /// <summary>
        /// Llena el grid de los coadministradores para su edición.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _LeaguesDetail_CoadminsGridEdit(int ligId, string key)
        {
            var model = db.getLeagueCoadministratorsById(ligId, key);
            return PartialView("Ligas/_LeaguesDetail_CoadminsGridEdit", model);
        }
        
        /// <summary>
        /// Accion que se ejecuta al guardar un nuevo registro en el grid.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <param name="coadmin"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _LeaguesDetail_CoadminsGridEdit_AddNewPartial(int ligId, string key, LeagueCoAdministratorsViewModel coadmin)
        {
            var model = db.getLeagueCoadministratorsById(ligId, key);
            if(coadmin.lcaEmail.Trim() != ""){
                string emailAddress = coadmin.lcaEmail.Replace("\"", "").Trim();
                string errMensaje = "";
                //Valida que el usuario exista, sino lo crea
                /*
               
                */
                bool sendEmail = true;
                schemaLigaCoAdminInit coAdministrador = new schemaLigaCoAdminInit();
                //schemaLigaCoAdministradores coAdministrador = new schemaLigaCoAdministradores();
                if (errMensaje == "")
                {
                    //Revisa que el usuario no se repita en la tabla.
                    coAdministrador.ligId = ligId;
                    coAdministrador.userEmail = emailAddress;
                    coAdministrador.lcaConfirmacion = false;
                    coAdministrador.lcaCodigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());

                    var admin = model.Where(m=> m.lcaEmail == emailAddress).FirstOrDefault();
                    if (admin != null)
                    {
                        if (admin.lcaConfirmado == true)
                            sendEmail = false;
                    }
                    else
                    {
                        if (!db.setLeagueCoadmin(coAdministrador))
                        {
                            errMensaje = "Hubo un error agregando al coadministrador. Intentalo nuevamente.";   
                        }
                    }
                }

                if (errMensaje == "" && sendEmail)
                {
                    //Envia mensaje si no hay ningun error
                    if (!sendCoAdministratorEmail(coAdministrador))
                    {
                        errMensaje = "Hubo un error enviando el correo de confirmación. Intenta agregar nuevamente el coadministrador.";
                    }
                }

                if(errMensaje == "")
                {
                    ModelState.Clear();
                    ViewData["EditResult"] = "ok";
                }
                else
                {
                    ViewData["EditError"] = errMensaje;
                    ViewData["LeagueCoAdministratorsViewModel"] = coadmin;
                }               
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
                ViewData["LeagueCoAdministratorsViewModel"] = coadmin;
            }

            return PartialView("Ligas/_LeaguesDetail_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que elimina a un coadministrador desde el boton de eliminar del grid.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <param name="lcaUserId"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _LeaguesDetail_CoadminsGridEdit_Delete(int ligId, string key, string lcaUserId)
        {
            if (lcaUserId != null && lcaUserId.Trim() != "")
            {
                if(!db.setDeleteCoadmin(ligId, key, lcaUserId))
                    ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
                else
                    ViewData["EditResult"] = "ok";
            }
            else
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";

            var model = db.getLeagueCoadministratorsById(ligId, key);
            return PartialView("Ligas/_LeaguesDetail_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Envia el correo para recuperar la contraseña
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool sendPasswordRecoveryEmail(ApplicationUser user)
        {
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
            var callbackUrl = Url.Action("ResetPasswordCode", "Account", new { email = user.Email, code = code }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            if (db.setUpdatePasswordRecoveryCode(user, code))
            {
                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    string body = Global_Functions.getBodyHTML("~/Emails/PasswordRecovery.html");
                    body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(user.Email, siteConfig.scoSenderDisplayEmailName, "Recuperación de contraseña", body,
                                                                siteConfig.scoSenderEmail,
                                                                Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                                siteConfig.scoSenderSMTPServer,
                                                                siteConfig.scoSenderPort,
                                                                null, "", "", true, "");
                    if (mailSended)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Envia el correo para confirmar la cuenta de correo.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool sendConfirmEmailAccountCodeEmail(ApplicationUser user)
        {
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { email = user.Email, code = code }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            if (db.setUpdateEmailValidation(user, code))
            {
                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    string body = Global_Functions.getBodyHTML("~/Emails/confirmEmailAccount.html");
                    body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(user.Email, siteConfig.scoSenderDisplayEmailName, "Confirmar cuenta de correo", body,
                                                                siteConfig.scoSenderEmail,
                                                                Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                                siteConfig.scoSenderSMTPServer,
                                                                siteConfig.scoSenderPort,
                                                                null, "", "", true, "");
                    if (mailSended)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Envia el correo de confirmación para ser un coadministrador de una liga
        /// </summary>
        /// <param name="coAdmin"></param>
        /// <returns></returns>
        public bool sendCoAdministratorEmail(schemaLigaCoAdminInit coAdmin)
        {
            // Send an email with this link
            var code = coAdmin.lcaCodigoConfirmacion;
            var callbackUrl = Url.Action("confirmCoadministration", "AdminLigas", new { email = coAdmin.userEmail, code = code }, protocol: Request.Url.Scheme);
            var rejectUrl = Url.Action("rejectCoadministration", "AdminLigas", new { email = coAdmin.userEmail, code = code }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

           
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var liga = db.getLigaById(coAdmin.ligId);
                var prof = db.getUserMainProfile(User.Identity.GetUserId());
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    
                string body = Global_Functions.getBodyHTML("~/Emails/ConfirmCoAdmin.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre.Trim());
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                body = body.Replace("<%= UrlRejectCode %>", rejectUrl);                    
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(coAdmin.userEmail, siteConfig.scoSenderDisplayEmailName, "Coadministrador de Liga", body,
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
        /// Accion que se ejecuta cuando el usuario acepta la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult confirmCoadministration(string email, string code)
        {

            bool cerrarSesion = false;
            var usuPassword = "";
            var usr = db.getUserByUserEmail(email);
            if (usr == null)
                cerrarSesion = true;
            else
                if (usr.Id != User.Identity.GetUserId())
                cerrarSesion = true;

            if (cerrarSesion)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }

            var liga = db.getLigaByCoAdminIntCodeEmail(email, code);
            ViewBag.LigaNombre = liga.ligNombreLiga;
            //var user = db.getUserByUserEmail(email);

            /**/




            var user = db.getUserByUserEmail(email);
            var emailAddress = email;
            var errMensaje = "";
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = emailAddress,
                    Email = emailAddress,
                    PhoneNumber = "",
                    EmailConfirmed = true,
                    usuEstatus = true
                };
                string passwordCode = db.generator_Pass();
                var result = UserManager.Create(user, passwordCode);
                if (result != IdentityResult.Success)
                {
                    errMensaje = "Hubo un error creando al usuario. Intentalo nuevamente.";
                }
                else
                {
                    var prof = new schemaUsersProfiles();
                    prof.uprNombres = emailAddress;
                    prof.uprApellidos = "-";
                    prof.uprTelefono = "";
                    db.setUserProfileMain_UpdateInsert(user, prof);

                    UserManager.AddToRole(user.Id, constClass.rolPlayer);
                }

                user = db.getUserByUserEmail(emailAddress);
                enviarEmailCountCoadministarcionLiga(liga, user, passwordCode);
                if (user != null)
                {
                    //sendPasswordRecoveryEmail(user);
                    //sendConfirmEmailAccountCodeEmail(user);
                }
                else
                {
                    errMensaje = "Hubo un error creando al usuario. Intentalo nuevamente.";
                }
            }

            if (db.setConfirmCoadminInt(user, code,liga.ligId))
            {
                /**/
                if (db.setConfirmCoadmin(UserManager, user, liga.ligId))
                {
                    enviarEmailAceptacionCoadministarcionLiga(liga, user);
                    var prof = db.getUserMainProfile(user.Id);
                    if (prof == null)
                        prof = new schemaUsersProfiles();
                    string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    if (usuarioNombre == "")
                        usuarioNombre = user.Email;
                    ViewBag.UsuarioNombre = usuarioNombre;
                    ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                }
                else
                {
                    errMensaje = "error al confirmar coadmin";
                }

            }
            else
            {
                errMensaje = "error al confirmar coadmin Int";
            }
            if (errMensaje != "")
            {
                ModelState.AddModelError(constClass.error, "Hubo un error aceptando la solicitud. Redireccionando . . .");
                string rand = Global_Functions.getRandomString(10);
                ViewBag.jsScript = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                    @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";
            }
            else
            {
                SignInManager.SignIn(user, false, false);
                ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                string rand = Global_Functions.getRandomString(10);
                string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                                @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

                ViewBag.jsScript = redirectHome;
            }
           

            return View("CoadministrationConfirm");
        }

        /// <summary>
        /// Accion que se ejecuta cuando el usuario rechaza la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult rejectCoadministration(string email, string code)
        {
            var liga = db.getLigaByCoAdminCodeEmail(email, code);
            ViewBag.LigaNombre = liga.ligNombreLiga;
            if (db.setDeleteCoadmin(email, code))
            {
                var user = db.getUserByUserEmail(email);
                enviarEmailRechazoCoadministarcionLiga(liga, user);
                var prof = db.getUserMainProfile(user.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = user.Email;
                ViewBag.UsuarioNombre = usuarioNombre;
                ModelState.AddModelError(constClass.success, "La solicitud ha sido rechazada.");
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error rechazando la solicitud. Redireccionando . . .");
                string rand = Global_Functions.getRandomString(10);
                ViewBag.jsScript = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                    @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";
            }

            return View("CoadministrationReject");
        }

        public bool enviarEmailCountCoadministarcionLiga(schemaLigas liga, ApplicationUser userConfirmado, string pass)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var prof = db.getUserMainProfile(userConfirmado.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = userConfirmado.Email;

                string body = Global_Functions.getBodyHTML("~/Emails/CoadminLigaAceptacionCount.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);

                body = body.Replace("<%= Usuario %>", userConfirmado.Email);
                body = body.Replace("<%= Password %>", pass);


                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(userConfirmado.Email, siteConfig.scoSenderDisplayEmailName, "Aceptación Coadministración de Liga", body,
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
        /// Envia los correos de aceptacion de coadministracion de liga al dueño de la liga.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailAceptacionCoadministarcionLiga(schemaLigas liga, ApplicationUser userConfirmado)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var prof = db.getUserMainProfile(userConfirmado.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if(usuarioNombre == "")
                    usuarioNombre = userConfirmado.Email;
                string body = Global_Functions.getBodyHTML("~/Emails/CoadminLigaAceptacion.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(liga.tblUserCreador.Email, siteConfig.scoSenderDisplayEmailName, "Aceptación Coadministración de Liga", body,
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
        /// Envia los correos de rechazo de coadministracion de liga al dueño de la liga.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailRechazoCoadministarcionLiga(schemaLigas liga, ApplicationUser userConfirmado)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var prof = db.getUserMainProfile(userConfirmado.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = userConfirmado.Email;
                string body = Global_Functions.getBodyHTML("~/Emails/CoadminLigaRechazo.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(liga.tblUserCreador.Email, siteConfig.scoSenderDisplayEmailName, "Rechazo Coadministración de Liga", body,
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
        /// Accion que regresa la vista para mostrar el mapa de la liga con su dirección y la opcion de editar.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_Map_Modal(int Id, string code)
        {
            var model = db.getUserLeaguesDetailById(Id, code);
            model.enableEdit = true;
            ViewBag.gMapId = Id;

            return PartialView("Home/_LeaguesMap_Modal", model);
        }

        /// <summary>
        /// Accion que regresa la vista para mostrar el mapa de la para su edición
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_MapEdit_Modal(int Id, string code)
        {
            var model = db.getUserLeaguesDetailById(Id, code);
            ViewBag.gMapId = Id;

            return PartialView("Ligas/_LeaguesDetail_MapEdit_Modal", model);
        }

        /// <summary>
        /// Actualiza la longitud y latitud de la ubicacion de la liga
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _LeaguesDetail_MapEdit_Save(LeaguesActiveDetailViewModel model)
        {
            var resultJson = new JsonResultViewModel();
            if (model.ligLiga != null)
            {
                if (db.setUpdateLeagueLocation(model.ligLiga.ligId, model.ligLiga.ligCreator, model.ligLiga.ligLatitud, model.ligLiga.ligLongitud))
                {
                    resultJson.booSuccess = true;
                    resultJson.booHasErrMessagePartialView = true;
                    ModelState.AddModelError(constClass.success, "Los datos de la liga se han guardado.");
                }
                else
                {
                    resultJson.booHasErrMessagePartialView = true;
                    ModelState.AddModelError(constClass.error, "Hubo un error actualizando la ubicación. Intenta nuevamente.");
                }
            }
            else
            {
                resultJson.booHasErrMessagePartialView = true;
                ModelState.AddModelError(constClass.error, "Favor de seleccionar la ubicación de la liga.");
            }

            if(resultJson.booHasErrMessagePartialView)
                resultJson.strErrMessagePartialViewString = RenderPartialViewToString("_ModalState_Errors");

            return Json(resultJson);
        }

        /// <summary>
        /// Actualiza la longitud y latitud de la ubicacion de la liga Manual
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _LeaguesDetail_MapEdit_SaveAuto(int ligId, string ligLatitud, string ligLongitud)
        {
            var resultJson = new JsonResultViewModel();
            if (ligId > 0)
            {
                var ligCreator = db.getLigaById(ligId).ligUserIdCreador;
                if (db.setUpdateLeagueLocation(ligId, ligCreator, ligLatitud, ligLongitud))
                {
                    resultJson.booSuccess = true;
                    resultJson.booHasErrMessagePartialView = true;
                    ModelState.AddModelError(constClass.success, "Los datos de la liga se han guardado.");
                }
                else
                {
                    resultJson.booHasErrMessagePartialView = true;
                    ModelState.AddModelError(constClass.error, "Hubo un error actualizando la ubicación. Intenta nuevamente.");
                }
            }
            else
            {
                resultJson.booHasErrMessagePartialView = true;
                ModelState.AddModelError(constClass.error, "Favor de seleccionar la ubicación de la liga.");
            }

            if (resultJson.booHasErrMessagePartialView)
                resultJson.strErrMessagePartialViewString = RenderPartialViewToString("_ModalState_Errors");

            return Json(resultJson);
        }
        /// <summary>
        /// Accion que regresa la vista para mostrar el mapa de la liga.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_Map_Refresh(int Id, string code)
        {
            var model = db.getUserLeaguesDetailById(Id, code);
            ViewBag.gMapId = "";

            return PartialView("Ligas/_LeaguesDetail_Map", model);
        }

        /// <summary>
        /// Accion que regresa la vista para mostrar el mapa de la para su edición
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_MultimediaEdit(int Id, string token)
        {
            if (db.getValidateLeagueByIdToken(Id, token))
            {
                ViewBag.ligId = Id;
                ViewBag.token = token;
            }

            return PartialView("Ligas/_LeaguesDetail_MultimediaEdit");
        }

        [ValidateInput(false)]
        public ActionResult _LeaguesDetail_MultimediaEditCallback(string token, string folder, int Id = 0)
        {
            LeagueImagesEditSettingsViewModel model;
            if (db.getValidateLeagueByIdToken(Id, token))
            {
                if (folder != null)
                    Session.Add("ligIdUploadFile", Id.ToString());

                ViewBag.ligId = Id;
                ViewBag.token = token;

                model = new LeagueImagesEditSettingsViewModel();
                model.RootFolder += Id.ToString();
                return PartialView("Ligas/_LeaguesDetail_MultimediaEditFileManager", model.Model);
            }
            else
            {
                if (folder == null && token == null)
                {
                    if (Session["ligIdUploadFile"] != null)
                    {
                        model = new LeagueImagesEditSettingsViewModel();
                        model.RootFolder += (string)Session["ligIdUploadFile"];
                        Session.Remove("ligIdUploadFile");
                        return PartialView("Ligas/_LeaguesDetail_MultimediaEditFileManager", model.Model);
                    }
                }
            }

            return PartialView("Ligas/_LeaguesDetail_MultimediaEditFileManager");
        }

        /// <summary>
        /// Regresa la vista parcial de las imagenes de la liga.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _LeaguesDetail_MultimediaEdit_Cancel(int Id, string token)
        {
            LeaguesActiveDetailViewModel model = null;
            if (db.getValidateLeagueByIdToken(Id, token))
            {
                model = db.getUserLeaguesDetailById(Id, token);
                model.enableEdit = true;
            }
            return PartialView("Ligas/_LeaguesDetail_Multimedia", model);
        }

        /// <summary>
        /// Regresa la vista de la pantalla de estatus
        /// </summary>
        /// <returns></returns>
        public ActionResult Status()
        {
            return View();
        }

        #endregion

        #region Torneos

        /// <summary>
        /// Regresa la vista de la pantalla de torneos
        /// </summary>
        /// <returns></returns>
        public ActionResult Torneos()
        {           
            return View();
        }
        [HttpGet]   
        public ActionResult TorneoDetails(int? Id)
        {
            if (Id==null)
            {
                return HttpNotFound();
            }else
            {
                var torId = (int)Id;
                //var model = TorneoEquipos(torId);
                //return PartialView("AdminLigas/TorneoEquipos", model);
                return RedirectToAction("TorneoEquipos", new { Id=torId});
            }
            //return PartialView("Equipos/_equiposGrid");
        }
        [HttpGet]
        public ActionResult TorneoEquipos(int Id)
        {
            var torneo = db.getTorneoById((int)Id);
            var liga = db.getLigaById(torneo.ligId);
            var model = new TorneosGridViewModel();
            model.ligNombre = liga.ligNombreLiga;
            model.torId = (int)Id;
            model.torNombre = torneo.torNombreTorneo;

            if (torneo.torEsCoaching == true)
            {
                var teamCoaching = db.getEquipoByTorneo(model.torId);
                if (teamCoaching.Count > 0)
                {
                    ViewBag.ValidateTeamTotal = teamCoaching.Count;
                }
                
            }
            model.coaching = torneo.torEsCoaching;
            return View(model);
        }
        public ActionResult EquipoDetails(int id)
        {
            return View(id);
        }
        
     
        /// <summary>
        /// Regresa la vista de la pantalla de torneos para editar un torneo.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Torneos(int id, string token)
        {
            ViewBag.torneosShowId = id;
            ViewBag.torneosShowToken = token;
            return View();
        }

        /// <summary>
        /// Regresa la vista para crear un nuevo torneo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TorneoNuevo()
        {
            return View();
        }

        /// <summary>
        /// Regresa la vista para crear un nuevo torneo con la liga predeterminada.
        /// </summary>
        /// <param name="ligId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TorneoNuevo(int ligId)
        {
            if (ligId > 0)
                ViewBag.torneoNuevoLigId = ligId;

            return View();
        }

        /// <summary>
        /// Regresa la vista para crear categorias de la liga
        /// </summary>
        /// <returns></returns>
        public ActionResult TorneoCategorias()
        {
            return View();
        }

        /// <summary>
        /// Regresa la vista para agregar canchas a la liga
        /// </summary>
        /// <returns></returns>
        public ActionResult TorneoCanchas()
        {
            return View();
        }

        #endregion

        #region Equipos

        #region Acciones

        /// <summary>
        /// Accion que se ejecuta cuando se va a agregar un nuevo equipo el administrador de ligas
        /// </summary>
        /// <returns></returns>
        public ActionResult EquipoNuevo()
        {
            var model = _EquipoNuevo_NuevoFiltros(null);
            if (model!=null)
            {
                if (model.torId>0)
                {
                    var total = db.getEquipoByTorneo((int)model.torId);
                    var torneo = db.getTorneoById((int)model.torId);
                        
                    model.coaching = torneo.torEsCoaching;
                    model.totalEquipo = total.Count();
                }else
                {
                    model.coaching = false;
                    model.totalEquipo = 0;
                }
            }
            return View(model);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se va a agregar un nuevo equipo el administrador de ligas de un torneo especifico.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="torId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EquipoNuevo(int ligId, int torId)
        {
            EquiposJugadoresFiltrosViewModel model = null;
            if (ligId > 0 && torId > 0)
            {
                ViewBag.equipoNuevoLigId = torId;
                model = _EquipoNuevo_NuevoFiltros(ligId);
                model.ligId = ligId;
                model.torId = torId;
            }
            else
            {
                model = _EquipoNuevo_NuevoFiltros(null);
            }

            return View(model);
        }
        public ActionResult filtroLigaTorneo(int? ligId=null, int? torId=null)
        {
            var liga = (ligId != null) ? (int)ligId : 0;
            var torneo = (torId != null) ? (int)torId : 0;
            var model = _EquipoNuevo_NuevoFiltros(liga, torneo);
      // D:\Enligate\sw_EnligateWeb\Views\Shared\Ligas\_FlitrosLigaTorneo.cshtml
            return PartialView("Ligas/_FlitrosLigaTorneo", model);
        }
        /// <summary>
        /// Accion que se ejecuta cuando se cambia la liga del torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _EquipoNuevoEdit_ChangeLiga(int ligId, bool datEquipo, bool datSinTorneos)
        {
            ModelState.Clear();
            var model = _EquipoNuevo_NuevoFiltros(ligId);

            int torId = (model.torId != null) ? (int)model.torId : 0;
            var result = new EquiposNuevoEditPostbackViewModel();
            result.datosFiltros = RenderPartialViewToString("Ligas/_EquipoNuevo_Filtros", model);
            var model2 = _EquipoNuevo_ViewModel(torId);
            model2.equAdminLigaTorneos = true;
            result.datSinTorneos = (torId > 0) ? false : true;
            //if (datEquipo != model.mostrarDatosEquipo || datSinTorneos || model.torId == null || model.torId == 0)
            //{
            result.recargarPartialCompleto = true;
            result.datosNuevoEquipo = RenderPartialViewToString("Equipos/_NuevoEdit", model2);
            //}
            //else
            //{
            //    result.recargarPartialCompleto = false;
            //    result.datosNuevoDatos = RenderPartialViewToString("Equipos/_NuevoEdit_Datos", model2); ;
            //}
            result.datosAgregar = "";

            if (result.datSinTorneos)
            {
                var torneo = db.getTorneoById(torId);
                if (torneo!=null)
                {
                    if (torneo.torEsCoaching)
                    {
                        var equipo = db.getEquipoByTorneo(torId);
                        if (equipo!=null && equipo.Count > 0)
                        {
                            equipo.OrderByDescending(o=> o.equFechaCreacionUTC).FirstOrDefault();
                            if (equipo.Count > 1)
                            {
                                result.datosAgregar = "False";
                                //result.datosEquipoCoach = RenderPartialViewToString("Equipos/_EquipoCoach", equipo);
                            }
                        }
                        
                    }
                }
            }
            result.datosParticipantes = "";
            result.mostrarDatosEquipo = model2.mostrarDatosEquipo;

            return Json(result);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se cambia el torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _EquipoNuevoEdit_ChangeTorneo(int torId, bool datEquipo)
        {
            ModelState.Clear();
            var model2 = _EquipoNuevo_ViewModel(torId);
            model2.equAdminLigaTorneos = true;
            var model = _EquipoNuevo_NuevoFiltros(model2.tblTorneo.ligId, torId);
            var result = new EquiposNuevoEditPostbackViewModel();
            result.datSinTorneos = false;
            result.datosFiltros = "";
            result.recargarPartialCompleto = true;
            result.datosNuevoEquipo = RenderPartialViewToString("Equipos/_NuevoEdit", model2);
   
            result.datosAgregar = "";
            if (torId>0)
            {
                ViewBag.numEquipos = 0;
                var torneo = db.getTorneoById(torId);
                if (torneo != null)
                {
                    if (torneo.torEsCoaching)
                    {
                        result.esCoaching = true;
                        result.datosAgregar = "False";
                        var equipo = db.getEquipoByTorneo(torId);
                        //var equipoModel = new EquiposJugadoresViewModel();
                        if (equipo != null && equipo.Count > 0)
                        {
                            var equipo2 = equipo.OrderByDescending(o => o.equFechaCreacionUTC);
                            if (equipo.Count > 0)
                                result.numequipos = equipo.Count;
                            else
                                result.numequipos = 0;
                        }else
                        {
                            result.numequipos = 0;
                        }
                    }
                }
            }
            result.datosParticipantes = "";
            result.mostrarDatosEquipo = model2.mostrarDatosEquipo;

            return Json(result);
        }

        #endregion

        #region Funciones

        /// <summary>
        /// Prepara los filtros para crear un nuevo equipo y/o participantes de un torneo. 
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="torId"></param>
        /// <returns></returns>
        public EquiposJugadoresFiltrosViewModel _EquipoNuevo_NuevoFiltros(int? ligId = null, int? torId = null)
        {
            var model = new EquiposJugadoresFiltrosViewModel();
            ApplicationUser user = db.getUserById(User.Identity.GetUserId());
            //Carga las ligas de los filtros
            model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id,user.usuRolActual);

            if (ligId != null)
                model.ligId = (int)ligId;
            var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == model.ligId.ToString());
            if (ligaSeleccionada != null)
                ligaSeleccionada.Selected = true;
            else if (model.ddlLigas.Count > 0)
                model.ligId = int.Parse(model.ddlLigas.First().Value);
            else
                model.ligId = 0;

            model.ddlTorneos = _EquipoNuevo_GetDropDownListTorneos((int)model.ligId, user.usuRolActual, user.Id);
            if (model.ddlTorneos.Count<=0)
            {
                var ddl = new List<SelectListItem>();
                ddl.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
                model.ddlTorneos = ddl;
                torId = 0;
            }
            if (torId != null)
                model.torId = torId;
            else if (model.ddlTorneos.Count > 0)
                model.torId = int.Parse(model.ddlTorneos.First().Value);

            if (model.torId != null && model.torId > 0)
            {
                model.tblTorneo = db.getTorneoById((int)model.torId);
                model.recargarImagen = true;
                if (model.tblTorneo.torTipo == constClass.torTipoCoaching || !model.tblTorneo.torDeporteEnEquipo)
                    model.mostrarDatosEquipo = false;
                else
                    model.mostrarDatosEquipo = true;
            }

            return model;
        }

        /// <summary>
        /// Regresa las ligas que le pertenecen al usuario.
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> _EquipoNuevo_GetDropDownListLigas(string user_Id,string user_usuRolActual, int? ligId=null,string dep = "")
        {
            if (ligId!=null)
            {
                var liga = db.getLigaById((int)ligId);

                if (liga != null)
                {
                    var ddl = new List<SelectListItem>();
                    ddl.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                    return ddl;
                }
            }
            if (dep!="")
            {                
                var dataDep = db.getLigasByDeportes(dep, user_Id)
                     .Select(l => new SelectListItem { Text = l.ligNombreLiga.ToUpper(), Value = l.ligId.ToString() })
                     .ToList();
                return dataDep;
            }

            var rolPlayer = db.getRoleByName(constClass.rolPlayer).Id;
            var rolCoach = db.getRoleByName(constClass.rolCoach).Id;
            var rolTorneo = db.getRoleByName(constClass.rolAdminTorneos).Id;

            if (user_usuRolActual.ToUpper()==rolPlayer.ToUpper())
            {
                var userId = user_Id;
                var ligas = db.getLigasByJugador(userId);
                return ligas.Select(l => new SelectListItem { Text = l.ligNombreLiga.ToUpper(), Value = l.ligId.ToString() })
                     .ToList();
            }
            else if (user_usuRolActual.ToUpper() == rolCoach.ToUpper())
            {
                var user = db.getUserById(user_Id);
                var part = db.getPartidosEquipos(user).OrderBy(o=> o.torId);

                var torIds = new List<int>();
                var ligIds = new List<int>();
                var tor = new List<schemaTorneos>();
                var lig = new List<schemaLigas>();
                foreach (var item in part)
                {
                    if (!torIds.Contains(item.torId))
                    {
                        torIds.Add(item.torId);
                        var torneo = db.getTorneoById(item.torId);
                        lig.Add(db.getLigaById(torneo.ligId));
                    }
                }
                
                return lig.Select(l => new SelectListItem { Text = l.ligNombreLiga.ToUpper(), Value = l.ligId.ToString() })
                     .ToList();
            }
            else if (user_usuRolActual.ToUpper()== rolTorneo.ToUpper())
            {
                var torneos = db.getTorneosByUser(user_Id);
                var ddl = new List<SelectListItem>();
                if (torneos!=null || torneos.Count > 0)
                {
                    foreach (var item in torneos)
                    {                         
                        var liga = db.getLigaById(item.ligId);

                        var exist = ddl.Where(l => l.Value == liga.ligId.ToString());
                        if (exist == null || exist.Count() <= 0)
                        {
                            ddl.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                        }                        
                    }                    
                }
                return ddl;                
            }

            var data= db.getUserLeagues(user_Id, user_usuRolActual)
                     .Select(l => new SelectListItem { Text = l.ligNombre.ToUpper(), Value = l.ligId.ToString() })
                     .ToList();
                        
            return data;
        }

        /// <summary>
        /// Regresa la lista de los torneos que pertenecen a la liga que no tienen una fecha de termino
        /// o que no han terminado
        /// </summary>
        /// <param name="ligId"></param>
        /// <returns></returns>
        protected List<SelectListItem> _EquipoNuevo_GetDropDownListTorneos(int ligId, string user_usuRolActual, string user_Id)
        {
            var Torneos = new List<SelectListItem>();
            /*if (ligId!=0)
            {
                Torneos = db.getTorneosByLiga(ligId)
                     .Where(t => t.torFechaTermino == null
                              || t.torFechaTermino >= db.DateTimeMX() && t.torEstatus == true)
                     .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                     .ToList();
            }
            */

            var rolCoach = db.getRoleByName(constClass.rolCoach).Id;
            var rolTorneo = db.getRoleByName(constClass.rolAdminTorneos).Id;
            if (user_usuRolActual.ToUpper() == rolCoach.ToUpper())
            {
                var user = db.getUserById(user_Id);
                var part = db.getPartidosEquipos(user).OrderBy(o => o.torId).Where(l=> l.ligId == ligId).ToList();
                var torIds = new List<int>();
                var ligIds = new List<int>();
                var tor = new List<schemaTorneos>();
                var lig = new List<schemaLigas>();
                foreach (var item in part)
                {
                    if (!torIds.Contains(item.torId))
                    {
                        torIds.Add(item.torId);
                        var torneo = db.getTorneoById(item.torId);
                        tor.Add(torneo);

                    }
                }

                return tor.Where(t => t.torFechaTermino == null || t.torFechaTermino >= db.DateTimeMX() && t.torEstatus == true)
                              .Select(l => new SelectListItem { Text = l.torNombreTorneo.ToUpper(), Value = l.torId.ToString() })
                              .ToList();
            }
            else if (user_usuRolActual.ToUpper() == rolTorneo.ToUpper())
            {
                var torneos = db.getTorneosByUser(user_Id).Where(l=> l.ligId == ligId).ToList();
                return torneos.Where(t => t.torFechaTermino == null || t.torFechaTermino >= db.DateTimeMX() && t.torEstatus == true)
                              .Select(l => new SelectListItem { Text = l.torNombreTorneo.ToUpper(), Value = l.torId.ToString() })
                              .ToList();
            }
            else
            {
                Torneos = db.getTorneosByLiga(ligId)
                     .Where(t => t.torFechaTermino == null
                              || t.torFechaTermino >= db.DateTimeMX() && t.torEstatus == true)
                     .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                     .ToList();
            }

            return Torneos;
        }

        /// <summary>
        /// Regresa el modelo de un nuevo equipo o participantes de un torneo en especifico.
        /// Utiliza la funcion de el AdminEquiposController
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        protected EquiposJugadoresViewModel _EquipoNuevo_ViewModel(int torId)
        {
            var adminEquipoCtrl = new AdminEquiposController();
            var model = adminEquipoCtrl._EquipoNuevo_OnLoadViewModel(torId,0);
            

            return model;
        }

        #endregion

        #endregion

        #region Partidos
        public ActionResult Partidos()
        {
            PartidosViewModel data = new PartidosViewModel();
            data.dep = false;
            var model = _Partido_NuevoFiltros(data);
            model.parFecha_Inicio = db.DateTimeMX();
            model.parFecha_Fin = db.DateTimeMX();
            model.parHour = 0;
            model.parMinutes = 30;
            return View(model);
            //return RedirectToAction("MainLeague");
        }
        public ActionResult _EventoNuevo(DateTime date, int? ligId=null, int? torId=null)
        {
            PartidosViewModel data = new PartidosViewModel();
            ViewBag.dateSelect = date;
            data.parFecha_Inicio = date;
            data.dep = false;
            data.ligId = (ligId!=null)?(int)ligId:0;
            data.torId = (torId!=null)?(int)torId:0;
            var model = _Partido_NuevoFiltros(data);
            if (model.torId!=0)
            {
                var torneo = db.getTorneoById(model.torId);
                ViewBag.TorneoInit = torneo.torFechaInicio;
                ViewBag.TorneoEnd = torneo.torFechaTermino;
                if (torneo.torFechaInicio <= date)
                {
                    model.parFecha_Inicio = date;
                }else
                {
                    model.parFecha_Inicio = (DateTime)torneo.torFechaInicio  ;
                }
            }            
            
            //model.parFecha_Inicio = date;
            model.parEstatus = true;
            model.parHour = 0;
            model.parMinutes = 30;
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Filtros", model);
            //return RedirectToAction("MainLeague");
        }
        public ActionResult _Partidos_Edit(int parId)
        {
            PartidosViewModel model = new PartidosViewModel();
            var data = db.getPartidosById(parId);
            var fecha = data.parFecha_Fin;
                       
            var dateMx = db.DateTimeMX();
            if (fecha < dateMx)
            {
                return _Partidos_Refeere(parId);
            }
            var result = false;
            var parEstado = data.parEstado;
            var equResultadoUno = data.equResultadoUno;
            var equResultadoDos = data.equResultadoDos;

            model.ligId = data.ligId;
            model.torId = data.torId;
            model.equIdUno = data.equIdUno;
            model.equIdDos = data.equIdDos;
            model.canId = data.lcatId;
            model.parId = data.parId;
            model.parFecha_Inicio = data.parFecha_Inicio;
            model.parFecha_Fin = data.parFecha_Fin;
            model.arbId = data.arbId;

            model = _Partido_NuevoFiltros(model);
            
            if (dateMx.Date > fecha.Date)
                result = true;
            model.result = result;
            model.parEstado = parEstado;
            model.equResultadoUno = equResultadoUno;
            model.equResultadoDos = equResultadoDos;
            model.parEstatus = data.parEstatus;
            model.arbNombre = data.arbNombre;
            
            model.equNombreEquipoUno = data.equNombreEquipoUno;
            model.equNombreEquipoDos = data.equNombreEquipoDos;
            model.imgUno = db.getEquipoById(data.equIdUno).equImgUrl;
            model.imgDos = (data.equIdDos > 0) ? db.getEquipoById(data.equIdDos).equImgUrl : data.imgDos;
            model.equIdUno = data.equIdUno;
            model.equIdDos = data.equIdDos;
            if (model.tblTorneo != null)
            {
                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
                ViewBag.dateSelect = (DateTime)model.tblTorneo.torFechaInicio;
            }
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Filtros", model);
            //return RedirectToAction("MainLeague");
        }
        public ActionResult _Partidos_Refeere(int parId)
        {
            PartidosViewModel model = new PartidosViewModel();
            var data = db.getPartidosById(parId);
            var fecha = data.parFecha_Fin;
            var result = false;
            var parEstado = data.parEstado;
            var equResultadoUno = data.equResultadoUno;
            var equResultadoDos = data.equResultadoDos;

            model.ligId = data.ligId;
            model.ligNombre = data.tblLigas.ligNombreLiga;
            model.torId = data.torId;
            model.torNombre = data.tblTorneos.torNombreTorneo;
            model.equIdUno = data.equIdUno;
            model.equNombreEquipoUno = data.equNombreEquipoUno;
            model.equNombreEquipoDos = data.equNombreEquipoDos;
            model.equIdDos = data.equIdDos;
            model.canId = data.lcatId;
            var cancha = db.getCanchasbyLigas(data.ligId).Where(l => l.lcatId == data.lcatId).FirstOrDefault();
            if (cancha!=null)
            {
                model.lat = float.Parse(cancha.lcatLatitud);
                model.lng = float.Parse(cancha.lcatLongitud);                
                model.canNombre = cancha.lcatNombre;
                model.canchaDireccion = cancha.lcatdomicilio + " #" + cancha.lcatNumExtInt + " " + cancha.lcatColonia + " " + cancha.lcatMunicipio + " " + cancha.lcatEstado + " , C.P: " + cancha.lcatCodigoPostal;
            }else
            {
                model.canNombre = "Sin Cancha";
                model.canchaDireccion = " Sin Direccion";
                model.canId = 0;
            }
           
            model.depNombre = data.tblTorneos.tblCategoriaTorneo.depNombre;
            model.parId = data.parId;
            model.parFecha_Inicio = data.parFecha_Inicio;
            // model = _Partido_NuevoFiltros(model);
            var aux = db.getJugadoresByTorneo_Equipo(model.torId, model.equIdUno);
            model.ddlJugUno = db.getJugadoresByTorneo_Equipo(model.torId, model.equIdUno).Where(l => l.jugConfirmado == true).ToList(); ;
            model.ddlJugDos = db.getJugadoresByTorneo_Equipo(model.torId, model.equIdDos).Where(l => l.jugConfirmado == true).ToList(); ;
            //.Where(l => l.jugConfirmado == true).ToList();
            
            if (db.DateTimeMX() > fecha)
                result = true;
            model.result = result;
            model.parEstado = parEstado;
            model.equResultadoUno = equResultadoUno;
            model.equResultadoDos = equResultadoDos;
            model.arbId = data.arbId;
            model.arbNombre = (data.arbNombre != null)? data.arbNombre: (data.arbId!=null && data.arbId >0)? db.getArbitroById(data.arbId).arbNombre : "" ;
            model.imgUno = db.getEquipoById(data.equIdUno).equImgUrl;
            model.imgDos = (data.equIdDos>0) ? db.getEquipoById(data.equIdDos).equImgUrl : data.imgDos;
            ViewBag.haveResults = false;
            var est = db.getEstadisticaFutbolByPartido(model.parId);

            if (est.Count > 0)
            {
                ViewBag.haveResults = true;
            }
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Refeere", model);
            //return RedirectToAction("MainLeague");
        }

        public ActionResult _ResultadoByPartido(int parId)
        {
            var model = new ResultadoPartido();
            var est = db.getEstadisticaFutbolByPartido(parId);

            if (est!=null)
            {
                List<string> list = new List<string>();
                int i = 0;
                foreach (var item in est)
                {
                    var jug = db.getJugadorByUserId(item.UserIdJugador,"");
                    var ddl = new List<schemaJugadorEquipos>();
                    var newJug = new schemaJugadorEquipos();
                    newJug.jugUserId = jug.jugUserId;
                    newJug.jugCorrreo = jug.jugCorrreo;
                    // ddl.Add(new SelectListItem { Text = jug.jugNombre, Value = jug.jugUserId });
                    ddl.Add(newJug);
                    model.ddlJugUno = ddl;
                    model.equIdUno = item.equId;
                    model.equNombreEquipoUno = item.tblEquipo.equNombreEquipo;
                    model.goles = item.jfejGoles;
                    model.faltas = item.jfejFaltas;
                    model.parSuspendido = item.jfejPartidosSuspendidos;
                    model.roja = item.jfejTarjetasRojas;
                    model.amarillas = item.jfejTarjetasAmarillas;
                    model.Id = i;
                    model.asistencias = item.jfejAsistencias;
                    //  list.Add(RenderPartialViewToString("Admin/_Perfil", model._profile));           
                    list.Add(RenderPartialViewToString("Referee/_EquipoJugadorSelect", model));
                    i++;
                }

                var json = list.ToArray();
                return Json(json);
            }            

            return PartialView("Referee/_EquipoJugadorSelect", model);
        }
        public ActionResult _Partidos_Nuevo(int? ligId=null,int? torId=null)
        {
            PartidosViewModel data = new PartidosViewModel();
            if (ligId!=null)
                data.ligId = (int)ligId;

            if (torId!=null)
                data.torId = (int)torId;

            data.dep = false;
            var model = _Partido_NuevoFiltros(data);
            model.parEstatus = true;
            model.parHour = 0;
            model.parMinutes = 30;
            if (model.tblTorneo != null)
            {
                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
                ViewBag.TorneoEnd = model.tblTorneo.torFechaTermino;
            }
            model.parFecha_Inicio = (ViewBag.TorneoInit != null) ? (ViewBag.TorneoInit > db.DateTimeMX()) ? ViewBag.TorneoInit : db.DateTimeMX() : db.DateTimeMX(); ;

            return PartialView("Ligas/_Partidos_Filtros",model);
        }
        public ActionResult _Change_Deportes_Option(PartidosViewModel model)
        {
            model = _Partido_NuevoFiltros(model);
            model.parHour = (model.parHour == 0) ? 0  : model.parHour;
            model.parMinutes = (model.parMinutes == 0) ? 30 : model.parMinutes;
            if (model.tblTorneo != null)
            {
                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
            }
            return PartialView("Ligas/_Partidos_Filtros", model);
        }
        public ActionResult _Change_Liga_Option(PartidosViewModel model, DateTime dateSelect)
        {
            model = _Partido_NuevoFiltros(model);
            model = _Partido_NuevoFiltros(model);
            model.parHour = (model.parHour == 0) ? 0 : model.parHour;
            model.parMinutes = (model.parMinutes == 0) ? 30 : model.parMinutes;
            ViewBag.dateSelect = dateSelect;
            if (model.tblTorneo != null)
            {
                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
                if (model.tblTorneo.torFechaInicio <= dateSelect)
                {
                    model.parFecha_Inicio = dateSelect;
                }
                else
                {
                    model.parFecha_Inicio = (DateTime)model.tblTorneo.torFechaInicio;
                }
            }
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Filtros", model);
        }
        public ActionResult _Change_Torneo_Option(PartidosViewModel model,DateTime dateSelect)
        {
            model = _Partido_NuevoFiltros(model);
            model.parHour = (model.parHour == 0) ? 0 : model.parHour;
            model.parMinutes = (model.parMinutes == 0) ? 30 : model.parMinutes;
            ViewBag.dateSelect = dateSelect;
            if (model.tblTorneo != null)
            {
                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
                if (model.tblTorneo.torFechaInicio <= dateSelect)
                {
                    model.parFecha_Inicio = dateSelect;
                }
                else
                {
                    model.parFecha_Inicio = (DateTime)model.tblTorneo.torFechaInicio;
                }
            }
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Filtros", model);
        }
        public ActionResult _Change_Equipo_Option(PartidosViewModel model)
        {
            model.parHour = (model.parHour == 0) ? 0 : model.parHour;
            model.parMinutes = (model.parMinutes == 0) ? 30 : model.parMinutes;

            model = _Partido_NuevoFiltros(model);
            ViewBag.dateTimeMx = db.DateTimeMX();
            return PartialView("Ligas/_Partidos_Filtros", model);
        }
        public PartidosViewModel _Partido_NuevoFiltros(PartidosViewModel data = null)
        {
            PartidosViewModel model = new PartidosViewModel();
            ApplicationUser user = db.getUserById(User.Identity.GetUserId());
            //Lista de Deportes 
            if (data.dep != false)
            {
                model.ddlDeportes = db.getDeportes_Active()
                   .Select(t => new SelectListItem { Text = t.depNombre.ToUpper(), Value = t.depNombre.ToString() })
                    .ToList();

                if (data.depNombre != null)
                {
                    var depSeleccionada = model.ddlDeportes.FirstOrDefault(l => l.Value == data.depNombre);
                    if (depSeleccionada != null)
                        depSeleccionada.Selected = true;
                    model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id, user.usuRolActual, null, data.depNombre);
                }
                else
                    model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id, user.usuRolActual, null, model.ddlDeportes.First().Value);
                
                model.dep = data.dep;
            }
            else
            {
                model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id, user.usuRolActual);
            }
            //Carga  las ligas de los filtros
            model.ligId= 0;
            if (model.ddlLigas.Count > 0)
            {
                if (data.ligId > 0)
                {
                    model.ligId = data.ligId;
                    var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == model.ligId.ToString());
                    if (ligaSeleccionada != null)
                        ligaSeleccionada.Selected = true;
                }
                else
                    model.ligId = int.Parse(model.ddlLigas.First().Value);
            }
            //Torneos
            model.ddlTorneos = _EquipoNuevo_GetDropDownListTorneos((int)model.ligId, user.usuRolActual, user.Id);
            if (model.ddlTorneos.Count > 0)
            {
                if (data.torId > 0)
                {
                    model.torId = data.torId;
                    var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == model.torId.ToString());
                    if (torneoSeleccionada != null)
                        torneoSeleccionada.Selected = true;
                    else
                        model.torId = int.Parse(model.ddlTorneos.First().Value);
                    model.torTipo  = db.getTorneoById(data.torId).torTipo;
                     
                }
                else
                {
                    model.torId = int.Parse(model.ddlTorneos.First().Value);
                    var tipo = db.getTorneoById(model.torId);
                    model.torTipo = tipo.torTipo;
                }
                                   
            }
            else
            {
                var ddl = new List<SelectListItem>();
                ddl.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
                model.ddlTorneos = ddl;
                model.torId = 0;
            }
            model.tblTorneo = db.getTorneoById(model.torId);
            var arbitros = db.getArbitros(model.ligId);
            
            model.ddlArbitros.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
            model.ddlArbitros.AddRange(arbitros.Select(t => new SelectListItem { Text = (t.arbNombre != null) ? t.arbNombre.ToUpper() : "", Value = t.arbId.ToString() }).ToList());
            if (data.arbId > 0)
            {
                var arbSeleccionada = model.ddlArbitros.FirstOrDefault(l => l.Value == data.arbId.ToString());
                if (arbSeleccionada != null)
                {
                    arbSeleccionada.Selected = true;
                    model.arbId = data.arbId;
                }                    
            }
            
            //model.arbNombre = arbitros.;
            //Canchas
            model.ddlCanchas = new List<SelectListItem>();
            model.ddlCanchas.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
            var listCanchas = _Partidos_GetDropDownListCanchas(user.Id, user.usuRolActual, model.ligId);
            if (listCanchas != null)
            {
                model.ddlCanchas.AddRange(listCanchas);
            }
            
            if (data.canId > 0)
            {
                var canchaSeleccionada = model.ddlCanchas.FirstOrDefault(l => l.Value == data.canId.ToString());
                if (canchaSeleccionada != null) { 
                    canchaSeleccionada.Selected = true;
                    model.canId = data.canId;
                }
            }                

            //Equipos
            

            if (model.ddlTorneos.Count > 0 && model.torId > 0)
            {
                var torId = model.torId;
                schemaEquipos imgDos = new schemaEquipos();                    
                var equIdUno = data.equIdUno.ToString();
                var equIdDos = data.equIdDos.ToString();

                ViewBag.TorneoInit = model.tblTorneo.torFechaInicio;
                ViewBag.TorneoEnd = model.tblTorneo.torFechaTermino;

                if (model.tblTorneo.torEsCoaching)
                        model.ddlEquipoUno = _Partidos_GetDropDownListEquipos(torId,true);
                    else
                        model.ddlEquipoUno = _Partidos_GetDropDownListEquipos(torId,false);

                    if (model.ddlEquipoUno.Count() != 0)
                    {
                        var equipoUnoSeleccionada = model.ddlEquipoUno.FirstOrDefault(l => l.Value == equIdUno);
                        if (equipoUnoSeleccionada != null)
                            equipoUnoSeleccionada.Selected = true;
                        else
                            equIdUno = model.ddlEquipoUno.First().Value;

                        var imgUno = db.getEquipoById(int.Parse(equIdUno));
                        model.imgUno = imgUno.equImgUrl;

                        if (!model.tblTorneo.torEsCoaching)
                        {

                            model.ddlEquipoDos = _Partidos_GetDropDownListEquipos(torId, false, int.Parse(equIdUno));

                            if (model.ddlEquipoDos.Count() != 0)
                            {
                                if (equIdUno != equIdDos)
                                {
                                    var equipoDosSeleccionada = model.ddlEquipoDos.FirstOrDefault(l => l.Value == equIdDos);

                                    if (equipoDosSeleccionada != null)
                                        equipoDosSeleccionada.Selected = true;
                                    else if (model.ddlEquipoDos.Count() != 0)
                                        equIdDos = model.ddlEquipoDos.First().Value;

                                    imgDos = db.getEquipoById(int.Parse(equIdDos));
                                    model.imgDos = imgDos.equImgUrl;
                                }
                                else
                                {
                                    var equipoDosSeleccionada = model.ddlEquipoDos.First();
                                    if (equipoDosSeleccionada != null)
                                    {
                                        equipoDosSeleccionada.Selected = true;
                                        imgDos = db.getEquipoById(int.Parse(model.ddlEquipoDos.First().Value));
                                    }
                                }
                                model.imgDos = imgDos.equImgUrl;
                            }
                        }
                    else
                    {
                        model.equIdDos = 0;
                    }
                } 
            }
            /*if ((int)model.torId > 0)
            {
                if(data.equIdUno <= 0 && data.equIdDos <= 0)
                {
                    model.ddlEquipoUno = _Partidos_GetDropDownListEquipos((int)model.torId,model.tblTorneo.torEsCoaching);
                    if (model.ddlEquipoUno.Count > 0)
                    {
                        var imgUno = db.getEquipoById(int.Parse(model.ddlEquipoUno.First().Value));
                        model.imgUno = imgUno.equImgUrl;
                    }
                }
            }*/
            if (data.parId > 0)
                model.parId = (int)data.parId;

            model.parHour = (data.parFecha_Inicio < data.parFecha_Fin)? (int)(data.parFecha_Fin - data.parFecha_Inicio).Hours : data.parHour;

            model.parMinutes = (data.parFecha_Inicio < data.parFecha_Fin) ? (int)(data.parFecha_Fin - data.parFecha_Inicio).Minutes : data.parMinutes;

            model.parFecha_Inicio = data.parFecha_Inicio;

            ViewBag.dateTimeMx = db.DateTimeMX();
            return model;
        }

        protected List<SelectListItem> _Partidos_GetDropDownListEquipos(int torId,  bool esCoach,int? equId = null)
        {
            if (esCoach)
            {                
                var equipo = db.getEquiposByTorneo(torId)
                    .OrderBy(o => o.equFechaCreacionUTC)
                    .FirstOrDefault();
                if (equipo!=null)
                {
                    var ddl = new List<SelectListItem>();
                    ddl.Add(new SelectListItem { Text = equipo.equNombreEquipo.ToUpper(), Value = equipo.equId.ToString() });
                    return ddl;
                }
                    
            }
            if (equId!=null)
            {
                var equipos = db.getEquiposByTorneo(torId)
                     .Where(x => x.equId != equId)
                     .Select(t => new SelectListItem { Text = t.equNombreEquipo.ToUpper(), Value = t.equId.ToString() })
                     .ToList();
                return equipos;
            }

            return db.getEquiposByTorneo(torId)
                     .Select(t => new SelectListItem { Text = t.equNombreEquipo.ToUpper(), Value = t.equId.ToString() })
                     .ToList();
        }
        
        protected List<SelectListItem> _Partidos_GetDropDownListCanchas(string user_Id, string user_usuRolActual,int ligId)
        {
            if (ligId>0)
            {
                var cancha = db.getLigasCanchasTorneos(user_Id, user_usuRolActual);
                if (cancha != null)
                {                    
                    return cancha.OrderByDescending(o => o.fechaCreacion).Where(l => l.ligId == ligId)
                        .Select(t => new SelectListItem { Text = t.lcatNombre.ToUpper(), Value = t.lcatId.ToString() })
                        .ToList();
                } 
                    
            }else
            {
                var ddl = new List<SelectListItem>();
                ddl.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
                return ddl;
            }
            return null;
        }
        public ActionResult _Partidos_Grid()
        {            
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_PartidosGrid");
        }
            
        /// <summary>
        /// Accion que se activa cuando el Grid hace Callback
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult PartidosCallback(int torId)
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
            List<PartidosViewModel> model = new List<PartidosViewModel>();

            var roles = db.getRoleByName(constClass.rolPlayer);
            if (user.usuRolActual == roles.Id.ToUpper())
                model = db.getPartidosPlayer(user);
            else
                model = db.getPartidos(user).Where(l=> l.torId==torId).ToList();
            model = model.Where(l=> l.parEstatus == true).OrderByDescending(o => o.parFecha_Inicio).ToList();
            return PartialView("Torneos/_PartidosGrid", model);
        }
        [AllowAnonymous]
        public ActionResult PartidosViewCallback(int? torId=null, int? equId=null)
        {
            List<PartidosViewModel> model = new List<PartidosViewModel>();
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                var user = db.getUserById(userId);
                _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
                var role1 = db.getRoleByName(constClass.rolOwners);
                var role2 = db.getRoleByName(constClass.rolCoach);
                var role3 = db.getRoleByName(constClass.rolReferee);
                var role4 = db.getRoleByName(constClass.rolAdminTorneos);
                if (torId!=null && torId > 0)
                {
                    model = db.getPartidos(null).Where(l => l.parFecha_Fin > db.DateTimeMX() && l.torId == torId).ToList();
                }else if (equId != null)
                {
                    model = model.Where(l => l.equIdUno == equId || l.equIdDos == equId).ToList();
                }                    
                else
                { 
                    if (role1.Id.ToUpper() == user.usuRolActual.ToUpper())
                    {
                        var partido = db.getPartidos(user);
                        model = partido.Where(l => l.parFecha_Fin > db.DateTimeMX()).ToList();
                    }                    
                    else if (role2.Id.ToUpper() == user.usuRolActual.ToUpper())
                    {
                        model = db.getPartidosEquipos(user).Where(l => l.parFecha_Fin > db.DateTimeMX()).ToList();
                    }
                    else if (role3.Id.ToUpper() == user.usuRolActual.ToUpper())
                    {
                        model = db.getPartidosRefeere(user).Where(l => l.parFecha_Fin > db.DateTimeMX()).ToList();
                    }else if (role4.Id.ToUpper() == user.usuRolActual.ToUpper())
                    {
                        var partido = db.getPartidos(user);
                        model = partido.Where(l => l.parFecha_Fin > db.DateTimeMX()).ToList();
                    }
                    else
                    {
                        model = db.getPartidosPlayer(user).Where(l => l.parFecha_Fin > db.DateTimeMX()).ToList();
                      
                    }
                }
                //var model = db.getPartidos(user).Where(l => l.parFecha_Fin > db.DateTimeMX());
            }else
            {
                model = db.getPartidos(null).Where(l=> l.parFecha_Fin > db.DateTimeMX() && l.torId==torId).ToList();
                if (equId != null)
                {
                    model = model.Where(l => l.equIdUno == equId || l.equIdDos == equId).ToList();
                }
            }
            model = model.Where(l=> l.parCheck == false).OrderByDescending(o=> o.parFecha_Inicio).Take(10).ToList();
            
            return PartialView("Torneos/_Partidos_Show", model);
        }
        [AllowAnonymous]
        public ActionResult PartidosResultCallback(int? torId=null,int? equ = null)
        {
            List<PartidosViewModel> model = new List<PartidosViewModel>();
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                var user = db.getUserById(userId);
                _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
                var role2 = db.getRoleByName(constClass.rolCoach);
                var role3 = db.getRoleByName(constClass.rolPlayer);
                if (torId!=null && torId > 0) {
                    //model = db.getPartidos(null).Where(l => l.parFecha_Fin < db.DateTimeMX() && l.torId == torId).ToList();
                    model = db.getPartidos(null).Where(l => l.torId == torId).ToList();
                }
                else if (role2.Id.ToUpper() == user.usuRolActual.ToUpper())
                {
                    //model = db.getPartidosEquipos(user).Where(l => l.parFecha_Fin < db.DateTimeMX()).ToList();
                    model = db.getPartidosEquipos(user).Where(l => l.parCheck == true).ToList();
                }
                else if (role3.Id.ToUpper() == user.usuRolActual.ToUpper())
                {
                    model = db.getPartidosPlayer(user);                  
                }
                else
                    model = db.getPartidos(user);
            }
            else
            {
                model = db.getPartidos(null).Where(l => l.parCheck == true && l.torId==torId).ToList();
            }
            
            model = model.Where(l=> l.parCheck == true ).OrderByDescending(o => o.parFecha_Fin).Take(10).ToList();            
            //model = model.Where(l => l.parFecha_Fin >= db.DateTimeMX().AddDays(-2)).ToList();
            return PartialView("Torneos/_Partidos_Resultados", model);
        }
        /// <summary>
        /// Funcion que llena los ViewData que utiliza el Grid de Partidos para llenar los combobox
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        protected void _PartidosGridEdit_EditViewData(string userId, string roleId)
        {
            List<schemaTorneos> Torneos = new List<schemaTorneos>();
            List<schemaEquipos> Equipos = new List<schemaEquipos>();
            List<schemaLigaCanchasTorneos> Canchas = new List<schemaLigaCanchasTorneos>();

            var ligas = db.getLigasActivasByUser(db.getUserLeagues(userId, roleId));

            for (int i = 0; i < (ligas.Count)-1 ; i++)
            {
               var aux = ligas[i].ligId;
               Torneos = db.getTorneosByLiga(aux);
               Canchas = db.getCanchasbyLigas(aux);
            }

            for (int i = 0; i < (Torneos.Count) - 1; i++)
            {
                var aux = Torneos[i].torId;
                Equipos = db.getEquiposByTorneo(aux);
            }

            ViewData["cmbLigas"] = ligas;
            
            ViewData["cmbTorneos"] = Torneos;          
                        
            ViewData["cmbEquipos"] = Equipos;

            ViewData["cmbCanchas"] = Canchas;            
        }
        public ActionResult Partido_ChageLiga(int ligId)
        {
            var model = _EquipoNuevo_NuevoFiltros(ligId);
            var item = new PartidosViewModel();
            item.ddlLigas = model.ddlLigas;
            item.ddlTorneos = model.ddlTorneos;
            item.ligId = (int)model.ligId;
            item.torId = (int)model.torId;

            return PartialView("Ligas/_FiltroPartidos", item);
        }
        #endregion
        /// <summary>
        /// Regresa la vista de la pantalla de calendario
        /// </summary>
        /// <returns></returns>
        public ActionResult Calendario()
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var partido = db.getPartidos(user);            
            //PartidosViewModel model = adm._Calendario_Event(user);

            var model = filtros_Calendario(user);
            model.numPartidos = partido.Count;
            
            return View(model);
        }
        public PartidosViewModel filtros_Calendario(ApplicationUser user, int? ligId=null,int? torId=null)
        {
            PartidosViewModel model = new PartidosViewModel();
            var ddlLiga = new List<SelectListItem>();
            var ddlTorneo = new List<SelectListItem>();
            var ddl = new List<SelectListItem>();
            
            model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id, user.usuRolActual);

            model.ddlLigas.Add(new SelectListItem { Text = "-- Todas --", Value = "0" });
            if (ligId != null)
            {
                var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                if (ligaSeleccionada != null)
                    ligaSeleccionada.Selected = true;
            }

            if (model.ddlLigas != null && model.ddlLigas.Count > 0)
            {
                if (ligId != null)
                    model.ligId = (int)ligId;
                else
                    model.ligId = 0;

                if (user.usuRolActual.ToUpper() == db.getRoleByName(constClass.rolAdminTorneos).Id.ToUpper())
                {
                    model.ddlTorneos = db.getTorneosByUser(user.Id).Where(l=> l.ligId == model.ligId)
                                             .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                             .ToList();
                }
                else if (user.usuRolActual.ToUpper() == db.getRoleByName(constClass.rolCoach).Id.ToUpper())
                {
                    model.ddlTorneos = db.getTorneosByTeam(user.Id).Where(l => l.ligId == model.ligId)
                                             .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                             .ToList();
                }
                else
                {
                    model.ddlTorneos = db.getTorneosByLiga(model.ligId)
                                             .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                             .ToList();

                }

                model.ddlTorneos.Add(new SelectListItem { Text = "-- Todas --", Value = "0" });

                if (model.ddlTorneos.Count > 0)
                {
                    if (torId > 0)
                    {
                        model.torId = (int)torId;
                        var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == model.torId.ToString());
                        if (torneoSeleccionada != null)
                            torneoSeleccionada.Selected = true;
                        else
                            model.torId = int.Parse(model.ddlTorneos.First().Value);
                        model.torTipo = db.getTorneoById((int)torId).torTipo;

                    }else
                    {
                        model.torId = 0;
                    }
                  
                }
                else
                {
                    model.torId = 0;
                }
                // model.ddlTorneos = ddlTorneo;
            }
            return model;
        }
        public ActionResult Calendario_ChangeLiga(int? ligId=null,int? torId=null)
        {
            
            var user = db.getUserById(User.Identity.GetUserId());

            var partido = db.getPartidos(user);

            var model = filtros_Calendario(user,ligId,torId);

            return PartialView("Ligas/_CalendarioNuevo_Filtros", model);
        }
        public ActionResult Arbitro()
        {
            string codigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
            return View();
        }
        public PartidosViewModel _Calendario_Event(ApplicationUser user)
        {
            PartidosViewModel model = new PartidosViewModel();

            model.ddlLigas = _EquipoNuevo_GetDropDownListLigas(user.Id, user.usuRolActual);
            if (model.ddlLigas != null && model.ddlLigas.Count > 0)
            {
                model.ligId = int.Parse(model.ddlLigas.First().Value);
                model.ddlTorneos = _EquipoNuevo_GetDropDownListTorneos(model.ligId, user.usuRolActual,user.Id);

            }
            return model;
        }
        public JsonResult GetEvents(int? ligId = null, int? torId = null)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            if (torId == 0)
                torId = null;
            List<PartidosViewModel> ApptListForDate = new List<PartidosViewModel>();
            if (torId != null && ligId != null)
            {
                var tor = db.getTorneosByLiga((int)ligId);
                if(tor.Count <= 0)
                    torId = tor.FirstOrDefault().torId;
            }

            if (ligId != null && torId != null)
                ApptListForDate = db.getEvents(user,(int)ligId,(int)torId);
            else if(ligId != null && ligId > 0)
                ApptListForDate = db.getEvents(user, (int)ligId);
            else
            {
                var usrRoles = db.set_getUserCurrentRole(user.UserName);
                var rolActual = (string)usrRoles["currentUsrRoleName"];
                if (rolActual.ToUpper() == constClass.rolCoach.ToUpper())
                    ApptListForDate = db.getPartidosEquipos(user);
                else
                    ApptListForDate = db.getEvents(user);
                
            }
            var eventList = from e in ApptListForDate
                            select new
                            {
                                id = e.parId,
                                ligId = e.ligId,
                                liga = e.ligNombre,
                                title = e.equNombreEquipoUno + " vs " + e.equNombreEquipoDos,
                                torneo = e.torNombre,
                                equIdUno = e.equIdUno,
                                imgUno = e.imgUno,
                                imgDos = e.imgDos,
                                equIdDos = e.equIdDos,
                                equNombreUno = e.equNombreEquipoUno,
                                equNombreDos = e.equNombreEquipoDos,
                                canId = e.canId,
                                canNombre = e.canNombre,
                                start = e.parFecha_Inicio.ToString("s"),
                                end = e.parFecha_Fin.ToString("s"),
                                allDay = false,
                                color = (e.parEstatus != false) ? e.colDeporte : "#c0392b",
                                Estatus = (e.parEstatus == false) ? "Cancelado" : ""
                            };
            
            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);                      
        }
       /// <summary>
        /// Regresa la vista de la pantalla de pagos
        /// </summary>
        /// <returns></returns>
        public ActionResult Pagos()
        {
            return View();
        }
        public ActionResult _PagosGrid()
        {
            return PartialView("Banwire/PagosGrid");
        }
        public ActionResult _PagosGrid_Callback()
        {
            List<PagosGridViewModel> model = new List<PagosGridViewModel>();
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            var roles = db.getRoleByName(constClass.rolAdmin);

            if (user.usuRolActual.ToUpper() == roles.Id.ToUpper())                
                model = db.getGridPagos(user, true);                
            else
                model = db.getGridPagos(user).Where(l=> l.userId == user.Id).ToList();
            
            return PartialView("Banwire/PagosGrid", model);
        }
        public JsonResult _RealizarPago(int ligId)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            
            var datosUsuario = db.getUserProfile(user);

            var liga = db.getLigaById(ligId);

            var cust = from e in datosUsuario
                       select new
                       {
                            ligId = liga.ligId,
                            ligNombre = liga.ligNombreLiga,
                            userId= user.Id,
                            fname = e.fname,
                            mname = e.mname,
                            email = user.Email,
                            phone = e.phone,
                            addr = e.addr,
                            city = e.city,
                            state = e.state,
                            country = e.country,
                            zip = e.zip,
                            total = liga.ligTotalPagar,
                        };
                
            var rows = cust.ToArray();           
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _DetallesPago(int ligId)
        {
            var pagos = db.getDetallesPagos(ligId);
            var rows = pagos.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
       
        /// <summary>
        /// Regresa la vista de la pantalla de arbitros
        /// </summary>
        /// <returns></returns>
        public ActionResult Arbitros()
        {
            return View();
        }
        public ActionResult _Arbitro_RefreshFiltro(int ligId,string arbCorreo,string arbNombre)
        {
            PartidosViewModel model = new PartidosViewModel();
            var arbLiga = db.getArbitroLigId(arbCorreo, ligId).FirstOrDefault();

            var arbitro = db.getArbitroByEmail(arbCorreo).Where(l=> l.arbId == arbLiga.arbId);
            model.ddlArbitros = arbitro.Select(t => new SelectListItem { Text = t.arbNombre.ToUpper(), Value = t.arbId.ToString() }).ToList();

            return PartialView("Ligas/_ArbitroFiltro",model);
        }
        public ActionResult _Canchas_Refresh(int ligId)
        {
            var model = new PartidosViewModel();
            var user = db.getUserById(User.Identity.GetUserId());
            model.ddlCanchas = _Partidos_GetDropDownListCanchas(user.Id, user.usuRolActual, ligId);
            return PartialView("Ligas/_CanchasSelect",model);
        }
        [AllowAnonymous]
        public ActionResult _Arbitro_ShowModal()
        {
            return PartialView("Ligas/_ArbitrosModal"); 
        }
        public ActionResult _Arbitro_AgregarModal(int ligId,int torId)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var result = new AdminTorneosController()._Filtro_Liga_Torneo(ligId,torId,user,false);

            return result;
        }
        //[HttpPost]
        //public ActionResult Pago_Result(string status, string auth_code, string reference, string id,string hash,decimal total)
        public JsonResult Pago_Result(PagosViewModel pagos)
        {
            var lig = db.getLigaById(pagos.conceptoId);
            conekta.Api.version = "1.0.0";
            conekta.Api.apiKey = "key_VF6qS6PFEWEEqQeq6pMHxA";
            var total = (pagos.total*100);
            var nombre = pagos.nombre;
            var tel = pagos.tel;
            var email = pagos.email;
            var item = pagos.item;
            var descripcion = "Pago de "+pagos.concepto;
            var calle =pagos.street;
            var ciudad =pagos.ciudad;
            var pais = pagos.pais;
            var cp =pagos.cp;
            var reference = "9839_Enligate";
            
            Newtonsoft.Json.Linq.JObject data =
                new Newtonsoft.Json.Linq.JObject(
                new Newtonsoft.Json.Linq.JProperty("description", "Enligate"),
                new Newtonsoft.Json.Linq.JProperty("amount", total),
                new Newtonsoft.Json.Linq.JProperty("currency", "MXN"),
                new Newtonsoft.Json.Linq.JProperty("reference_id", reference),
                new Newtonsoft.Json.Linq.JProperty("card", pagos.token),
                //new Newtonsoft.Json.Linq.JProperty("cash",
                //    new Newtonsoft.Json.Linq.JObject(
                //        new Newtonsoft.Json.Linq.JProperty("type", "oxxo")
                //    )
                //),
                new Newtonsoft.Json.Linq.JProperty("details",
                    new Newtonsoft.Json.Linq.JObject(
                        new Newtonsoft.Json.Linq.JProperty("name", nombre),
                        new Newtonsoft.Json.Linq.JProperty("phone", tel),
                        new Newtonsoft.Json.Linq.JProperty("email", email),
                        new Newtonsoft.Json.Linq.JProperty("customer",
                            new Newtonsoft.Json.Linq.JObject(
                                new Newtonsoft.Json.Linq.JProperty("logged_in", true),
                                new Newtonsoft.Json.Linq.JProperty("successful_purchases", 14),
                                new Newtonsoft.Json.Linq.JProperty("created_at", db.DateTimeMX()),
                                new Newtonsoft.Json.Linq.JProperty("updated_at", db.DateTimeMX()),
                                new Newtonsoft.Json.Linq.JProperty("offline_payments", 4),
                                new Newtonsoft.Json.Linq.JProperty("score", 9)
                            )
                       ),
                       new Newtonsoft.Json.Linq.JProperty("line_items",
                            new Newtonsoft.Json.Linq.JArray(
                                new Newtonsoft.Json.Linq.JObject(
                                    new Newtonsoft.Json.Linq.JProperty("name", item),
                                    new Newtonsoft.Json.Linq.JProperty("description", descripcion),
                                    new Newtonsoft.Json.Linq.JProperty("unit_price", total),
                                    new Newtonsoft.Json.Linq.JProperty("quantity", 1),
                                    new Newtonsoft.Json.Linq.JProperty("sku", "ligatest1qaz"),
                                    new Newtonsoft.Json.Linq.JProperty("category", "Liga")
                               )
                            )
                       ),
                       new Newtonsoft.Json.Linq.JProperty("billing_address",
                            new Newtonsoft.Json.Linq.JObject(
                                new Newtonsoft.Json.Linq.JProperty("street1", calle),
                                new Newtonsoft.Json.Linq.JProperty("street2", null),
                                new Newtonsoft.Json.Linq.JProperty("street3", null),
                                new Newtonsoft.Json.Linq.JProperty("city", ciudad),
                                new Newtonsoft.Json.Linq.JProperty("zip", cp),
                                new Newtonsoft.Json.Linq.JProperty("country", pais),
                                new Newtonsoft.Json.Linq.JProperty("tax_id", null),
                                new Newtonsoft.Json.Linq.JProperty("company_name", null),
                                new Newtonsoft.Json.Linq.JProperty("phone", null),
                                new Newtonsoft.Json.Linq.JProperty("email", null)
                          )
                     )

                  )
               )
            );
            var cargo = JsonConvert.SerializeObject(data);
            conekta.Charge charge = new conekta.Charge().create(cargo);
            var status = charge.status;
            //var id = charge.reference_id;
            var id = charge.id;
            var method = charge.payment_method;
            var ip = Request.UserHostAddress;
            var IdPago = 0;
            if (pagos.concepto == "Liga")
            {
                IdPago = db.setPago(pagos.userId, pagos.conceptoId, id,pagos.concepto);
                if (IdPago > 0)
                {
                    DetallePagoViewModel detPago = new DetallePagoViewModel();
                    detPago.IdPago = IdPago;
                    detPago.conceptoPago = descripcion;
                    detPago.total = pagos.total;
                    detPago.IdTransaccion = id;
                    detPago.referencia = reference;
                    detPago.metodoPago = method.type;
                    detPago.status = status;
                    detPago.ipAddress = ip;

                    var idDetPag = db.setDetallePago(detPago);

                    if (idDetPag > 0)
                    {
                        var setLiga = db.setLigaPago(pagos.conceptoId, status);
                        if(setLiga)
                            return Json(pagos.conceptoId);
                        else
                            return Json("Error al confirmar pago de equipo");
                      
                    }

                }
            }else if(pagos.concepto == "Torneo")
            {
                var user = db.getUserByUserEmail(pagos.email);
                IdPago = db.setPago(user.Id, pagos.conceptoId, id, pagos.concepto);
                if (IdPago > 0)
                {
                    DetallePagoViewModel detPago = new DetallePagoViewModel();
                    detPago.IdPago = IdPago;
                    detPago.conceptoPago = descripcion;
                    detPago.total = pagos.total;
                    detPago.IdTransaccion = id;
                    detPago.referencia = reference;
                    detPago.metodoPago = method.type;
                    detPago.status = status;
                    detPago.ipAddress = ip;

                    var idDetPag = db.setDetallePago(detPago);

                    if (idDetPag > 0)
                    {
                        var setTorneo = db.setTorneoPago(pagos.conceptoId, status);
                        if (setTorneo)
                            return Json(pagos.conceptoId);
                        else
                            return Json("Error al confirmar pago de torneo");

                    }

                }
            }
            else if (pagos.concepto == "Equipo" || pagos.concepto == "Jugador")
            {
                if (status=="paid")
                {
                    var userIdPlayer = db.getUserByUserEmail(email);
                    var jsonData = new
                    {
                        userId = userIdPlayer.Id,
                        transaccionId  = id,
                        concepto = pagos.concepto,
                        conceptoId=pagos.conceptoId,
                        descripcion = descripcion,
                        metodo = method.type,
                        total = pagos.total,
                        status = status,                        
                        referencia  = reference,
                        ip = ip
                    };
                    return Json(jsonData);                  
                }               
            }           

            return Json(cargo);
            //return RedirectToAction("Pagos");
        }
        [HttpPost]
        public JsonResult webhook()
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;            
            string filename = Server.MapPath("~/_Logs/logfile4webhook.txt");         
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var jstring = serializer.Serialize(json);
            try
            {
                dynamic jsonObject = serializer.Deserialize<dynamic>(json);
                var a = jsonObject["object"];
                var b = a["id"];
                var c = a["payment_method"];
                var d = c["barcode"];
                var e = c["barcode_url"];
                ostrm = new FileStream(filename, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
                Console.SetOut(writer);
                Console.Write(db.DateTimeMX() + " ");
                Console.WriteLine(jstring);
                Console.SetOut(oldOut);
                writer.Close();
                ostrm.Close();
                return Json(HttpStatusCode.OK, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                // Try and handle malformed POST body
                ostrm = new FileStream(filename, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
                Console.SetOut(writer);
                Console.Write(db.DateTimeMX() + " ");
                Console.WriteLine(ex.Message);
                Console.SetOut(oldOut);
                writer.Close();
                ostrm.Close();
                return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
            }

            
        }
        public JsonResult _PaymentNotify()
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            var model = db.getGridPagos(user);
            model = model.Where(l => l.estado == "Pendiente").ToList();
            //.Where(l=> l.userId ==user.Id && l.estado == "Pendiente");

            var path = "";
            var rolActual = user.usuRolActual.ToUpper();
            var rolLiga = db.getRoleByName(constClass.rolOwners).Id.ToUpper();
            var rolTorneo = db.getRoleByName(constClass.rolAdminTorneos).Id.ToUpper();
            var rolEquipo = db.getRoleByName(constClass.rolCoach).Id.ToUpper();
            var rolJugador = db.getRoleByName(constClass.rolPlayer).Id.ToUpper();

            if (rolTorneo == rolActual)
            {
                path = "/AdminTorneos/Pagos";
            }
            else if (rolEquipo == rolActual)
            {
                path = "/AdminEquipos/MisPagos";
            }
            else if (rolJugador == rolActual)
            {
                path = "/Admin/MisPagos";
            }
            else if (rolLiga == rolActual)
            {
                path = "/AdminLigas/Pagos";
            }

            var model2 = model.Select(s => new {
                conceptoNombre = s.conceptoNombre,
                total = s.total,
                path = path
            });
            var rows = model2.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult _NotificationGridCallback()
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            var model = db.getLeagueGridStatus().Where(l=> l.ligCreadorId.ToUpper() == user.Id.ToUpper()).ToList();

            return PartialView("Ligas/_NotificationGrid", model);
        }

        public JsonResult _SetNotificationLiga(int ligId)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            if (ligId > 0)
            {
                if (db.setNotificationLeague(ligId))
                {
                    return Json("success");
                }
                //return Json("wrong",JsonRequestBehavior.AllowGet );
                return Json("wrong");
            }
            return Json("wrong");
        }

        public JsonResult _ChartPieLeagueDefualt()
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            var liga = db.getMainLeague(user.Id,user.usuRolActual);
            if (liga!= null) {
                var torneos = db.getTorneosByLiga(liga.ligLiga.ligId).ToList();
                var ids = torneos.Select(s => new { torId = s.torId,nombre=s.torNombreTorneo });
                List<int> numPartidos = new List<int>();
                List<string> torneoNombre = new List<string>();
                foreach (var item in ids)
                {
                    numPartidos.Add(db.getPartidosByTorneoId(item.torId).Count());
                    torneoNombre.Add(item.nombre);
                }
                var nombre = torneos.Select(s => new { nombre = s.torNombreTorneo }).ToList();
            
                var jsonData = new
                {
                    torneos = torneoNombre,
                    partidos = numPartidos,
                    liga=liga.ligLiga.ligNombre
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            return Json("wrong");
        }

        public ActionResult _SendReportBug(string reporteTxt)
        {
           
            var report = new schemaReporteBugs();
            var resultJson = new JsonResultViewModel();
            var usuario = db.getUserById(User.Identity.GetUserId());
            var browser = Request.Browser;

            report.correoUsuario = usuario.Email;
            report.reporte = reporteTxt.Replace("\""," ").Trim();
            report.ipAddress = Request.UserHostAddress;
            report.browser = "Browser: " + browser.Type + " Name: " + browser.Browser + " Version: " + browser.Version;
                        
            if (db.setReportBug(report))
            {
                ModelState.AddModelError(constClass.success, "Se ha enviado el Reporte.");

                Global_Functions.saveReport(report.correoUsuario+" "+report.ipAddress+" "+report.reporte+" "+report.browser);
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Ha ocurrido en error, intentelo mas tarde.");
            }

            resultJson.strErrMessagePartialViewString = RenderPartialViewToString("_ModalState_Errors");
            return Json(resultJson);
        }

        public ActionResult Estadisticas()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult _PayTeam(string userId, string status, string total, string conceptoId, string referencia, string ip, string metodo, string descripcion, string concepto, string transaccionId)
        {
            var IdPago = db.setPago(userId, Convert.ToInt16(conceptoId), transaccionId, concepto);
            if (IdPago > 0)
            {
                DetallePagoViewModel detPago = new DetallePagoViewModel();
                detPago.IdPago = IdPago;
                detPago.conceptoPago = descripcion;
                detPago.total = Convert.ToDecimal(total);
                detPago.IdTransaccion = transaccionId;
                detPago.referencia = referencia;
                detPago.metodoPago = metodo;
                detPago.status = status;
                detPago.ipAddress = ip;
                detPago.conceptoPago = "Pago de " + concepto;
                var idDetPag = db.setDetallePago(detPago);

                if (idDetPag > 0)
                {
                    if (concepto == "Equipo")
                    {
                        var equId = Convert.ToInt16(conceptoId);
                        var setEquipo = db.setEquipoPago(equId, userId, true);
                        if (setEquipo)
                            return Json(equId);
                        else
                            return Json("Error al confirmar pago de equipo");

                    }
                    else if (concepto == "Jugador")
                    {
                        var tor = db.getEquipoById(Convert.ToInt16(conceptoId));
                        var user = db.getUserById(userId);
                        var conf = db.setJugadoresEquipo_ConfirmarParticipacion(user.Email, tor.torId, Convert.ToInt16(conceptoId), userId,"");
                        if (conf)
                        {
                            var userRole = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolPlayer.ToUpper());
                            if (userRole == null)
                            {
                                var rolId = db.getRoleByName(constClass.rolPlayer).Id;
                                UserManager.AddToRole(user.Id, constClass.rolPlayer);
                                db.setCurrentUserRole(user.Id, rolId);
                            }
                            return Json(conceptoId);
                        }
                        else
                        {
                            return Json("Error al confirmar pago de Jugador");
                        }
                    }
                    return Json("Success");
                }
            }

            return Json("Error");
        }
        [AllowAnonymous]
        public ActionResult _AddPerfil(string correo, string nombre)
        {
            var resultJson = new JsonResultViewModel();
            var name = ""; var last = 0;
            var sequence = new List<int>();

            var val = true; var hasCorreo = false; var enligateEmail = false;
                            
            var user = db.getUserById(User.Identity.GetUserId());

            if (nombre != "")
            {
                for (int i = 0; i < nombre.Length; i++)
                {
                    if (!Char.IsDigit(nombre[i]))
                        name += nombre[i];
                }

                var counts = db.getUserCountLikeName(nombre);

                if (counts.Count > 0)
                {
                    var exist = counts.Where(l => l.Email.Contains(nombre));

                    if (exist.Count() > 0)
                    {
                        correo = exist.First().Email;
                    }
                    else
                    {
                        if (counts.Count > 0)
                        {
                            correo = "";
                            foreach (var item in counts)
                            {
                                string b = string.Empty;
                                string c = string.Empty;


                                for (int i = 0; i < item.UserName.Length; i++)
                                {
                                    if (Char.IsDigit(item.UserName[i]))
                                        b += item.UserName[i];
                                }

                                if (b.Length > 0)
                                    sequence.Add(int.Parse(b));
                            }
                            sequence = sequence.OrderBy(i => i).ToList();
                            last = sequence.Last();
                            last = last + 1;
                        }
                    }
                }
            }
            else
            {
                nombre = correo;
                name = nombre;
            }
            
            if (correo == "")
            {
                correo = name.Replace(" ", "");

                if (last!=0)
                    correo = correo + last + "@enligate.com";
                else
                    correo = correo + "@enligate.com";

                correo = RemoveAccentsWithRegEx(correo);
            }

            var juUser = db.getUserByUserEmail(correo);


            var usuarioId = "";
            if (juUser != null)
            {
                if ( juUser.usuEstatus != true  )
                {
                    ModelState.AddModelError(constClass.info, "El usuario esta deshabilitado.");
                    val = false;
                    resultJson.booSuccess = false;
                }else
                {
                    if (juUser.Id.ToUpper() == user.Id.ToUpper())
                    {
                        usuarioId = user.Id;
                    }
                    else
                    {
                        usuarioId = juUser.Id;
                    }
                }
                
            }
            else
            {
                if (correo != "")
                {
                    if (juUser == null)
                    {
                        ApplicationUser usuario = new ApplicationUser
                        {
                            UserName = name,
                            Email = correo,
                            PhoneNumber = "",
                            EmailConfirmed = false,
                            usuEstatus = true
                        };

                        var usuPassword = db.generator_Pass();
                        var modelRegister = new RegisterViewModel();
                        modelRegister.usuPassword = usuPassword;
                        modelRegister.usuEmail = correo;
                        var newUser = new AccountController()._RegisterNewAccount(modelRegister, "");
                        if (newUser)
                        {
                            ApplicationUser userId = db.getUserByUserEmail(correo);
                            usuarioId = userId.Id;
                            if (userId.usuRolActual == null)
                            {
                                var rolId = db.getRoleByName(constClass.rolPlayer).Id;
                                db.setCurrentUserRole(userId.Id, rolId);
                            }
                            var profile = db.getUserMainProfile(userId.Id);
                            if (profile == null)
                            {
                                var prof = new schemaUsersProfiles();
                                prof.uprNombres = (userId.UserName != null) ? userId.UserName : "-";
                                prof.uprApellidos = "-";
                                prof.uprTelefono = "";
                                db.setUserProfileMain_UpdateInsert(userId, prof);
                            }
                            enligateEmail = true;
                        }
                        else
                        {
                            usuarioId = null;
                        }
                        //usuarioId = RegisterPlayer(usuario, usuPassword, constClass.rolPlayer);
                        enviarEmailParticipanteAviso(user.Email, correo, usuPassword);
                        // return Json(resultJson);
                    }
                }
                else
                {
                    hasCorreo = true;
                }

            }
            
            if (usuarioId!=null)
            {
                if (hasCorreo)
                {
                    var msg = "El correo no es valido";
                    ModelState.AddModelError(constClass.error, msg);
                    resultJson.booSuccess = false;
                }
                else if (val)
                {
                    if (enligateEmail)
                    {
                        if (db.setNewSubPerfil(user.Id, usuarioId))
                        {
                            var msg = "Se le ha enviado un correo a " + correo + " con los datos necesarios para entrar al portal de Enligate.";
                            ModelState.AddModelError(constClass.success, msg);
                            resultJson.booSuccess = true;
                        }
                    }
                    else
                    {
                        enviarEmailParticipanteAvisoAgregar(user.Email, correo,name);
                        var msg = "El usuario " + correo + " ya tiene una cuenta propia, se le ha enviado un correo de invitación.";
                        ModelState.AddModelError(constClass.info, msg);
                        resultJson.booSuccess = true;
                    }
                }
                               
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Error al Registrar Jugador.");
                resultJson.booSuccess = false;
            }

            resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");

            return Json(resultJson);
        }

        public string setScapeNone(string data)
        {
            return data.Replace(" ", "");
        }
        [NonAction]
        public bool enviarEmailParticipanteAviso(string admin,string correo, string password)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var valCorreo = correo.ToUpper().EndsWith("@ENLIGATE.COM");
                string body = "";
                string emailTo = "";

                if (!valCorreo)
                {
                    body = Global_Functions.getBodyHTML("~/Emails/NuevoSubPerfil.html");
                    emailTo = correo;
                }
                else
                {
                    body = Global_Functions.getBodyHTML("~/Emails/NuevoSubPerfilEnligate.html");
                    emailTo = admin;
                }


                body = body.Replace("<%= NombreAdmin %>", admin);
                body = body.Replace("<%= usuario %>", correo);
                body = body.Replace("<%= password %>", password);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Confirmacion de Cuenta", body,
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
        [NonAction]
        public bool enviarEmailParticipanteAvisoAgregar(string admin, string correo, string name)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = correo;

                string body = Global_Functions.getBodyHTML("~/Emails/AgregarSubPerfil.html");

                var confirmarUrl = Url.Action("SubPerfilConfirmar", "Account", new { email_admin = admin, email_sub = correo , name = name }, protocol: Request.Url.Scheme);
                var rechazarUrl = Url.Action("SubPerfilRechazar", "Account", new { email_admin = admin, email_sub = correo }, protocol: Request.Url.Scheme);

                body = body.Replace("<%= NombreAdmin %>", admin);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);
                body = body.Replace("<%= UrlValidationCode %>", confirmarUrl);
                body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Confirmacion de Cuenta", body,
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
        public String RegisterPlayer(ApplicationUser user, string usuPassword, string role)
        {

            var result = UserManager.Create(user, usuPassword);
            if (result != IdentityResult.Success)
            {
                ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
            }
            else
            {
                var prof = new schemaUsersProfiles();
                prof.uprNombres = (user.UserName!=null)? user.UserName:"-";
                prof.uprApellidos = "-";
                prof.uprTelefono = "";
                db.setUserProfileMain_UpdateInsert(user, prof);

                result = UserManager.AddToRole(user.Id, role);

                if (result != IdentityResult.Success)
                {
                    UserManager.Delete(user);
                    ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                    Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
                }
                else
                {
                    var rol = db.getRoleByName(role);
                    db.setCurrentUserRole(user.Id, rol.Id);
                    db.setClearEmailValidation(user);
                    //SignInManager.SignIn(user, false, false);                    
                }
            }
            return user.Id;
        }
        public static string RemoveAccentsWithRegEx(string inputString)
        {
            Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            inputString = replace_a_Accents.Replace(inputString, "a");
            inputString = replace_e_Accents.Replace(inputString, "e");
            inputString = replace_i_Accents.Replace(inputString, "i");
            inputString = replace_o_Accents.Replace(inputString, "o"); 
            inputString = replace_u_Accents.Replace(inputString, "u");
            return inputString;
        }
        public ActionResult VerifySuscribeLeague(string ligName, int? ligId=null)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booSuccess = true;

            var ligas = new List<schemaLigas>();
            if (ligId != null)
                ligas = db.getLigasByName(ligName).Where(l=> l.ligId != ligId ).ToList();
            else
                ligas = db.getLigasByName(ligName);

            if (ligas.Any())
            {
                if (ligas.Count >= 1)
                {
                    ModelState.AddModelError(constClass.info, "Lo sentimos, El nombre de Liga '" + ligName + "' ya esta registrado !");
                    resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                    resultJson.booSuccess = false;
                }
            }

            return Json(resultJson);
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }

}