using Newtonsoft.Json;
using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models;
using sw_EnligateWeb.Models.HelperClasses;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Security;

namespace sw_EnligateWeb.Controllers
{
    public class RefereeController : Controller
    {
        DatabaseFunctions db = new DatabaseFunctions();
        // GET: Referee
        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public RefereeController()
        {
        }

        public RefereeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        public ActionResult Index()
        {
            return View();
        }

        #region Perfil

        /// <summary>
        /// Regresa el perfil del usuario obtenido por el nombre de usuario
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [NonAction]
        public ProfileViewModel getPerfilUsuario(string username)
        {
            var usr = db.getUserByUserName(username);
            var prof = db.getUserMainProfile(usr.Id);
            if (prof == null)
                prof = new schemaUsersProfiles();

            ProfileViewModel model = new ProfileViewModel();
            model._profile.usuUsername = usr.UserName;
            model._profile.imgURL = prof.uprProfileImageURL;
            model._profile.usuNombreCompleto = (((prof.uprNombres == "-") ? "" : prof.uprNombres.Trim()) + " " + ((prof.uprApellidos == "-") ? "" : prof.uprApellidos.Trim())).Trim();
            model._profile.usuGenero = (prof.uprGenero != null) ? prof.uprGenero : "";
            model._profile.usuFechaNacimiento = (prof.uprFechaNacimiento != null) ? ((DateTime)prof.uprFechaNacimiento).ToString(constClass.formatDateOnly) : "";
            model._profile.usuPais = (prof.uprPais != null) ? prof.uprPais.Trim() : "";
            model._profile.usuEstado = (prof.uprEstado != null) ? prof.uprEstado.Trim() : "";
            model._profile.usuCiudad = (prof.uprCiudad != null) ? prof.uprCiudad.Trim() : "";
            model._profile.usuCP = (prof.cp > 0) ? prof.cp : 0;
            model._profile.usuTelefono = (prof.uprTelefono != null) ? prof.uprTelefono.Trim() : "";
            model._profile.usuCorreo = (usr.Email != null) ? usr.Email.Trim() : "";

            return model;
        }

        /// <summary>
        /// Realiza la carga de la página del perfil
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Perfil()
        {
            ProfileViewModel model = getPerfilUsuario(User.Identity.Name);

            return View("Perfil", model);
        }

        [HttpGet]
        public ActionResult Calendario()
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var model = FilterCalendar(user);
            return View(model);
        }
        [HttpPost]
        public ActionResult Calendario_Change(int? ligId = null, int? torId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());
            var model = FilterCalendar(user, ligId, torId);

            return PartialView("Ligas/_CalendarioNuevo_Filtros", model);
        }
        public PartidosViewModel FilterCalendar(ApplicationUser user,int? ligId=null,int? torId=null) {

            var partido = db.getPartidosRefeere(user);

            PartidosViewModel model = new PartidosViewModel();
            var ddlLiga = new List<SelectListItem>();
            var ddlTorneo = new List<SelectListItem>();

            foreach (var item in partido)
            {
                var liga = db.getLigaById(item.ligId);


                if (ddlLiga.Count > 0)
                {
                    if (liga.ligId.ToString() != ddlLiga.Last().Value)
                    {
                        ddlLiga.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                    }
                }
                else
                    ddlLiga.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });

            }
            model.ddlLigas = ddlLiga;
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
                {
                    model.ligId = (int)ligId;
                }
                else
                {
                    model.ligId = int.Parse(model.ddlLigas.First().Value);
                }

                foreach (var item in partido)
                {
                    var torneo = db.getTorneoById(item.torId);
                    if (torneo.ligId == model.ligId)
                    {
                        if (ddlTorneo.Count > 0)
                        {
                            if (torneo.torId.ToString() != ddlTorneo.Last().Value)
                            {
                                ddlTorneo.Add(new SelectListItem { Text = torneo.torNombreTorneo.ToUpper(), Value = torneo.torId.ToString() });
                            }
                        }
                        else
                            ddlTorneo.Add(new SelectListItem { Text = torneo.torNombreTorneo.ToUpper(), Value = torneo.torId.ToString() });
                    }
                    
                }
                model.ddlTorneos = ddlTorneo;
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

                }
                else
                {
                    model.torId = 0;
                }

            }
            else
            {
                model.torId = 0;
            }

            model.numPartidos = partido.Count;

            return model;
        }
        [HttpGet]
        public ActionResult Ligas()
        {
            return View();
        }
        public ActionResult Torneos()
        {
            return View();
        }
        /// <summary>
        /// Actualiza los datos del perfil
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _Profile(UserProfileViewModel model)
        {
            bool error_ReturnData = false;
            var usr = db.getUserByUserName(model.usuUsername);
            if (ModelState.IsValid)
            {
                var profile = new schemaUsersProfiles();
                var nombreCompleto = Global_Functions.getName_LastName(model.usuNombreCompleto);
                profile.uprNombres = nombreCompleto["name"];
                profile.uprApellidos = nombreCompleto["lastname"];
                profile.uprGenero = (model.usuGenero != null) ? model.usuGenero : null;
                profile.uprFechaNacimiento = Global_Functions.stringToDate(model.usuFechaNacimiento);
                profile.uprPais = (model.usuPais != null) ? model.usuPais.Trim() : null;
                profile.uprEstado = (model.usuEstado != null) ? model.usuEstado.Trim() : null;
                profile.uprCiudad = (model.usuCiudad != null) ? model.usuCiudad.Trim() : null;
                profile.cp = (model.usuCP > 0) ? model.usuCP : 0;
                profile.uprTelefono = (model.usuTelefono != null) ? model.usuTelefono.Trim() : null;
                usr.Email = (model.usuCorreo != null) ? model.usuCorreo.Trim() : null;

                if (db.setUserProfileMain_UpdateInsert(usr, profile))
                {
                    ModelState.AddModelError(constClass.success, "Guardado.");
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la información.");
                    error_ReturnData = true;
                }
            }
            else
            {
                error_ReturnData = true;
            }

            if (error_ReturnData)
            {
                usr = db.getUserByUserName(model.usuUsername);
                var prof = db.getUserMainProfile(usr.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();

                model.usuNombreCompleto = ((prof.uprNombres == "-") ? "" : prof.uprNombres.Trim()) + " " + ((prof.uprApellidos == "-") ? "" : prof.uprApellidos.Trim());
                model.usuGenero = (prof.uprGenero != null) ? prof.uprGenero : "";
                model.usuFechaNacimiento = (prof.uprFechaNacimiento != null) ? ((DateTime)prof.uprFechaNacimiento).ToString(constClass.formatDateOnly) : "";
                model.usuCiudad = (prof.uprCiudad != null) ? prof.uprCiudad.Trim() : "";
                model.usuTelefono = (prof.uprTelefono != null) ? prof.uprTelefono.Trim() : "";
                model.usuCorreo = (usr.Email != null) ? usr.Email.Trim() : "";

                ModelState.AddModelError(constClass.error, "Un dato no es válido.");
            }

            model.modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

            JsonReturn_ErrorsViewModel resultModel = new JsonReturn_ErrorsViewModel();
            resultModel.jsScript = @"<script language='javascript' type='text/javascript'>
                                        " + model.jsGetModelFunctionName + "('" + JsonConvert.SerializeObject(model) + @"');
                                     </script>";

            return PartialView("_JsonReturn_Errors", resultModel);
        }

        /// <summary>
        /// Actualiza la imagen del perfil
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ProfileImage(UserProfileViewModel model)
        {
            string urlPath = Server.MapPath(constClass.urlProfileImages);
            bool fileSaved = false;
            string filename = "";
            var usr = db.getUserByUserName(User.Identity.Name);
            var prof = db.getUserMainProfile(usr.Id);
            if (prof != null)
                model.imgURL = prof.uprProfileImageURL;
            else
                prof = new schemaUsersProfiles();

            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    var extension = Path.GetExtension(fileContent.FileName);
                    if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                    {
                        filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                        fileContent.SaveAs(filename);

                        fileSaved = System.IO.File.Exists(filename);
                        break;
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                    }
                }
            }

            if (fileSaved)
            {
                prof.uprProfileImageURL = filename;
                if (db.setUserProfileMain_UpdateInsert(usr, prof))
                {
                    if (model.imgURL != null && model.imgURL != "" && model.imgURL != filename)
                        Global_Functions.deleteFiles(model.imgURL, true);
                    ModelState.AddModelError(constClass.success, "Guardado.");
                    model.imgURL = filename;
                }
                else
                {
                    Global_Functions.deleteFiles(filename, true);
                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen.");
            }

            if (model.imgURL != null)
                model.imgURL = "/" + model.imgURL.Replace(Server.MapPath("~"), "").Replace("\\", "/");

            model.modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

            JsonReturn_ErrorsViewModel resultModel = new JsonReturn_ErrorsViewModel();
            resultModel.jsScript = @"<script language='javascript' type='text/javascript'>
                                        " + model.jsGetModelFunctionName + "('" + JsonConvert.SerializeObject(model) + @"');
                                     </script>";

            return PartialView("_JsonReturn_Errors", resultModel);
        }

        #endregion
        
        public ActionResult Historial()
        {
            return View();
        }


        [HttpPost]
        public ActionResult changeEquipo(int torId, int equId,int Id)
        {
            ResultadoPartido model = new ResultadoPartido();
            model.ddlJugUno = db.getJugadoresByTorneo_Equipo(torId, equId).Where(l => l.jugConfirmado == true).ToList();
            model.Id = Id;
            return PartialView("Referee/_JugadorSelect",model);
        }
     /*   public ActionResult _GetAllData(int parId)
        {
            List<string> list = new List<string>();
            ResultadoPartido model = new ResultadoPartido();
            var data = db.getPartidosById(parId);
            db.getEstadisticaFutbolByPartido
            model.Id = Id;
            //list.Add(RenderPartialViewToString("Referee/_rowDeatilResult", model));
            return null;
        }*/
        public ActionResult addFormDataPlayer(int Id,int parId, List<String> jugIds)
        {
            ResultadoPartido model = new ResultadoPartido();
            var data = db.getPartidosById(parId);
            model.equNombreEquipoUno = data.equNombreEquipoUno;
            model.equNombreEquipoDos = data.equNombreEquipoDos;
            model.equIdUno = data.equIdUno;
            model.equIdDos = data.equIdDos;

            var JugUno = new List<schemaJugadorEquipos>();
            var JugDos = new List<schemaJugadorEquipos>();

            JugUno = db.getJugadoresByTorneo_Equipo(data.torId, model.equIdUno).Where(l => l.jugConfirmado == true).ToList();
            JugDos = db.getJugadoresByTorneo_Equipo(data.torId, model.equIdDos).Where(l => l.jugConfirmado == true).ToList();

            if (jugIds != null)
            {
                var aux1 = new List<String>(); ; var aux2 = new List<String>();
                var aux3 = new List<schemaJugadorEquipos>(); var aux4 = new List<schemaJugadorEquipos>() ;
                foreach (var item in jugIds)
                {
                    foreach (var jug1 in JugUno)
                    {
                        if (jug1.jugUserId == item)
                            aux1.Add(item);
                    }
                    foreach (var jug2 in JugDos)
                    {
                        if (jug2.jugUserId == item)
                            aux2.Add(item);
                    }                    
                }
                
                if (aux1.Count > 0)
                {
                    foreach (var item in aux1)
                    {
                        JugUno.Remove(JugUno.Single(s => s.jugUserId == item));
                    }
                    aux3 = JugUno;
                }
                else
                {
                    aux3 = JugUno;
                }
                                
                if (aux2.Count >0)
                {
                    foreach (var item in aux2)
                    {
                        JugDos.Remove(JugDos.Single(s => s.jugUserId == item));
                    }
                    aux4 = JugDos;
                }
                else
                {
                    aux4 = JugDos;
                }
                JugUno = aux3;
                JugDos = aux4;
                if (JugUno.Count <= 0 )
                    model.equNombreEquipoUno = "";

                if (JugDos.Count <= 0)
                    model.equNombreEquipoDos = "";                
            }

            model.ddlJugUno = JugUno;
            model.ddlJugDos = JugDos;

            model.Id = Id+1;

            return PartialView("Referee/_EquipoJugadorSelect",model);
        }

        [HttpPost]
        public JsonResult updateDetailsEvent(PartidosViewModel partido,List<int> team,List<int> score, List<int> asis, List<int> faltas,List<int> ama, List<int> roja, List<int> susp, List<String> jugIds)
        {
            for (int i = 0; i < team.Count; i++)
            {
                var equ = team[i];
                var gol = score[i];
                var jug = jugIds[i];
                var asistencias = asis[i];
                var fal = faltas[i];
                var amarillas = ama[i];
                var roj = roja[i];
                var suspencion = susp[i];

                var futEstJug = db.setFutbolEstadisticasJugador(partido.parId,equ,gol, asistencias,fal,amarillas,roj,suspencion, jug);
            }

            if (db.setPartidos_UpdateScore(partido))
            {
                enviarEmailAvisoMarcadorPartido(partido.parId);
                return Json("Resultados Guardados con exito, Se han enviado avisos a los administradores");
            }
                
            return Json("Ocurrio un Error al guardar Resultados");
        }
        [NonAction]
        public bool enviarEmailAvisoMarcadorPartido(int parId)
        {
            var partido = db.getPartidosById(parId);

            var coadminLiga = db.getLeagueCoadministratorsById(partido.tblLigas.ligId, partido.tblLigas.ligUserIdCreador);
            var coadminTorneo = db.getTorneoCoadministradoresById(partido.tblTorneos.torId);

            var admLiga = partido.tblLigas.tblUserCreador.Email;
            var adminTorneo = partido.tblTorneos.tblUserCreador.Email;

            var adminEquipo1 = db.getEquipoById(partido.equIdUno);
            var adminEquipo2 = db.getEquipoById(partido.equIdDos);
            

            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/AvisoMarcadorPartido.html");

                body = body.Replace("<%= NombreEquipoUno %>", partido.equNombreEquipoUno);
                body = body.Replace("<%= NombreEquipoDos %>", partido.equNombreEquipoDos);
                body = body.Replace("<%= NombreTorneo %>", partido.tblTorneos.torNombreTorneo);
                body = body.Replace("<%= Fecha %>", partido.parFecha_Inicio.ToString());
                body = body.Replace("<%= MarcadorUno %>", partido.equResultadoUno.ToString());
                body = body.Replace("<%= MarcadorDos %>", partido.equResultadoDos.ToString());
                body = body.Replace("<%= imgEquipoUno %>", adminEquipo1.equImgUrl);
                body = body.Replace("<%= imgEquipoDos %>", (adminEquipo2!=null)? adminEquipo2.equImgUrl:"");
                
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                if (coadminLiga!=null && coadminLiga.Count >0)
                {
                    foreach (var item in coadminLiga)
                    {
                        Global_Functions.sendMail(item.lcaEmail, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                    }
                }
                if (coadminTorneo != null && coadminTorneo.Count > 0)
                {
                    foreach (var item in coadminTorneo)
                    {
                        Global_Functions.sendMail(item.tcaEmail, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                    }
                }

                Global_Functions.sendMail(adminTorneo, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");

                Global_Functions.sendMail(adminEquipo1.equAdminCorreo, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                if (adminEquipo2!=null)
                {
                    Global_Functions.sendMail(adminEquipo2.equAdminCorreo, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                }                

                bool mailSended = Global_Functions.sendMail(admLiga, siteConfig.scoSenderDisplayEmailName, "Aviso de partido", body,
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
        [AllowAnonymous]
        public ActionResult ArbitroPartidoConfirmar(string email, int ligId,int parId,string code)
        {
            var user = db.getUserByUserEmail(email);
            bool cerrarSesion = false;
            if (user == null)
                cerrarSesion = true;
            else
                if (user.Id != User.Identity.GetUserId())
                cerrarSesion = true;

            if (cerrarSesion)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                // TempData["ReturnUrl"] = Url.Action("ArbitroConfirmar", "Admin", new { email = email, torId = torId, code = code });
                // return RedirectToAction("Index", "Home");
            }

            var arbPar = db.getArbitroPartidoByParId(parId);
            if (arbPar != null)
            {
                var arbId = db.getArbitroById(arbPar.arbId);
                if (arbPar.arbCodigoConfirmacion == code)
                {
                    if (db.setConfirmarArbitroPartido(arbId.arbId, parId))
                    {
                        var liga = db.getLigaById(ligId);
                        var userId = arbPar.tblArbitros.arbUserId;
                        var usuPassword = "";
                        if (userId == null)
                        {
                            var tblUser = db.getUserByUserEmail(arbPar.tblArbitros.arbCorreo);
                            if (tblUser != null)
                            {
                                db.setUsuarioArbitro(arbPar.arbId, tblUser.Id);
                                userId = tblUser.Id;
                            }
                            else
                            {
                                ApplicationUser usr = new ApplicationUser
                                {
                                    UserName = email,
                                    Email = email,
                                    PhoneNumber = "",
                                    EmailConfirmed = true,
                                    usuEstatus = true
                                };
                                usuPassword = db.generator_Pass();
                                var modelRegister = new RegisterViewModel();
                                modelRegister.usuPassword = usuPassword;
                                modelRegister.usuEmail = email;
                                var newUser = new AccountController()._RegisterNewAccount(modelRegister, "");

                                if (newUser)
                                {
                                    ApplicationUser usuario = db.getUserByUserEmail(email);
                                    if (usuario.usuRolActual == null)
                                    {
                                        var rolId = db.getRoleByName(constClass.rolPlayer).Id;
                                        db.setCurrentUserRole(usuario.Id, rolId);
                                    }
                                    var profile = db.getUserMainProfile(usuario.Id);
                                    if (profile == null)
                                    {
                                        var prof = new schemaUsersProfiles();
                                        prof.uprNombres = (usuario.UserName != null) ? usuario.UserName : "-";
                                        prof.uprApellidos = "-";
                                        prof.uprTelefono = "";
                                        db.setUserProfileMain_UpdateInsert(usuario, prof);
                                    }
                                    userId = usuario.Id;
                                }

                            }
                            user = db.getUserById(userId);
                         
                            enviarEmailArbitroAviso(user, usuPassword);
                            
                            var rolActual = user.usuRolActual;
                            var rolReferee = db.getRoleByName(constClass.rolReferee);
                            if (rolActual != null)
                            {
                                if (rolReferee.Id.ToUpper() != rolActual.ToUpper())
                                {
                                    var rolUser = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolReferee.ToUpper());
                                    if (rolUser.Count() <= 0)
                                        UserManager.AddToRole(user.Id, constClass.rolReferee);
                                    db.setCurrentUserRole(user.Id, rolReferee.Id);
                                }
                            }
                            else
                            {
                                UserManager.AddToRole(user.Id, constClass.rolReferee);
                                db.setCurrentUserRole(user.Id, rolReferee.Id);
                            }
                            
                        }
                        if (arbId.arbUserId == null)
                        {
                            db.setArbitroUserId(user.Id, user.Email);
                        }
                        if (arbId.arbEstatus == false)
                        {
                            db.setArbitros_ConfirmarParticipacion(user.Id, user.Email, ligId);
                        }
                        enviarEmailArbitroPartido(email, liga.tblUserCreador.Email, liga.ligNombreLiga, parId, true);
                    }
                }               
            }
            else
            {
                var liga = db.getLigaById(ligId);
                enviarEmailArbitroPartido(email, liga.tblUserCreador.Email, liga.ligNombreLiga, parId, true);
                //enviarEmailArbitroPartido(email, "fredy_aa15@hotmail.com", liga.ligNombreLiga, parId, true);

            }
            if (user != null)
            {
                SignInManager.SignIn(user, false, false);
            }
            string rand = Global_Functions.getRandomString(10);

            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            ViewBag.jsScript = redirectHome;

            return Redirect("Index");
        }
        [NonAction]
        public bool enviarEmailArbitroAviso(ApplicationUser user, string password)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = user.Email;

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroAvisoConfirmacion.html");

                body = body.Replace("<%= NombreJugador %>", user.UserName);

                body = body.Replace("<%= usuario %>", user.Email);
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

        public bool enviarEmailArbitroConfirmacion(string emailArb, string emailAdmLig, string liga)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = emailAdmLig;

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroAvisoConfirmacionAdm.html");

                body = body.Replace("<%= NombreJugador %>", emailArb);
                body = body.Replace("<%= NombreLiga %>", liga);
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

        public bool enviarEmailArbitroPartido(string emailArb, string emailAdmLig, string liga,int parId, bool participacion)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
            var partido = db.getPartidoById(parId);
            string status1, status2, status3;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                //string emailTo = emailAdmLig;

                string emailTo = emailAdmLig;

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroAvisoPartidoAdm.html");

                if(participacion)
                {
                    status1 = "Confirmacion";
                    status2 = "aceptado";
                    status3 = "Arbitro Acepta Partido";
                }
                else
                {
                    status1 = "Rechazo";
                    status2 = "rechazado";
                    status3 = "Arbitro Rechaza Partido";
                }

                body = body.Replace("<%= status1 %>", status1);
                body = body.Replace("<%= status2 %>", status2);
                body = body.Replace("<%= NombreJugador %>", emailArb);
                body = body.Replace("<%= NombreLiga %>", liga);
                body = body.Replace("<%= NombreTorneo %>", partido.tblTorneos.torNombreTorneo);
                body = body.Replace("<%= fecha %>", partido.parFecha_Fin.ToString());
                body = body.Replace("<%= NombreEquipoUno %>", partido.equNombreEquipoUno);
                body = body.Replace("<%= NombreEquipoDos %>", partido.equNombreEquipoDos);            
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, status3, body,
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

        [AllowAnonymous]

        public ActionResult ArbitroPartidoRechazar(string email, int ligId, int parId, string code)
        {

            var arbPar = db.getArbitroPartidoByParId(parId);
            if (arbPar != null)
            {
                var arbId = arbPar.arbId;
                if (arbPar.arbCodigoConfirmacion == code)
                {
                    var emailTo = db.getLigaById(ligId).tblUserCreador.Email;
                    var partido = db.getPartidoById(parId);
                    var coAdminTorneo = db.getTorneoCoadministradoresById(partido.torId).ToList();
                    var coAdminLiga = db.getCoAdminLigasByLigIg(partido.ligId).ToList();
                    if (coAdminTorneo.Any())
                    {
                        emailTo += "," + String.Join(",", coAdminTorneo.Select(s => s.tcaEmail).ToArray());
                    }
                    if (coAdminLiga.Any())
                    {
                        emailTo += "," + String.Join(",", coAdminLiga.Select(s => s.tblUsuario.Email).ToArray());
                    }
                    if (db.setRechazarArbitroPartido(arbId, parId))
                    {
                        enviarEmailArbitroPartido(email, emailTo, db.getLigaById(ligId).ligNombreLiga, parId, false);
                        return View("AccionesCorreos/_ParticipanteRechazar");
                    }
                }
            }
            //else
            //{
            //    enviarEmailArbitroPartido(email, emailTo, "Liga prueba", parId, false);
            //}
            return View("AccionesCorreos/_ParticipanteRechazar");
        }
        public ActionResult _ArbitrosGrid_Callback()
        {
            var par = db.getArbitroPartidoRechazado();
            par.ForEach(f => f.estado = "Partido Cancelado");
            ViewBag.totalArbitrosRechados = par.Count;
            return PartialView("Referee/_GridViewArbitrosRejected",par);
        }

        public ActionResult _ArbitrosAceptedGrid_Callback()
        {
            var par = db.getArbitroPartidoAceptado();
            par.ForEach(f => f.estado = "Partido Aceptados");
            ViewBag.totalArbitrosAceptados = par.Count;
            return PartialView("Referee/_GridViewArbitrosAcepted", par);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public JsonResult EliminarEstadistica(string idJugador, int idEquipo, int idPartido)
        {
            try
            {
                var result = db.set_eliminarEstadistica(idJugador, idEquipo, idPartido);
                if(result==true)
                    return Json(new { err = false, msg = "Estadistica Eliminada" });
                else
                    return Json(new { err = true, msg = "Error inesperado" });
            }
            catch(Exception ex)
            {
                return Json(new{err=true, msg=ex.Message});
            }
        }
    }
}
