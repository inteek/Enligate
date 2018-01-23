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
using Microsoft.Owin.Security;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace sw_EnligateWeb.Controllers
{
    public class AdminEquiposController : MyBaseController
    {
        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminEquiposController()
        {

        }

        public AdminEquiposController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        /// <summary>
        /// Redirecciona a la pantalla principal del administrador de equipos.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
            //return RedirectToAction("Equipos");
        }

        #region Equipos

        /// <summary>
        /// Regresa la vista de la pantalla de equipos
        /// </summary>
        /// <returns></returns>
        public ActionResult Equipos()
        {
            return View();
        }

        #region Crear/Editar Equipos

        #region Acciones

        /// <summary>
        /// Accion que se ejecuta cuando se ingresa a la pantalla de crear nuevo equipo.
        /// </summary>
        /// <returns></returns>
       [AllowAnonymous]
        public ActionResult NuevoEdit(int equId,int torId, bool adminLigaTorneos = false, bool usuAgregarCoadmin = false,bool edit=true)
        {
            ModelState.Clear();

            var model = _EquipoNuevo_OnLoadViewModel(torId, equId);
            model.equAdminLigaTorneos = adminLigaTorneos;
            model.usuAgregarCoadmin = usuAgregarCoadmin;
            model.edit = edit;
            return PartialView("Equipos/_NuevoEdit", model);
        }

        /// <summary>
        /// Accion que busca al participante en la tabla de usuarios para regresar sus datos.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _NuevoEdit_BuscarJugador(AgregarJugadorEquipoViewModel model)
        {
            string nombre = "";
            string id = "";

            if (ModelState.IsValid)
            {
                var jugador = db.getUserByUserEmail(model.jugCorreo.Trim().ToLower());
                if (jugador != null)
                {
                    var perfil = db.getUserMainProfile(jugador.Id);
                    nombre = (perfil != null) ? (perfil.uprNombres + " " + perfil.uprApellidos).Replace("-", "").Trim() : "";
                    id = jugador.Id;
                }
            }

            return Json(new { nombre = nombre, id = id});
        }

        /// <summary>
        /// Agrega un nuevo jugador a la lista de jugadores.
        /// Si el jugador ya estaba agregado lo busca y lo actualiza para evitar que se repitan.
        /// </summary>
        /// <param name="modelJugador"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _NuevoEdit_AgregarJugador(AgregarJugadorEquipoViewModel modelJugador, int torMaxJugadoresEquipo, int torId,int equId)
        {   
            try
            {
                modelJugador.listJugadores.RemoveAll(r => r.jugCorreo == modelJugador.jugCorreo);
                var save_status = true;
                var jugadorUser = db.getUserByUserEmail(modelJugador.jugCorreo.Trim().ToLower());
                if (jugadorUser != null)
                {
                    var perfil = db.getUserMainProfile(jugadorUser.Id);
                    modelJugador.jugNombre = (perfil != null) ? (perfil.uprNombres + " " + perfil.uprApellidos).Replace("-", "").Replace(",", " ").Trim() : "Perfil sin nombre   ";

                }
                else
                {
                    modelJugador.jugNombre = "Perfil sin nombre";
                }

                var model = new EquiposJugadoresViewModel();
                var userM = db.getUserById(User.Identity.GetUserId());
                var roles = db.getRoles();
                var rol_actual = roles.Where(l => l.rolId.ToUpper() == userM.usuRolActual.ToUpper()).FirstOrDefault();
                model.torMaximoJugadoresEquipo = torMaxJugadoresEquipo;
                model.listJugadores = modelJugador.listJugadores;

                if (rol_actual.rolName == constClass.rolOwners || rol_actual.rolName == constClass.rolCoach || rol_actual.rolName == constClass.rolAdminTorneos)
                {
                    model.edit = true;
                    model.equAdminLigaTorneos = true;
                }

                foreach (var item in model.listJugadores)
                {
                    item.jugNombre = RemoveAccentsWithRegEx(item.jugNombre);
                }
                if (true)
                {
                    if (model.listJugadores.Where(j => j.jugEstatus == true).ToList().Count < torMaxJugadoresEquipo)
                    {
                        var user = db.getUserByUserEmail(modelJugador.jugCorreo.Trim().ToLower());
                        var val = true;
                        var jug_user_id = "";
                        if (user != null)
                        {
                            if (user.usuEstatus != true)
                            {
                                ModelState.AddModelError(constClass.info, "El usuario " + model.jugCorreo + " esta deshabilitado.");
                                val = false;
                                save_status = false;
                            }
                            jug_user_id = user.Id;
                        }
                        if (val)
                        {
                            var jugador = modelJugador.listJugadores.Where(j => j.jugCorreo.Trim().ToLower() == modelJugador.jugCorreo.Trim().ToLower()).FirstOrDefault();
                            if (jugador == null)
                            {
                                if (modelJugador.listJugadores == null)
                                    modelJugador.listJugadores = new List<JugadoresViewModel>();

                                jugador = new JugadoresViewModel();
                                jugador.jugCorreo = modelJugador.jugCorreo;
                                jugador.jugNombre = RemoveAccentsWithRegEx(modelJugador.jugNombre);
                                jugador.jugUserId = (jug_user_id != "")? jug_user_id  : null;
                                jugador.jugEstatus = true;
                                jugador.jugConfirmado = false;
                                jugador.jugNuevo = true;
                                jugador.jugCodConfirmacion = false;

                                modelJugador.listJugadores.Add(jugador);
                            }
                            else
                            {
                                jugador.jugCodConfirmacion = false;
                                jugador.jugNombre = RemoveAccentsWithRegEx(modelJugador.jugNombre);

                                jugador.jugEstatus = true;
                            }

                            ModelState.Clear();
                            model.jugCorreo = "";
                            model.jugNombre = "";
                            model.jugUserId = "";

                            ModelState.AddModelError(constClass.success, "Se agrego el participante.");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError(constClass.info, string.Format(@"Se ha alcanzado el máximo de participantes y no se ha  
                                                                               agregado a la lista, el máximo es de {0}.",
                                                                                   torMaxJugadoresEquipo.ToString()));
                        save_status = false;
                    }
                    
                }
                model.torId = torId;
                model.equId = equId;
                
                return Json(new { html = RenderPartialViewToString("Jugadores/_JugadoresEquipo_Agregar", model) , save =  save_status});
            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
        }

        /// <summary>
        /// Quita al jugador de la lista
        /// </summary>
        /// <param name="modelJugador"></param>
        /// <param name="jugCorreoEliminar"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _NuevoEdit_QuitarJugador(AgregarJugadorEquipoViewModel modelJugador, string jugCorreoEliminar, int torMaxJugadoresEquipo, int torId,int equId)
        {
            bool noEncontrado = false;

            var model = new EquiposJugadoresViewModel();
            model.torMaximoJugadoresEquipo = torMaxJugadoresEquipo;
            model.listJugadores = modelJugador.listJugadores;
            if (model.listJugadores.Count > 0 && jugCorreoEliminar != null)
            {
                var jugador = modelJugador.listJugadores.Where(j => j.jugCorreo.Trim().ToLower() == jugCorreoEliminar.Trim().ToLower()).FirstOrDefault();
                if (jugador != null)
                {
                    ModelState.Clear();
                    jugador.jugEstatus = false;
                    jugador.jugConfirmado= false;
                    jugador.jugCodConfirmacion = true;

                    ModelState.AddModelError(constClass.success, "El participante se eliminara al guardar el equipo.");
                    var equipo = db.getEquipoById(equId);
                    if (equipo != null)
                    {
                        enviarEmailEliminarJugador(jugador.jugCorreo, equipo.equNombreEquipo);
                    }
                    
                }
                else
                    noEncontrado = true;
            }
            else
                noEncontrado = true;

            if(noEncontrado)
                ModelState.AddModelError(constClass.info, "No se encontro el participante a eliminar.");
            model.edit = true;
            model.equAdminLigaTorneos = true;
            model.torId = torId;
            model.equId = equId;
            return PartialView("Jugadores/_JugadoresEquipo_Agregar", model);
        }

        /// <summary>
        /// Accon que cambia el codigo de confirmación y reenvia la invitación al participante seleccionado.
        /// </summary>
        /// <param name="jugCorreo"></param>
        /// <param name="torId"></param>
        /// <param name="equId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _NuevoEdit_InvitarJugador(string jugCorreo, int torId, int equId)
        {
            string msgResRender;
            bool error = false;

            string codigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
            var jugador = db.setJugadoresEquipo_VolverInvitar(jugCorreo, torId, equId, codigoConfirmacion);
            if(jugador != null)
            {
                string nombreAdmin = null;
                string nombreEquipo = null;
                string nombreTorneo = null;
                if(jugador.equId > 0)
                {
                    var perfilAdmin = db.getUserMainProfile(jugador.tblEquipos.equUserIdCreador);
                    if (perfilAdmin != null)
                        nombreAdmin = (perfilAdmin.uprNombres.Trim() != "-") ? (perfilAdmin.uprNombres.Trim() + " " + perfilAdmin.uprApellidos.Trim()).Trim() : perfilAdmin.tblUsers.Email;
                    else
                    {
                        var userAdmin = db.getUserById(jugador.tblEquipos.equUserIdCreador);
                        nombreAdmin = userAdmin.Email;
                    }
                    nombreEquipo = jugador.tblEquipos.equNombreEquipo;
                }
                nombreTorneo = jugador.tblTorneos.torNombreTorneo;



                var listJug = new List<schemaJugadorEquipos>();
                listJug.Add(jugador);

                if (!enviarEmailParticipanteInvitacion(listJug, torId, equId, codigoConfirmacion, nombreAdmin, nombreTorneo, nombreEquipo))
                    error = true;
            }

            if(!error)
                ModelState.AddModelError(constClass.success, "Se envio nuevamente la invitación al participante " + jugador.jugCorrreo + ".");
            else
                ModelState.AddModelError(constClass.error, "Hubo un error invitando al participante. Intentalo nuevamente.");

            msgResRender =  RenderPartialViewToString("_ModalState_Errors");
            return Content(msgResRender);
        }

        /// <summary>
        /// Accion que guarda un nuevo Equipo y/o los participantes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// [AllowAnonymous]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult _PayTeam(string userId, string status,string total,string conceptoId, string referencia, string ip, string metodo, string descripcion, string concepto, string transaccionId)
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
                        var setEquipo = db.setEquipoPago(equId, userId,true);
                        if (setEquipo)
                            return Json(equId);
                        else
                            return Json("Error al confirmar pago de equipo");

                    }
                    else if (concepto == "Jugador")
                    {
                        var tor = db.getEquipoById(Convert.ToInt16(conceptoId));
                        var user = db.getUserById(userId);
                        var conf = db.setJugadoresEquipo_ConfirmarParticipacion(user.Email, tor.torId, Convert.ToInt16(conceptoId), userId,"",true);
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

        public ActionResult _InscribirEquipoGratis(int equId)
        {
            var setEquipo = db.setEquipoPago(equId,"",true);
            if (setEquipo)
                return Json(equId, JsonRequestBehavior.AllowGet);
            else
                return Json("Error al confirmar pago de equipo", JsonRequestBehavior.AllowGet);
        }
        public ActionResult _InscripcionJugador(int equId,int torId, string email)
        {
            var resultJson = new JsonResultViewModel();
            var tor = db.getEquipoById(equId);

            var juUser = db.getUserByUserEmail(email);

            var user = db.getUserById(User.Identity.GetUserId());

            var usuarioId = "";
            if (juUser!=null)
            {
                if (juUser.Id.ToUpper()==user.Id.ToUpper())
                {
                    usuarioId = user.Id;
                }else
                {
                    usuarioId = juUser.Id;
                }
            }else
            {
                ApplicationUser usuario = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = "",
                    EmailConfirmed = false,
                    usuEstatus = true
                };
                var usuPassword = db.generator_Pass();
                var modelRegister = new RegisterViewModel();
                modelRegister.usuPassword = usuPassword;
                modelRegister.usuEmail = email;
                var newUser = new AccountController()._RegisterNewAccount(modelRegister, "");
                if (newUser)
                {
                    ApplicationUser userId = db.getUserByUserEmail(email);
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
                }

            }

            var conf = db.setJugadoresEquipo_ConfirmarParticipacion(email, torId, equId, usuarioId,"",true);
            if (conf)
            {

                var userRole = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolPlayer.ToUpper());
                if (userRole == null)
                {
                    var rolId = db.getRoleByName(constClass.rolPlayer).Id;
                    UserManager.AddToRole(user.Id, constClass.rolPlayer);
                    db.setCurrentUserRole(user.Id, rolId);
                }
                ModelState.AddModelError(constClass.success, "Jugador Registrado !!.");
                resultJson.booSuccess = true;
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Error al Registrar Jugador.");
                resultJson.booSuccess = false;
            }

            //ModelState.AddModelError(constClass.error, "Error al Registrar Jugador.");
            resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");

            return Json(resultJson);
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
                prof.uprNombres = "-";
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
                    db.setClearEmailValidation(user);
                    //SignInManager.SignIn(user, false, false);                    
                }
            }
            return user.Id;
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EquipoJugadoresNuevoEdit_GuardarNuevo(EquiposJugadoresViewModel model, bool? inscTorneo=true)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booHasPartialView = true;
            if ((bool)inscTorneo)
            {

            }
            var res = _EquipoNuevo_CrearEditarEquipoParticipantes(model, inscTorneo);
            var errors = ModelState.Where(e => e.Key == constClass.error).Select(e => e.Value.Errors).ToList();

            if (res.result)
                ModelState.Clear();
            foreach (var error in errors)
            {
                try
                {
                    ModelState.AddModelError(constClass.error, error[0].ErrorMessage);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            //ModelState.AddModelError(res.errorTipo, res.mensaje);
            if (!res.result)
            {
                ModelState.AddModelError(constClass.error, "" + res.mensaje);
                resultJson.strPartialViewString = RenderPartialViewToString("home/_inscribirEquipoForm", model);
                resultJson.booSuccess = false;
                resultJson.strErrMessagePartialViewString = RenderPartialViewToString("_ModalState_Errors");
            }
            else {
                ModelState.AddModelError(constClass.success, "Cambios Realizados");
                resultJson.booSuccess = true;
                resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                
                //return PartialView("Equipos/_NuevoEdit", model);
                //resultJson.strPartialViewString = RenderPartialViewToString("Equipos/_NuevoEdit", model);

            }

            /*var usuario = db.getUserById(User.Identity.GetUserId());
            var rolOwner = db.getRoleByName(constClass.rolOwners).Id.ToUpper();
            if (usuario.usuRolActual.ToUpper() != rolOwner)
            {
                var roles = db.getUserRoles(usuario).Where(l=> l.rolName.ToUpper() == constClass.rolCoach);

                var rol = db.getRoleByName(constClass.rolCoach).Id;

                if (usuario.usuRolActual.ToUpper() != rol.ToUpper())
                {
                    if (roles.Count() <= 0)
                    {
                        UserManager.AddToRole(usuario.Id, constClass.rolCoach);
                    }
                    db.setCurrentUserRole(usuario.Id, rol);
                }
            }*/
            if (res.equId > 0)
            {
                resultJson.equId = res.equId;
                return Json(resultJson);
            }

            return Json(resultJson);

            //return PartialView("Equipos/_NuevoEdit", model);
        }

        #region GridJugadores

        /// <summary>
        /// Accion que regresa la vista del grid de los jugadores (participantes)
        /// </summary>
        /// <returns></returns>
        /// 
        [AllowAnonymous]
        public ActionResult EquipoJugadoresGrid()
        {
            return PartialView("Jugadores/_JugadoresEquipo_Grid");
        }

        /// <summary>
        /// Accion que busca los perfiles de los jugadores de la lista y los acomoda para que se muestren en el grid de jugadores.
        /// </summary>
        /// <param name="jugadores"></param>
        /// <returns></returns>

        public static string RemoveAccentsWithRegEx(string inputString)
        {
            if (inputString != null)
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
            }
            
            return inputString;
        }

        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult _EquipoJugadoresGrid_Callback(string jugadores,bool edit)
        {
            var jugList = new List<JugadoresViewModel>();
            
            if (jugadores.Trim() != "")
            {

                jugadores = jugadores.Replace("&#39;", "'");
                jugadores = RemoveAccentsWithRegEx(jugadores);
                var jugListArr = jugadores.Split(';');
                foreach (var item in jugListArr)
                {
                    var jug = item.Split(',');
                    var perfil = db.getUserMainProfile(jug[2]);
                    var jugNombre = (perfil != null) ? (perfil.uprNombres + " " + perfil.uprApellidos).Replace("-", "").Replace(",", " ").Trim() : "Perfil sin nombre   ";
                    jugList.Add(new JugadoresViewModel()
                    {
                        jugCorreo = jug[0],
                        jugNombre = jugNombre,
                        jugUserId = jug[2],
                        jugEstatus = bool.Parse(jug[3]),
                        jugConfirmado = bool.Parse(jug[4]),
                        jugNuevo = bool.Parse(jug[5]),
                        jugCodConfirmacion = bool.Parse(jug[6])
                    });
                }
            }
            
            List<JugadoresEquipoGridViewModel> model = new List<JugadoresEquipoGridViewModel>();
            jugList = db.getJugadoreIdByListEquipo(jugList);
            var resBusqueda = db.getJugadoresByListEquipo(jugList);
            resBusqueda = resBusqueda.Where(j => j.jugEstatus == true).ToList();
            int count = 0;
            for (int i = 0; i < resBusqueda.Count; i++)
            {
                count++;
                var item = new JugadoresEquipoGridViewModel();
                item.rowId = count;

                item.jug1.jugImg = resBusqueda[i].jugImg;
                item.jug1.jugNombre = resBusqueda[i].jugNombre;
                item.jug1.jugGenero = resBusqueda[i].jugGenero;
                item.jug1.jugFechaNacimiento = resBusqueda[i].jugFechaNacimiento;
                item.jug1.jugCiudad = resBusqueda[i].jugCiudad;
                item.jug1.jugTelefono = resBusqueda[i].jugTelefono;
                item.jug1.jugCorreo = resBusqueda[i].jugCorreo;
                item.jug1.jugConfirmado = resBusqueda[i].jugConfirmado;
                item.jug1.jugCodConfirmacion = resBusqueda[i].jugCodConfirmacion;
                item.jug1.jugNuevo = resBusqueda[i].jugNuevo;
                item.jug1.jugEstatus = resBusqueda[i].jugEstatus;

                i++;
                if(i < resBusqueda.Count)
                {
                    item.jug2.jugImg = resBusqueda[i].jugImg;
                    item.jug2.jugNombre = resBusqueda[i].jugNombre;
                    item.jug2.jugGenero = resBusqueda[i].jugGenero;
                    item.jug2.jugFechaNacimiento = resBusqueda[i].jugFechaNacimiento;
                    item.jug2.jugCiudad = resBusqueda[i].jugCiudad;
                    item.jug2.jugTelefono = resBusqueda[i].jugTelefono;
                    item.jug2.jugCorreo = resBusqueda[i].jugCorreo;
                    item.jug2.jugConfirmado = resBusqueda[i].jugConfirmado;
                    item.jug2.jugCodConfirmacion = resBusqueda[i].jugCodConfirmacion;
                    item.jug2.jugNuevo = resBusqueda[i].jugNuevo;
                    item.jug2.jugEstatus = resBusqueda[i].jugEstatus;
                }
                model.Add(item);
            }
            model.ForEach(f=> f.edit=edit);
            return PartialView("Jugadores/_JugadoresEquipo_Grid", model);
        }

        #endregion

        #endregion

        #region Funciones

        /// <summary>
        /// Prepara el modelo para un nuevo Equipo/Participantes de un torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        [NonAction]
        public EquiposJugadoresViewModel _EquipoNuevo_OnLoadViewModel(int torId, int equId)
        {
            var model = new EquiposJugadoresViewModel();
            if (torId > 0)
            {
                model.torId = torId;
                model.equId = equId;
                model.tblTorneo = db.getTorneoById(torId);
                model.torKey = model.tblTorneo.torFechaCreacionUTC.ToString("ddMMyyyyhhmmssfff");
                var totales = db.getEquiposJugadoresTotalByTorneo(torId);
                if (totales != null)
                {
                    model.torEquiposRegistrados = totales.totalEquipos;
                    model.torJugadoresRegistrados = totales.totalJugadores;
                }
                model.torMaximoEquipos = model.tblTorneo.torNumeroEquipos;
                model.torMaximoJugadoresEquipo = model.tblTorneo.torMaxJugadoresEquipo;
  //              if (model.tblTorneo.torTipo == constClass.torTipoCoaching || !model.tblTorneo.torDeporteEnEquipo)
   //             {
  //                  model.mostrarDatosEquipo = false;
   //                 model.listJugadores = db.getJugadoresListaByTorneo_EquipoId(model.torId, model.equId);
  //             }
  //              else
  //              {
                    model.mostrarDatosEquipo = true;
                    if (model.equId > 0)
                    {
                        model.listJugadores = db.getJugadoresListaByTorneo_EquipoId(model.torId, model.equId);
                        foreach (var item in model.listJugadores)
                        {
                            item.jugNombre = RemoveAccentsWithRegEx(item.jugNombre);
                        }
                        var equipo = db.getEquipoById(model.equId);
                        if(equipo != null)
                        {
                            model.equImg = equipo.equImgUrl;
                            model.equNombre = equipo.equNombreEquipo;

                            model.equCorreoAdministrador = equipo.equUserIdCreador!=null? db.getUserById(equipo.equUserIdCreador).UserName : equipo.equAdminCorreo ;
                            model.equCreadorEquipoId = equipo.equUserIdCreador;
                        }
                   }
                if (model.tblTorneo.torTipo == constClass.torTipoCoaching)
                    model.usuAgregarCoadmin = false;
                else
                    model.usuAgregarCoadmin = true;
                model.equEstatus = true;
            }

            return model;
        }

        /// <summary>
        /// Metodo que realizar la creación o edición de un equipo y/o participantes de un torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NonAction]
        public EquiposJugadoresGuardarResultado _EquipoNuevo_CrearEditarEquipoParticipantes(EquiposJugadoresViewModel model, bool? inscTorneo=true)
        {
            var resultado = new EquiposJugadoresGuardarResultado();
            var nuevo_user = false;
            var nuevo_coach = false;
            var new_team = false;
            var change_admin = "";
            string passwordCode = db.generator_Pass();

            //Verifica que el torneo y el token sean validos para que no agregue un equipo a un torneo que no es
            //y elimina del modelo los campos que no deben de ir.
            var modelValidar = _EquipoNuevo_OnLoadViewModel(model.torId, model.equId);
            model.tblTorneo = modelValidar.tblTorneo;
            
            if (model.tblTorneo != null && model.tblTorneo.torFechaCreacionUTC.ToString(constClass.formatDateToken) == model.torKey)
            {
                model.equEstatus = true;
                model.torPrecio = model.tblTorneo.torPrecioTorneo;

                if (!model.mostrarDatosEquipo)
                {
                    ModelState.Remove("equNombre");
                    ModelState.Remove("equCorreoAdministrador");
                }

                if (model.mostrarDatosEquipo && !model.equAdminLigaTorneos)
                    ModelState.Remove("equCorreoAdministrador");

                ModelState.Remove("jugCorreo");
                ModelState.Remove("jugNombre");
                ModelState.Remove("jugUserId");
            }
            else
            {
                resultado.result = false;
                resultado.errorTipo = constClass.error;
                resultado.mensaje = @"El token para crear el equipo y/o participantes no es valido,
                                      recarga la página e intenta nuevamente.";
                return resultado;
            }

            //Valida que haya cupo en el torneo.
            if (modelValidar.mostrarDatosEquipo)
            {
                if (modelValidar.torEquiposRegistrados >= modelValidar.torMaximoEquipos && model.equId == 0)
                {
                    resultado.result = false;
                    resultado.errorTipo = constClass.info;
                    resultado.mensaje = @"Ya no hay cupo en el torneo para más equipos.";
                    return resultado;
                }
                if (model.listJugadores.ToList().Count > modelValidar.torMaximoJugadoresEquipo)
                {
                    resultado.result = false;
                    resultado.errorTipo = constClass.info;
                    resultado.mensaje = string.Format(@"Se supera el máximo de jugadores por equipo, el máximo es de {0}.",
                                                      model.tblTorneo.torMaxJugadoresEquipo.ToString());
                    return resultado;
                }
            }
            else
            {
                if (model.listJugadores.ToList().Count > modelValidar.torMaximoJugadoresEquipo)
                {
                    resultado.result = false;
                    resultado.errorTipo = constClass.info;
                    resultado.mensaje = string.Format(@"Ya no hay cupo en el torneo para más participantes, el máximo es de {0}.",
                                                      model.tblTorneo.torMaxJugadoresEquipo.ToString());
                    return resultado;
                }
            }

            //Validar que los campos estén bien para comenzar el proceso de creacion o edición de un equipo y/o participantes.
            if (ModelState.IsValid)
            {
                if (modelValidar.mostrarDatosEquipo)
                {
                    //Valida si el equipo lo esta creando el admin de liga o torneo
                    //para crear el usuario del administrador en caso de que no exista.
                    if (model.equAdminLigaTorneos)
                    {
                        var email = model.equCorreoAdministrador;
                        var role = constClass.rolCoach;
                        //var user = _CreaUsuarioByEmail(, ref errMensaje, constClass.rolCoach);
                        var user = db.getUserByUserEmail(email);

                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                PhoneNumber = "",
                                EmailConfirmed = true,
                                usuEstatus = true
                            };
                            var result = UserManager.Create(user, passwordCode);
                            if (result != IdentityResult.Success)
                            {
                                resultado.result = false;
                                resultado.errorTipo = constClass.error;
                                resultado.mensaje = "Hubo un error creando al usuario. Intentalo nuevamente.";
                                return resultado;
                            }
                            else
                            {
                                var prof = new schemaUsersProfiles();
                                prof.uprNombres = "-";
                                prof.uprApellidos = "-";
                                prof.uprTelefono = "";
                                db.setUserProfileMain_UpdateInsert(user, prof);
                                if (role != null)
                                    UserManager.AddToRole(user.Id, role);
                                else
                                    UserManager.AddToRole(user.Id, constClass.rolPlayer);
                                nuevo_user = true;
                            }                            
                        }
                        else
                        {
                            model.equCreadorEquipoId = user.Id;

                            var roles = db.getUserRoles(user);
                            if (!roles.Where(l => l.rolName.ToUpper() == role.ToUpper()).Any())
                                UserManager.AddToRole(user.Id, role);

                            var rol_coach = db.getRoleByName(role);

                            if (model.equCreadorEquipoId.ToUpper() != User.Identity.GetUserId().ToUpper())
                            {
                                if (user.usuRolActual=="")
                                {
                                    db.setCurrentUserRole(user.Id, rol_coach.Id);
                                }
                            }
                            else
                            {
                                var rolesAll = db.getRoles();
                                var aux = true;
                                var rolAdmLig = rolesAll.Where(l => l.rolName.ToUpper() == constClass.rolOwners.ToUpper()).FirstOrDefault();
                                var rolAdmTor = rolesAll.Where(l => l.rolName.ToUpper() == constClass.rolAdminTorneos.ToUpper()).FirstOrDefault();
                                if (user.usuRolActual.ToUpper() != rolAdmLig.rolId.ToUpper())
                                {
                                    db.setCurrentUserRole(user.Id, rol_coach.Id);
                                    aux = false;
                                }
                                else
                                {
                                    aux = false;
                                }
                                if (aux)
                                {
                                    if (user.usuRolActual.ToUpper() != rolAdmTor.rolId.ToUpper()){
                                        db.setCurrentUserRole(user.Id, rol_coach.Id);
                                    }
                                }
                            }
                        }                     
                    }
                    
                    //Se guardará el equipo y los datos de los jugadores si es un torneo interno con deporte en equipo.
                    //Guardar la imagen
                    foreach (string file in Request.Files)
                    {
                        string filename = "", filenameDB="";
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            string imgEquipo = model.equImg;
                            model.equImg = null;

                            string urlPath = Server.MapPath(constClass.urlEquiposImagenes);
                            if (!System.IO.Directory.Exists(urlPath))
                                System.IO.Directory.CreateDirectory(urlPath);

                            var extension = Path.GetExtension(fileContent.FileName);
                            if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                            {
                                filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                                filenameDB = constClass.urlEquiposImagenes + "/" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                                filenameDB = filenameDB.Replace("~","");
                                fileContent.SaveAs(filename);
                                bool savedFile = System.IO.File.Exists(filename);

                                if (savedFile)
                                {
                                    model.equImg = filenameDB;
                                    if (System.IO.File.Exists(imgEquipo))
                                        System.IO.File.Delete(imgEquipo);
                                }
                                else
                                {
                                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen del equipo.");
                                    model.equImg = imgEquipo;
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                            }
                        }
                    }
                }

                //Agrega a los jugadores y si es en equipo crea un nuevo equipo
                string codigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
                var userId = User.Identity.GetUserId();
                if(model.equId > 0)
                {
                    var equ_temp = db.getEquipoById(model.equId);
                    if (equ_temp.equAdminCorreo.ToUpper() != model.equCorreoAdministrador.ToUpper())
                    {
                        nuevo_coach = true;
                        change_admin = equ_temp.equAdminCorreo;
                    }
                }
                else
                {
                    new_team = true;
                }
                var equId = db.setJugadoresEquipo_AgregarEditar(model, codigoConfirmacion,userId, inscTorneo);

                if (equId > 0)
                {
                    //Se guardo el Equipo                    
                    //Actualiza el modelo con los datos de la base de datos por si se modifico algo en la pantalla.
                    modelValidar = _EquipoNuevo_OnLoadViewModel(model.torId,0);
                    model.mostrarDatosEquipo = modelValidar.mostrarDatosEquipo;
                    model.tblTorneo = modelValidar.tblTorneo;
                    model.torMaximoEquipos = modelValidar.torMaximoEquipos;
                    model.torEquiposRegistrados = modelValidar.torEquiposRegistrados;
                    model.torMaximoJugadoresEquipo = modelValidar.torMaximoJugadoresEquipo;
                    model.torJugadoresRegistrados = modelValidar.torJugadoresRegistrados;
                    model.torPrecio = modelValidar.tblTorneo.torPrecioTorneo;
                    model.listJugadores = modelValidar.listJugadores;
                    //Envia los correos de invitación.
                    //Se obtienen los jugadores del torneo y/o equipo que fueron agregados
                    //esto se sabe por que son los que tienen el codigo de confirmacion que se genero.
                    var jugEnviarCorreo = db.getJugadoresByTorneo_Equipo(model.torId, model.equId)
                                            .Where(j => j.jugCodigoConfirmacion == codigoConfirmacion).ToList();
                    //Actualiza la lista de jugadores del modelo para que se muestre el boton de pendiente con la opcion
                    //de volver a invitar.
                    (from jug in model.listJugadores
                     join jug2 in jugEnviarCorreo on jug.jugCorreo equals jug2.jugCorrreo
                     select jug).ToList().ForEach(j => j.jugCodConfirmacion = true);

                    string nombreAdmin = null;
                    string nombreEquipo = null;
                    string adminEmail = null;
                    if (model.mostrarDatosEquipo)
                    {
                        var profAdmin = db.getUserMainProfile(model.equCreadorEquipoId);
                        //var userAdmin = db.getUserById(model.equCreadorEquipoId);

                        adminEmail = model.equCorreoAdministrador;
                        if (profAdmin != null)
                            nombreAdmin = (profAdmin.uprNombres.Trim() != "-") ? (profAdmin.uprNombres.Trim() + " " + profAdmin.uprApellidos.Trim()).Trim() : profAdmin.tblUsers.Email;
                        else
                        {                            
                            nombreAdmin = adminEmail;
                        }
                        if (model.mostrarDatosEquipo)
                            nombreEquipo = model.equNombre;
                    }
                    if(jugEnviarCorreo.Count > 0)
                    {
                        if (!enviarEmailParticipanteInvitacion(jugEnviarCorreo, model.torId, model.equId, 
                                                               codigoConfirmacion, nombreAdmin, model.tblTorneo.torNombreTorneo, 
                                                               nombreEquipo))
                            ModelState.AddModelError(constClass.error, "Algunos correos de invitación no se pudieron enviar.");
                    }
                    //Enviar Correo a Admin de Equipo
                    //
                    if (nuevo_user)
                    {
                        enviarEmailCoachPerfilAviso(db.getUserByUserEmail(model.equCorreoAdministrador), passwordCode, model.equNombre, model.tblTorneo.torNombreTorneo, model.equId, model.torId);
                        if (nuevo_coach)
                        {
                            enviarEmailAdminEquipoCancel(change_admin, model.tblTorneo.torNombreTorneo, model.equNombre);
                        }
                    }
                    else if(nuevo_coach)
                    {
                        enviarEmailAdminEquipoInvitacion(model.equCorreoAdministrador, model.torId, model.equId, codigoConfirmacion, nombreAdmin, model.tblTorneo.torNombreTorneo, model.equNombre);
                        enviarEmailAdminEquipoCancel(change_admin, model.tblTorneo.torNombreTorneo, model.equNombre);
                    }
                    else if(new_team)
                    {
                        enviarEmailAdminEquipoInvitacion(model.equCorreoAdministrador, model.torId, model.equId, codigoConfirmacion, nombreAdmin, model.tblTorneo.torNombreTorneo, model.equNombre);
                    }
                    //Regresa el mensaje de que se guardo.
                    resultado.result = true;
                    resultado.errorTipo = constClass.success;
                    resultado.equId = equId;
                    if (model.mostrarDatosEquipo)
                        resultado.mensaje = "El Equipo ha sido guardado.";
                    else
                        resultado.mensaje = "Los participantes han sido guardados.";

                    return resultado;
                }
                else
                {
                    resultado.result = false;
                    resultado.errorTipo = constClass.error;
                    if (model.mostrarDatosEquipo)
                        resultado.mensaje = "Ocurrio un error guardando el equipo. Intenta nuevamente.";
                    else
                        resultado.mensaje = "Ocurrio un error guardando los participantes. Intenta nuevamente.";
                    return resultado;
                }
            }

            resultado.result = false;
            resultado.errorTipo = constClass.error;
            resultado.mensaje = "Favor de llenar todos los campos.";
            return resultado;
        }

        /// <summary>
        /// Crea un usuario a partir de su correo
        /// Envia los correo de cambio de contraseña para que la recupere y de confirmacion de correo.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [NonAction]
        public string _CreaUsuarioByEmail(string email, ref string errMensaje,string role = null)
        {
            //Si el usuario lo creo el administrador de liga o torneo
            //valida que exista el usuario administrador sino lo crea.
           
            return email;
        }

        public ActionResult _Partidos_Equipos(int parId)
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
            if (cancha != null)
            {
                model.canNombre = cancha.lcatNombre;
                model.canchaDireccion = cancha.lcatdomicilio + " #" + cancha.lcatNumExtInt + " " + cancha.lcatColonia + " " + cancha.lcatMunicipio + " " + cancha.lcatEstado + " , C.P: " + cancha.lcatCodigoPostal;
            }
            else
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
            
            return PartialView("Admin/_Partidos_Player", model);
            //return RedirectToAction("MainLeague");
        }

        /// <summary>
        /// Envia el correo para recuperar la contraseña
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [NonAction]
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
        [NonAction]
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
        /// Envia los correos de rechazo de participacion al administrador de torneo o equipo.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        [NonAction]
        public bool enviarEmailParticipanteInvitacion(List<schemaJugadorEquipos> jugadores, int torId, int equId, string codigoConfirmacion, string nombreAdmin, string nombreTorneo, string nombreEquipo)
        {
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
                foreach (var jug in jugadores)
                {

                    var correo = jug.jugCorrreo;

                    // Links del correo

                    var valCorreo = jug.jugCorrreo.ToUpper().EndsWith("@ENLIGATE.COM");
                    if (valCorreo)
                    {
                        var mainPerfil = db.getMainPerfilById(jug.jugUserId);
                        
                        if (mainPerfil.Count != 0)
                        {
                            foreach (var item in mainPerfil)
                            {
                                correo = item.tblUserAdmin.Email;
                            }
                        }
                        else
                        {
                            correo = jug.jugCorrreo;
                        }
                    }

              

                    var confirmarUrl = Url.Action("ParticipanteConfirmar", "Admin", new { email = jug.jugCorrreo, torId = torId, equId = equId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);
                    var rechazarUrl = Url.Action("ParticipanteRechazar", "Admin", new { email = jug.jugCorrreo, torId = torId, equId = equId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);

                    string body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoEquipoInvitacion.html");
                    if (nombreEquipo == null)
                        body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoInvitacion.html");
                    body = body.Replace("<%= NombreAdministrador %>", nombreAdmin);

                    body = body.Replace("<%= NombreJugador %>", jug.jugCorrreo);

                    body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                    body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    body = body.Replace("<%= UrlValidationCode %>", confirmarUrl);
                    body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                    bool mailSended = Global_Functions.sendMail(correo, siteConfig.scoSenderDisplayEmailName, "Invitación a torneo", body,
                                                                siteConfig.scoSenderEmail,
                                                                Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                                siteConfig.scoSenderSMTPServer,
                                                                siteConfig.scoSenderPort,
                                                                null, "", "", true, "");
                    if (!mailSended)
                        correosEnviados = false;
                }
            }else
                correosEnviados = false;
            return correosEnviados;
        }
                
        public bool enviarEmailAdminEquipoCancel(string admin, string torneo, string equipo)
        {
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

                string body = Global_Functions.getBodyHTML("~/Emails/AdministradorTorneoEquipoDelete.html");

                body = body.Replace("<%= NombreAdministrador %>", admin);
                body = body.Replace("<%= NombreEquipo %>", equipo);
                body = body.Replace("<%= NombreTorneo %>", torneo);

                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                
                bool mailSended = Global_Functions.sendMail(admin, siteConfig.scoSenderDisplayEmailName, "Cancelación Coach", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "");
                if (!mailSended)
                    correosEnviados = false;

            }
            return correosEnviados;
        }
         public bool enviarEmailAdminEquipoInvitacion(string CorreoAdmin, int torId, int equId, string codigoConfirmacion, string nombreAdmin, string nombreTorneo, string nombreEquipo)
        {
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
                
                // Links del correo
                    var confirmarUrl = Url.Action("AdminEquipoConfirmar", "Admin", new { email = CorreoAdmin, torId = torId, equId = equId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);
                    var rechazarUrl = Url.Action("AdminEquipoRechazar", "Admin", new { email = CorreoAdmin, torId = torId, equId = equId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);

                    string body = Global_Functions.getBodyHTML("~/Emails/AdministradorTorneoEquipoInvitacion.html");
                    if (nombreEquipo == null)
                           body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoInvitacion.html");
                    body = body.Replace("<%= NombreAdministrador %>", nombreAdmin);
                    body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                    body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    body = body.Replace("<%= UrlValidationCode %>", confirmarUrl);
                    body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                    bool mailSended = Global_Functions.sendMail(CorreoAdmin, siteConfig.scoSenderDisplayEmailName, "Invitación a torneo", body,
                                                                siteConfig.scoSenderEmail,
                                                                Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                                siteConfig.scoSenderSMTPServer,
                                                                siteConfig.scoSenderPort,
                                                                null, "", "", true, "");
                    if (!mailSended)
                        correosEnviados = false;
                
            }
            return correosEnviados;
        }

        #endregion

        #endregion

        #region Coadministradores Equipos

        #region Accionesc

        /// <summary>
        /// Regresa la vista de los coadministradores del Equipo para editarlos o agregar
        /// </summary>
        /// <returns></returns>
        public ActionResult Equipos_CoadminsGridEdit()
        {
            return PartialView("Equipos/_CoadminsGridEdit");
        }

        /// <summary>
        /// Llena el grid de los coadministradores para su edición.
        /// </summary>
        /// <param name="equId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _Equipos_CoadminsGridEdit_Callback(int equId, string token)
        {
            var model = db.getEquipoCoadministradoresById(equId, token).Where(c => c.ecaEstatus == true).ToList();
            return PartialView("Equipos/_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta al guardar un nuevo registro en el grid.
        /// </summary>
        /// <param name="equId"></param>
        /// <param name="key"></param>
        /// <param name="coadmin"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _Equipos_CoadminsGridEdit_AddNewPartial(int equId, string token, EquiposCoAdministradoresViewModel coadmin)
        {
            var model = db.getEquipoCoadministradoresById(equId, token);
            if (coadmin.ecaEmail.Trim() != "")
            {
                string emailAddress = coadmin.ecaEmail.Replace("\"", "").Trim();
                string errMensaje = "";
                bool sendEmail = true;

                var coAdministrador = new schemaEquiposCoAdministradores();
                var equCoadmin = model.Where(c => c.ecaEmail == emailAddress).FirstOrDefault();
                if (equCoadmin == null)
                {
                    //Valida que el usuario exista
                    var user = db.getUserByUserEmail(emailAddress);
                    string id = null;
                    if (user != null)
                        id = user.Id;

                    coAdministrador.equId = equId;
                    coAdministrador.ecaCorreoId = emailAddress;
                    coAdministrador.equUserId = id;
                    coAdministrador.tblUsuario = user;
                    coAdministrador.equConfirmado = false;
                    coAdministrador.equEstatus = true;

                    if (!db.setEquipoCoadmin(coAdministrador))
                    {
                        errMensaje = "Hubo un error agregando al coadministrador. Intentalo nuevamente.";
                        sendEmail = false;
                    }
                }
                else
                {
                    var user = db.getUserByUserEmail(emailAddress);
                    string id = null;
                    if (user != null)
                        id = user.Id;

                    coAdministrador.equId = equId;
                    coAdministrador.ecaCorreoId = emailAddress;
                    coAdministrador.equUserId = id;
                    coAdministrador.tblUsuario = user;
                    coAdministrador.equConfirmado = equCoadmin.ecaConfirmado;
                    coAdministrador.equEstatus = equCoadmin.ecaEstatus;

                    if (coAdministrador.equConfirmado == true && coAdministrador.equEstatus == true)
                        sendEmail = false;
                }

                if (sendEmail)
                { 
                    var equipo = db.getEquipoById(equId);
                    var adminId = User.Identity.GetUserId();
                    //Lo invita el administrador de equipo?
                    if(adminId !=  equipo.equUserIdCreador)
                    {
                        //Lo invita algun coadministrador?
                        if (model.Where(c => c.ecaConfirmado && c.ecaEstatus == true)
                                 .Select(c => c.ecaEmail).Contains(User.Identity.GetUserName()) == false)
                            adminId = equipo.equUserIdCreador;
                    }

                    //Envia mensaje si no hay ningun error
                    if (!EquipoSendCoAdministratorEmail(coAdministrador, adminId))
                    {
                        db.setEquipoCoadmin_Delete(equId, token, emailAddress);
                        errMensaje = "Hubo un error enviando el correo de confirmación. Intenta agregar nuevamente el coadministrador.";
                    }
                }

                if (errMensaje == "")
                {
                    ModelState.Clear();
                    ViewData["EditResult"] = "ok";
                }
                else
                {
                    ViewData["EditError"] = errMensaje;
                    ViewData["EquiposCoAdministradoresViewModel"] = coadmin;
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
                ViewData["EquiposCoAdministradoresViewModel"] = coadmin;
            }

            return PartialView("Equipos/_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que elimina a un coadministrador desde el boton de eliminar del grid.
        /// </summary>
        /// <param name="equId"></param>
        /// <param name="token"></param>
        /// <param name="lcaUserId"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _Equipos_CoadminsGridEdit_Delete(int equId, string token, string ecaEmail)
        {
            if (ecaEmail != null && ecaEmail.Trim() != "")
            {
                if (!db.setEquipoCoadmin_Delete(equId, token, ecaEmail))
                    ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
                else
                    ViewData["EditResult"] = "ok";
            }
            else
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";

            var model = db.getEquipoCoadministradoresById(equId, token);
            return PartialView("Equipos/_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta cuando el usuario acepta la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult confirmarCoadmin(string email, string code)
        {
            if (VerificaUsuarioRegistrado(email) == false)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                //string actionURL = Url.Action("confirmarCoadmin", "AdminEquipos", new { email = email, code = code });
                //return RedirectToAction("Index", "Home", new { ReturnUrl = actionURL });
                //TempData["ReturnUrl"] = 

            }

            //Si el usuario inicio sesión con su cuenta, se procede a validar la información.
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            bool error = false;
            var Equipo = db.getEquipoByCoAdminCodeEmail(email, code);
            if (Equipo != null)
            {
                ViewBag.EquipoNombre = Equipo.equNombreEquipo;
                var usr = db.getUserByUserEmail(email);
                var usuPassword = ""; String usuarioId; 
                if (usr == null)
                {
                    ApplicationUser usuario = new ApplicationUser
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
                    var newUserAccount = new AccountController()._RegisterNewAccount(modelRegister, "");
                    if (newUserAccount)
                    {
                        ApplicationUser userId = db.getUserByUserEmail(email);
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
                        usuario = userId;
                    }
                    db.setClearEmailValidation(usuario);
                    usuarioId = usuario.Id;
                    var newUser = db.getUserByUserEmail(email);
                    enviarEmailConfirmacionCoadministracionEquipo(Equipo, newUser, newUser.Email, usuPassword);
                    //enviarEmailParticipanteAviso(jugador, user.Email, jugador.tblTorneo.torNombreTorneo, jugador.tblEquipo.equNombreEquipo, usuPassword);
                }

                var user = db.getUserByUserEmail(email);
                if (db.setEquipoConfirmCoadmin(UserManager, user, code))
                {
                    string usuarioNombre = email;
                    var prof = db.getUserMainProfile(user.Id);
                    if (prof == null)
                        prof = new schemaUsersProfiles();
                    usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    if (usuarioNombre == "")
                        usuarioNombre = user.Email;


                    enviarEmailAceptacionCoadministracionEquipo(Equipo, user, usuarioNombre);
                    ViewBag.UsuarioNombre = usuarioNombre;
                    SignInManager.SignIn(user, true, false);

                    var rolActual = user.usuRolActual;
                    var rolPlayer = db.getRoleByName(constClass.rolCoach);
                    if (rolPlayer.Id.ToUpper() != rolActual.ToUpper())
                    {
                        var rolUser = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolCoach.ToUpper());
                        if (!rolUser.Any())
                            UserManager.AddToRole(user.Id, constClass.rolCoach);
                        db.setCurrentUserRole(user.Id, rolPlayer.Id);
                    }

                    ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                }
                else
                    error = true;
            }
            else
                error = true;

            if(error)
            {
                ModelState.AddModelError(constClass.error, "Hubo un error aceptando la solicitud. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }

            return View("CoadminConfirmacion");
        }

        /// <summary>
        /// Accion que se ejecuta cuando el usuario rechaza la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult rechazarCoadmin(string email, string code)
        {
            bool error = false;

            var Equipo = db.getEquipoByCoAdminCodeEmail(email, code);
            if (Equipo != null)
            {
                if (db.setEquipoCoadmin_Delete(email, code))
                {
                    var user = db.getUserByUserEmail(email);
                    string usuarioNombre = email;
                    if (user != null)
                    {
                        var prof = db.getUserMainProfile(user.Id);
                        if (prof == null)
                            prof = new schemaUsersProfiles();
                        usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                        if (usuarioNombre == "")
                            usuarioNombre = user.Email;

                    }
                    ViewBag.UsuarioNombre = usuarioNombre;
                    ViewBag.EquipoNombre = Equipo.equNombreEquipo;

                    enviarEmailRechazoCoadministracionEquipo(Equipo, usuarioNombre);
                    ModelState.AddModelError(constClass.success, "La solicitud ha sido rechazada.");
                }
                else
                    error = true;
            }
            else
                error = true;

            if (error)
            {
                ModelState.AddModelError(constClass.error, "Hubo un error rechazando la solicitud. Redireccionando . . .");
                string rand = Global_Functions.getRandomString(10);
                ViewBag.jsScript = @"function jsRedirect_Home" + rand + @"(){
                                                document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                    @"}
                                            setTimeout(jsRedirect_Home" + rand + ",2500);";
            }

            return View("CoadminRechazo");
        }

        #endregion

        #region Funciones
        [NonAction]
        public bool enviarEmailCoachPerfilAviso(ApplicationUser user, string password,string equipo, string torneo,int equId,int torId)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
            var rechazarUrl = Url.Action("AdminEquipoRechazar", "Admin", new { email = user.Email, torId = torId, equId = equId }, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = user.Email;

                string body = Global_Functions.getBodyHTML("~/Emails/EmailCoachPerfilAviso.html");

                body = body.Replace("<%= NombreJugador %>", user.UserName);
                body = body.Replace("<%= NombreEquipo %>",equipo);
                body = body.Replace("<%= NombreTorneo %>",torneo);
                body = body.Replace("<%= usuario %>", user.Email);
                body = body.Replace("<%= password %>", password);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);
                body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Se le ha creado una cuenta Enligate", body,
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
        /// Envia el correo de confirmación para ser un coadministrador del equipo
        /// </summary>
        /// <param name="coAdmin"></param>
        /// <returns></returns>
        [NonAction]
        public bool EquipoSendCoAdministratorEmail(schemaEquiposCoAdministradores coAdmin, string adminId)
        {
            // Send an email with this link
            string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
            var callbackUrl = Url.Action("confirmarCoadmin", "AdminEquipos", new { email = coAdmin.ecaCorreoId, code = code }, protocol: Request.Url.Scheme);
            var rejectUrl = Url.Action("rechazarCoadmin", "AdminEquipos", new { email = coAdmin.ecaCorreoId, code = code }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            if (db.setEquipoCoadminConfirmCode_Update(coAdmin, code))
            {
                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    var equipo = db.getEquipoById(coAdmin.equId);
                    var profAdmin = db.getUserMainProfile(adminId);
                    if (profAdmin == null)
                        profAdmin = new schemaUsersProfiles();
                    string adminNombre = (profAdmin.uprNombres + " " + profAdmin.uprApellidos).Replace("-", "").Trim();
                    if (adminNombre == "")
                        adminNombre = db.getUserById(equipo.equUserIdCreador).UserName;

                    string body = Global_Functions.getBodyHTML("~/Emails/EquipoConfirmCoAdmin.html");
                    body = body.Replace("<%= NombreAdministrador %>", adminNombre.Trim());
                    body = body.Replace("<%= NombreEquipo %>", equipo.equNombreEquipo);
                    body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                    body = body.Replace("<%= UrlRejectCode %>", rejectUrl);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(coAdmin.ecaCorreoId, siteConfig.scoSenderDisplayEmailName, "Coadministrador de Equipo", body,
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
        /// Envia los correos de aceptacion de coadministracion de Equipo al dueño del Equipo.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailAceptacionCoadministracionEquipo(schemaEquipos Equipo, ApplicationUser userConfirmado, string usuarioNombre)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/EquipoCoadminAceptacion.html");
                body = body.Replace("<%= NombreUsuario %>", usuarioNombre);
                body = body.Replace("<%= NombreEquipo %>", Equipo.equNombreEquipo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(db.getUserById(Equipo.equUserIdCreador).Email, siteConfig.scoSenderDisplayEmailName, "Aceptación Coadministración de Liga", body,
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
        public bool enviarEmailConfirmacionCoadministracionEquipo(schemaEquipos Equipo, ApplicationUser userConfirmado, string usuarioNombre,string pass)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/EquipoCoadminConfirmacion.html");
                body = body.Replace("<%= NombreUsuario %>", usuarioNombre);
                body = body.Replace("<%= NombreEquipo %>", Equipo.equNombreEquipo);
                body = body.Replace("<%= password %>", pass);
                body = body.Replace("<%= usuario %>", userConfirmado.Email);
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
        /// Envia los correos de rechazo de coadministracion de liga al dueño de la liga.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailRechazoCoadministracionEquipo(schemaEquipos Equipo, string usuarioNombre)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            var emailTo = db.getUserById(Equipo.equUserIdCreador).Email + "," + Equipo.tblTorneos.tblUserCreador.Email + "," + Equipo.tblTorneos.tblLiga.tblUserCreador.Email;

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/EquipoCoadminRechazo.html");
                body = body.Replace("<%= NombreUsuario %>", usuarioNombre);
                body = body.Replace("<%= NombreEquipo %>", Equipo.equNombreEquipo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Rechazo Coadministración de Liga", body,
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

        public bool enviarEmailEliminarJugador(string jugCorreo,string equNombreEquipo)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string body = Global_Functions.getBodyHTML("~/Emails/AvisoEliminarJugador.html");
                body = body.Replace("<%= NombreUsuario %>", jugCorreo);
                body = body.Replace("<%= NombreEquipo %>", equNombreEquipo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(jugCorreo, siteConfig.scoSenderDisplayEmailName, "Dado de baja", body,
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

        #endregion

        #region Grid Busquedas

        //public ActionResult EquiposGrid(int tipoGrid)
        //{
        //    switch (tipoGrid)
        //    {
        //        case 0:
        //            ViewBag.EquiposGridAction = "_EquiposGrid_LigaDetalleAdmin";
        //            break;
        //        case 1:
        //            ViewBag.EquiposGridAction = "_EquiposGrid_EquiposAdmin";
        //            break;
        //    }
        //    return PartialView("Equipos/_EquiposGrid");
        //}

        //[ValidateInput(false)]
        //public ActionResult _EquiposGrid_LigaDetalleAdmin(int ligId, string ligKey, string torFiltroEstatus)
        //{
        //    var model = db.getEquiposParaGridByLiga(ligId, ligKey);

        //    if (torFiltroEstatus != null)
        //    {
        //        // 0 = Equipos activos
        //        // 1 = Equipos finalizados
        //        switch (int.Parse(torFiltroEstatus))
        //        {
        //            case 0:
        //                model = model.Where(t => t.torFechaFinal == null
        //                                      || t.torFechaFinal >= db.DateTimeMX().AddDays(-5))
        //                             .ToList();
        //                break;
        //            case 1:
        //                model = model.Where(t => t.torFechaFinal <= db.DateTimeMX().AddDays(-5))
        //                             .ToList();
        //                break;
        //        }
        //    }

        //    model.ForEach(t => t.torAdminEquipo = true);

        //    return PartialView("Equipos/_EquiposGrid", model);
        //}

        //[ValidateInput(false)]
        //public ActionResult _EquiposGrid_EquiposAdmin(string torFiltroEstatus)
        //{
        //    string userId = User.Identity.GetUserId();
        //    var model = db.getEquiposParaGridByUsuario(userId);

        //    if (torFiltroEstatus != null)
        //    {
        //        // 0 = Equipos activos
        //        // 1 = Equipos finalizados
        //        switch (int.Parse(torFiltroEstatus))
        //        {
        //            case 0:
        //                model = model.Where(t => t.torFechaFinal == null
        //                                      || t.torFechaFinal >= db.DateTimeMX().AddDays(-5))
        //                             .ToList();
        //                break;
        //            case 1:
        //                model = model.Where(t => t.torFechaFinal <= db.DateTimeMX().AddDays(-5))
        //                             .ToList();
        //                break;
        //        }
        //    }

        //    model.ForEach(t => t.torAdminEquipo = true);

        //    return PartialView("Equipos/_EquiposGrid", model);
        //}

        #endregion

        #endregion

        /// <summary>
        /// Regresa la pantalla de Index para mostrar el dashboard del rol coach / Adnub de equipo.
        /// </summary>
        /// <returns></returns>
        public ActionResult Dashboard()
        {
            return View("Index");
        }

        /// <summary>
        /// Realiza la carga de la página del perfil
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Perfil()
        {
            ProfileViewModel model = new AdminController().getPerfilUsuario(User.Identity.Name);

            return View("Perfil", model);
        }

        public ActionResult Calendario()
        {
            var user = db.getUserById(User.Identity.GetUserId());
            var partido = db.getPartidosEquipos(user);

            var model = new AdminLigasController().filtros_Calendario(user);

            model.numPartidos = partido.Count;

            return View(model);
        }

        public ActionResult Calendario_ChangeLiga(int? ligId = null, int? torId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var partido = db.getPartidosEquipos(user);

            var model = new AdminLigasController().filtros_Calendario(user, ligId, torId);

            return PartialView("Ligas/_CalendarioNuevo_Filtros", model);
        }
        public ActionResult Historial()
        {
            return View();
        }

        public ActionResult MisPerfiles()
        {
            return View();
        }

        public ActionResult MisEquipos()
        {            
            return View();
        }
        public ActionResult MisTorneos()
        {
            return View();
        }
        public ActionResult MisPagos()
        {
            return View();
        }

        #region Funciones Generales

        /// <summary>
        /// Verifica si un usuario ya existe.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [NonAction]
        public bool VerificaUsuarioRegistrado(string email)
        {
            //Valida que la persona que está confirmando la cuenta exista y tenga la sesión iniciada.
            bool usuarioRegistrado = true;
            var usr = db.getUserByUserEmail(email);
            if (usr == null)
                usuarioRegistrado = false;
            else
                if (usr.Id != User.Identity.GetUserId())
                    usuarioRegistrado = false;

            return usuarioRegistrado;
        }

        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private void AddError(string error)
        {
            ModelState.AddModelError("", error);
        }

        private string RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            return Url.Action("Index", "Home"); //, new { controller = "Home", action = "Index" });
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private string GetRedirectUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return Url.Action("Index", "Home");
            }

            return returnUrl;
        }
        #endregion

        public ActionResult _Equipos_Grid()
        {            
            return PartialView("Equipos/_equiposGrid");
        }
        public ActionResult _Equipos_Grid_Player()
        {
            return PartialView("Equipos/_equiposGrid_player");
        }
        public JsonResult _RealizarPago(int equId)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            var datosUsuario = db.getUserProfile(user);

            var equipo = db.getEquipoById(equId);

            var cust = from e in datosUsuario
                       select new
                       {
                           equId = equipo.equId,
                           equNombre = equipo.equNombreEquipo,
                           userId = user.Id,
                           fname = e.fname,
                           mname = e.mname,
                           email = user.Email,
                           phone = e.phone,
                           addr = e.addr,
                           city = e.city,
                           state = e.state,
                           country = e.country,
                           zip = e.zip,
                           total = equipo.equPrecioTorneo,
                       };

            var rows = cust.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _DetallesPagoEqu(int equId)
        {
            var pagos = db.getDetallesPagosEquipos(equId);
            var rows = pagos.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _DetallesPagoEquJug(int equId)
        {
            var pagos = db.getDetallesPagosEquiposJug(equId);
            var rows = pagos.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _PagosGrid_Callback()
        {
            var user = User.Identity.GetUserId();
            var role = db.getUserById(user);
            var rolJug = db.getRoleByName(constClass.rolPlayer);
            var rolEqu = db.getRoleByName(constClass.rolCoach);
            var model = new List<PagosGridViewModel>();
            if (role.usuRolActual.ToUpper() == rolJug.Id.ToUpper())
            {
                model = db.getGridPagosJugador().Where(l=> l.userId==user).ToList();               
            }
            else if(role.usuRolActual.ToUpper() == rolEqu.Id.ToUpper()) {
                model = db.getGridPagosEquipo().Where(l => l.userId == user).ToList();
            }
            
            return PartialView("Banwire/_GridPagosEquipo", model);
        }
        [ValidateInput(false)]
        public ActionResult _Equipos_Grid_Callback(int torId)
        { 
            var user = User.Identity.GetUserId();
            var rol = db.getUserById(user);
            var rolName = db.getRoles();
            var name = rolName.Where(l=> l.rolId.ToUpper() == rol.usuRolActual.ToUpper())
                .FirstOrDefault().rolName;
            if (name==constClass.rolCoach)
            {
                ViewData["deleteTeam"] = false;
                var equAdm = db.getEquipoByAdmin(user).Where(l=> l.equEstatus==true && l.equDelete == false).OrderBy(l=> l.tblTorneos.torNombreTorneo);
                return PartialView("Equipos/_equiposGrid", equAdm);
            }
            if (name == constClass.rolPlayer)
            {
                ViewData["deleteTeam"] = false;
                var equAdm = db.getEquipoByPlayer(user).Where(l => l.equEstatus == true && l.equDelete == false);
                return PartialView("Equipos/_equiposGrid_player", equAdm);
            }
            else
            {
                ViewData["deleteTeam"] = true;
                var model = db.getEquipoByTorneo(torId).Where(l => l.equEstatus = true && l.equDelete == false).OrderBy(l => l.equFechaCreacionUTC);
                var torneo = db.getTorneoById(torId);
                if (torneo != null)
                {
                    var lista = new List<schemaEquipos>();
                    if (torneo.torEsCoaching)
                    {
                        ViewBag.EsCoaching = true;
                        if (model.Count()>0)
                        {
                            lista.Add(model.FirstOrDefault());
                            return PartialView("Equipos/_equiposGrid", lista);
                        }else
                        {
                            return PartialView("Equipos/_equiposGrid", model);
                        }
                    }
                }
                
                return PartialView("Equipos/_equiposGrid", model);
            }            
        }
        [HttpPost]
        public ActionResult _Jugadores_Grid(int equId, int torId)
        {
            ViewBag.torId = torId;
            ViewBag.equId = equId;
            return PartialView("Jugadores/_jugadoresGrid");
        }
        public ActionResult _Jugadores_Grid_Details()
        {
            return PartialView("Jugadores/_JugadoresGridCallback");
        }

        [ValidateInput(false)]
        public ActionResult _Jugadores_Grid_Callback(int torId, int equId)
        {
            var model = db.getJugadoresByTorneo_EquipoId(torId, equId);
            return PartialView("Jugadores/_JugadoresGridCallback", model);
        }
        [HttpPost]
        public JsonResult _Validate_Register_TeamTournament(int torId)
        {
            var resultJson = new JsonResultViewModel();
            bool value = false;
            var tournament = db.getTorneoById(torId);
            var total_equipos = db.getEquipoByTorneo(torId).Where(l=> l.equDelete == false).ToList().Count;

            if (total_equipos < tournament.torNumeroEquipos)
                value = true;
            else
            {
                ModelState.AddModelError(constClass.info, "Se ha cubierto la capacidad máxima de equipos por torneo.");
                resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
            }
            return Json(new { value = value, msg = resultJson });
        }
        public JsonResult _Equipos_Delete(int equId)
        {
            var result = new { Success = "true", Message = "Success" };
            try
            {
                if (!db.setDeleteTeam(equId))
                {
                    result.Success.Replace("true","false");
                    result.Message.Replace("Success", "Error to delete team");
                }
            }
            catch (Exception ex)
            {

                result.Success.Replace("true", "false");
                result.Message.Replace("Success", "Error to delete team");
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getTeamCoaching(int torId)
        {
            var team = db.getEquipoCoachingByTorneo(torId,User.Identity.GetUserId()).Select(s=> s.equNombreEquipo).ToList().ToArray();
            return Json(team, JsonRequestBehavior.AllowGet);

        }
    }
}