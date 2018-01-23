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
using System.Net;
using Microsoft.Owin.Security;

namespace sw_EnligateWeb.Controllers
{
    public class AdminTorneosController : MyBaseController
    {
        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminTorneosController()
        {

        }

        public AdminTorneosController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        /// Redirecciona a la pantalla principal del administrador de torneos.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
            //return RedirectToAction("Torneos");
        }

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

        #region Categorias

        ///// <summary>
        ///// Muestra la pantalla de categorias de torneos
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult TorneoCategorias()
        //{
        //    return View();
        //}

        #region Grid y Crear Categorias

        /// <summary>
        /// Regresa la vista del grid de edicion de las categorias de un torneo
        /// </summary>
        /// <returns></returns>
        public ActionResult CategoriasGridEdit()
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            _CategoriasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CategoriasGridEdit");
        }

        /// <summary>
        /// Funcion que llena los ViewData que utiliza el Grid de Categorias para llenar los combobox
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        protected void _CategoriasGridEdit_EditViewData(string userId, string roleId)
        {
            ViewData["cmbLigas"] = db.getLigasActivasByUser(db.getUserLeagues(userId, roleId));
            var deportes = db.getDeportes_Active();
            deportes.ForEach(d => d.depNombre = d.depNombre.ToUpper());
            ViewData["cmbDeportes"] = deportes;
            ViewData["cmbCategorias"] = db.getTiposTorneo_Active();
        }

        /// <summary>
        /// Accion que se activa cuando el Grid hace Callback
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _CategoriasGridEdit_Callback()
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCategoriasTorneos(user.Id, user.usuRolActual);
            _CategoriasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CategoriasGridEdit", model);
        }

        /// <summary>
        /// Cuando vienen los parametros del Guardar o Actualizar del grid, vienen con " ej: "hola", esta funcion la elimina.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected TorneoCategoriasViewModel clearGridCommasCategorias(TorneoCategoriasViewModel item)
        {
            item.lctNombre = (item.lctNombre != null) ? item.lctNombre.Replace("\"", "").Trim() : null;
            item.depNombre = (item.depNombre != null) ? item.depNombre.Replace("\"", "").Trim() : null;
            //item.lctGenero = item.lctGenero.Replace("\"", "").Trim();
            item.lctDescripcion = (item.lctDescripcion != null) ? item.lctDescripcion.Replace("\"", "").Trim() : null;
            return item;
        }

        /// <summary>
        /// Accion que se ejecuta cuando se llama al guardar un nuevo registro desde el grid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _CategoriasGridEdit_AddNewPartial(TorneoCategoriasViewModel item)
        {
            item = clearGridCommasCategorias(item);
            if (ModelState.IsValid)
            {
                if (db.setLigaCategoriasTorneos_Add(item))
                {
                    ModelState.Clear();
                    // ViewData["Success"] = "La categoria se guardo exitosamente !";
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

            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCategoriasTorneos(user.Id, user.usuRolActual);
            _CategoriasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CategoriasGridEdit", model);
        }


        /// <summary>
        /// Accion que se ejecuta cuando se llama al actualizar un nuevo registro desde el grid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _CategoriasGridEdit_UpdatePartial(TorneoCategoriasViewModel item)
        {
            item = clearGridCommasCategorias(item);
            if (ModelState.IsValid)
            {
                if (db.setLigaCategoriasTorneos_Edit(item))
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

            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCategoriasTorneos(user.Id, user.usuRolActual);
            _CategoriasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CategoriasGridEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se llama al confirmar eliminar un registro desde el grid.
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _CategoriasGridEdit_Delete(int lctId)
        {
            if (db.setLigaCategoriasTorneos_Delete(lctId))
            {
                ModelState.Clear();
                //ViewData["gvtCategoriasCallback"] = "ok";
            }
            else
            {
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
            }

            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCategoriasTorneos(user.Id, user.usuRolActual);
            _CategoriasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CategoriasGridEdit", model);
        }
        public ActionResult CategoryForm(int? ligId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var model = new CategoriaViewModel();
            var role = db.getRoles().Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper());

            if (role != null)
            {
                var rolName = role.FirstOrDefault().rolName;
                if (rolName == constClass.rolOwners)
                {
                    var liga = db.getUserLeagues(user.Id, user.usuRolActual).Where(l => l.ligId == ligId).FirstOrDefault();
                    model.ddlLigas.Add(new SelectListItem { Text = liga.ligNombre.ToUpper(), Value = liga.ligId.ToString() });

                }
                else if (rolName == constClass.rolAdminTorneos)
                {
                    var torneos = db.getTorneosByUser(user.Id);
                    if (torneos != null)
                    {
                        var lista = new List<SelectListItem>();
                        foreach (var item in torneos)
                        {
                            var ligas = db.getLigaById(item.ligId);
                            var ligaList = new SelectListItem { Text = ligas.ligNombreLiga.ToUpper(), Value = ligas.ligId.ToString() };
                            lista.Add(ligaList);
                        }
                        model.ddlLigas = lista;
                    }
                }
            }
            if (ligId != null)
            {
                var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                if (ligaSeleccionada != null)
                {
                    ligaSeleccionada.Selected = true;
                    model.ligId = (int)ligId;
                }
            }
            var deportes = db.getDeportes_Active();

            model.ddlDeporte.Add(new SelectListItem { Text = "SELECCIONA  DEPORTE", Value = "0" });
            model.ddlCategorias.Add(new SelectListItem { Text = "SELECCIONA  CATEGORIA", Value = "0" });

            model.ddlDeporte.AddRange(deportes.Select(l => new SelectListItem { Text = l.depNombre, Value = l.depNombre }).ToList());
            model.ddlCategorias.AddRange(db.getTiposTorneo_Active().Select(l => new SelectListItem { Text = l.ttoNombre, Value = l.ttoId.ToString() }).ToList());

            return PartialView("Torneos/_FormNewCategory", model);
        }
        #endregion

        #endregion

        #region Canchas

        ///// <summary>
        ///// Regresa la vista para ver las canchas del torneo.
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult TorneoCanchas()
        //{
        //    return View();
        //}

        #region Grid y Crear Canchas

        /// <summary>
        /// Regresa la vista del grid de edicion de las Canchas de un torneo
        /// </summary>
        /// <returns></returns>
        public ActionResult CanchasGridEdit()
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            _CanchasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CanchasGridEdit");
        }

        /// <summary>
        /// Funcion que llena los ViewData que utiliza el Grid de Canchas para llenar los combobox
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        protected void _CanchasGridEdit_EditViewData(string userId, string roleId)
        {
            ViewData["cmbLigas"] = db.getLigasActivasByUser(db.getUserLeagues(userId, roleId));
            var deportes = db.getDeportes_Active();
            deportes.ForEach(d => d.depNombre = d.depNombre.ToUpper());
            ViewData["cmbDeportes"] = deportes;
        }

        public ActionResult canchasEditCallback(int? canId = null, int? ligId = null)
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = new TorneoCanchasViewModel();

            model.ddlLigas = db.getLigasActivasByUser(db.getUserLeagues(userId, user.usuRolActual))
                .Select(l => new SelectListItem { Text = l.ligNombreLiga.ToUpper(), Value = l.ligId.ToString() }).ToList();
            model.ddlDeporte = db.getDeportes_Active().Select(l => new SelectListItem { Text = l.depNombre.ToUpper(), Value = l.depNombre.ToString() }).ToList();
            if (canId != null)
            {
                var item = db.getLigasCanchasTorneos(userId, user.usuRolActual).Where(l => l.lcatId == canId).ToList().FirstOrDefault();
                model.lcatdomicilio = item.lcatdomicilio;
                model.lcatCodigoPostal = item.lcatCodigoPostal;

                model.lcatColonia = item.lcatColonia;
                model.lcatDescripcion = item.lcatDescripcion;
                model.lcatDomicilio = item.lcatDomicilio;
                model.lcatEstado = item.lcatEstado;
                model.lcatId = item.lcatId;
                model.lcatLatitud = (item.lcatLatitud == "") ? null : item.lcatLatitud;
                model.lcatLongitud = (item.lcatLongitud == "") ? null : item.lcatLongitud;
                model.lcatMunicipio = item.lcatMunicipio;
                model.lcatNombre = item.lcatNombre;
                model.lcatNumExtInt = item.lcatNumExtInt;
                model.ligaNombre = item.ligaNombre;

                var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == item.ligId.ToString());
                if (ligaSeleccionada != null)
                {
                    ligaSeleccionada.Selected = true;
                    model.ligId = item.ligId;
                }

                model.edit = true;
            }
            if (ligId != null)
            {
                var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                if (ligaSeleccionada != null)
                {
                    ligaSeleccionada.Selected = true;
                    model.ligId = (int)ligId;
                }
            }


            return PartialView("Ligas/_TorneoCanchaEdit", model);
        }

        /// <summary>
        /// Accion que se activa cuando el Grid hace Callback
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _CanchasGridEdit_Callback()
        {
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCanchasTorneos(user.Id, user.usuRolActual);
            _CanchasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CanchasGridEdit", model);
        }

        /// <summary>
        /// Cuando vienen los parametros del Guardar o Actualizar del grid, vienen con " ej: "hola", esta funcion la elimina.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected TorneoCanchasViewModel clearGridCommasCanchas(TorneoCanchasViewModel item)
        {
            item.lcatNombre = item.lcatNombre.Replace("\"", "").Trim();
            item.depNombre = item.depNombre.Replace("\"", "").Trim();
            item.lcatDescripcion = item.lcatDescripcion.Replace("\"", "").Trim();
            item.lcatDireccion = item.lcatDireccion.Replace("\"", "").Trim();
            return item;
        }

        /// <summary>
        /// Accion que se ejecuta cuando se llama al guardar un nuevo registro desde el grid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public JsonResult _Canchas_AddNewPartial(int ligId, string lcatNombre, string lcatDescripcion, string lcatdomicilio = null, string lcatNumExtInt = null, string lcatColonia = null, string lcatMunicipio = null, string lcatEstado = null, string lcatCodigoPostal = null, string lcatLatitud = null, string lcatLongitud = null, bool edit = false, int? lcatId = null)
        {
            var item = new TorneoCanchasViewModel();

            item.ligId = ligId;
            item.lcatNombre = lcatNombre;
            item.lcatDescripcion = lcatDescripcion;
            item.lcatdomicilio = lcatdomicilio;
            item.lcatNumExtInt = lcatNumExtInt;
            item.lcatColonia = lcatColonia;
            item.lcatMunicipio = lcatMunicipio;
            item.lcatEstado = lcatEstado;
            item.lcatCodigoPostal = lcatCodigoPostal;
            item.lcatLatitud = lcatLatitud;
            item.lcatLongitud = lcatLongitud;
            item.edit = (edit) ? true : false;
            if (lcatId != null)
            {
                item.lcatId = (int)lcatId;
            }
            //item = clearGridCommasCanchas(item);
            if (ModelState.IsValid)
            {
                if (db.setLigaCanchasTorneos_Add(item))
                {
                    ModelState.Clear();
                }
                else
                {
                    ViewData["EditError"] = "Hubo un error guardando la Cancha";
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
            }


            return Json("success");
        }

        public ActionResult _Canchas_NewPartial()
        {
            return PartialView("canchasEditCallback", "AdminTorneos");
        }
        /// <summary>
        /// Accion que se ejecuta cuando se llama al actualizar un nuevo registro desde el grid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _CanchasGridEdit_UpdatePartial(TorneoCanchasViewModel item)
        {
            item = clearGridCommasCanchas(item);
            if (ModelState.IsValid)
            {
                if (db.setLigaCanchasTorneos_Edit(item))
                {
                    ModelState.Clear();
                }
                else
                {
                    ViewData["EditError"] = "Hubo un error guardando la Cancha";
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
            }

            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCanchasTorneos(user.Id, user.usuRolActual);
            _CanchasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CanchasGridEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se llama al confirmar eliminar un registro desde el grid.
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _CanchasGridEdit_Delete(int lcatId)
        {
            if (db.setLigaCanchasTorneos_Delete(lcatId))
            {
                ModelState.Clear();
                //ViewData["gvtCanchasCallback"] = "ok";
            }
            else
            {
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
            }

            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            var model = db.getLigasCanchasTorneos(user.Id, user.usuRolActual);
            _CanchasGridEdit_EditViewData(user.Id, user.usuRolActual);
            return PartialView("Torneos/_CanchasGridEdit", model);
        }

        #endregion

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

        public ActionResult TorneoNuevo()
        {
            return View();
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

        #region Crear/Editar Torneos

        #region Funciones

        /// <summary>
        /// Obtiene las ligas que pertenecen al usuario
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> _TorneoNuevo_GetDropDownListLigas(int ligId, bool? edit = false)
        {
            var ddl = new List<SelectListItem>();
            if (edit == true)
            {
                var liga = db.getLigaById(ligId);
                if (liga != null)
                {
                    ddl.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                    return ddl;
                }
            }
            else
            {
                var user = db.getUserById(User.Identity.GetUserId());
                if (user.usuRolActual.ToUpper() == db.getRoleByName(constClass.rolAdminTorneos).Id.ToUpper())
                {
                    var liga = db.getLigaById(ligId);
                    if (liga != null)
                    {
                        ddl.Add(new SelectListItem { Text = liga.ligNombreLiga.ToUpper(), Value = liga.ligId.ToString() });
                        return ddl;
                    }
                }

                return db.getUserLeagues(user.Id, user.usuRolActual)
                         .Select(l => new SelectListItem { Text = l.ligNombre.ToUpper(), Value = l.ligId.ToString() })
                         .ToList();
            }

            ddl.Add(new SelectListItem { Text = "--Seleccione--", Value = "0" });
            return ddl;
        }

        /// <summary>
        /// Obtiene la lista de categorias dependiendo de la liga.
        /// </summary>
        /// <param name="ligId"></param>
        /// <returns></returns>
        protected List<SelectListItem> _TorneoNuevo_GetDropDownListCategorias(int ligId)
        {
            return db.getLigaCategoriasTorneosActivosByLigaId(ligId)
                     .Select(l => new SelectListItem { Text = l.depNombre.ToUpper() + " - " + l.lctNombre.ToUpper(), Value = l.lctId.ToString() })
                     .OrderBy(l => l.Text)
                     .ToList();
        }

        /// <summary>
        /// Regresa la lista de estructuras del torneo.
        /// </summary>
        /// <param name="esDeporteEnEquipo"></param>
        /// <returns></returns>
        protected List<schemaTorneoEstructura> _TorneoNuevo_GetTorneoEstructuras(bool esDeporteEnEquipo)
        {
            return db.getTorneoEstructurasByTipoDeporte(esDeporteEnEquipo)
                     .OrderBy(l => l.tscNombre)
                     .ToList();
        }

        /// <summary>
        /// Regresa la lista de los tipos de pago de la liga.
        /// </summary>
        /// <param name="tcfppId"></param>
        /// <returns></returns>
        protected List<schemaTarifasCfppTiposPago> _TorneoNuevo_GetTarifasTiposPago(int tcfppId)
        {
            return db.getTarifasTiposPagoByPeriodicidad(tcfppId);
        }

        /// <summary>
        /// Regresa la lista de las tarifas y metodos de pago de las tarifas de pago de la liga o
        /// del torneo si ya es edicion y se esta cargando lo que se guardo.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="tcfpptpId"></param>
        /// <returns></returns>
        protected List<TorneosNuevoEditMetodosPagoViewModel> _TorneoNuevo_GetTarifas(int torId, int tcfpptpId)
        {
            if (torId > 0)
            {
                // Si el torneo esta en edicion, regresa las tarifas con las que se guardo
                // y el tipo de pago coincide regresa las tarifas ya guardadas.
                var tar = db.getFeesByTorneoId(torId);
                if (tar.Count > 0 && tar.Count(t => t.tblTarifas.tblTarifasCfpptpMetodoPago.tcfpptpId == tcfpptpId) > 0)
                    return tar.Where(t => t.tblTarifas.tblTarifasCfpptpMetodoPago.tcfpptpId == tcfpptpId)
                              .Select(l => new TorneosNuevoEditMetodosPagoViewModel
                              {
                                  tarId = l.tarId,
                                  tarCosto = l.tblTarifas.tarCosto,
                                  tmpIdMetodoPago = l.tblTarifas.tblTarifasCfpptpMetodoPago.tmpIdMetodoPago,
                                  tarHabilitado = l.ttaHabilitado
                              })
                              .ToList();
            }

            var tarifas = db.getTarifasByTarifasCfppTiposPagoId(tcfpptpId);
            return tarifas.Select(l => new TorneosNuevoEditMetodosPagoViewModel
            {
                tarId = l.tarId,
                tarCosto = l.tarCosto,
                tmpIdMetodoPago = l.tblTarifasCfpptpMetodoPago.tmpIdMetodoPago,
                tarHabilitado = true
            }).ToList();
        }

        /// <summary>
        /// Prepara todo el modelo para ser mostrado por primera vez para crear un nuevo torneo
        /// </summary>
        /// <returns></returns>
        protected TorneosViewModel _TorneoNuevo_ViewModel()
        {
            var user = db.getUserById(User.Identity.GetUserId());
            var model = new TorneosViewModel();

            //Ligas que pertenecen al usuario
            model.ddlLigas = _TorneoNuevo_GetDropDownListLigas(0);
            if (model.ddlLigas.Count() > 0)
            {
                var ddlLiga = model.ddlLigas.First();
                ddlLiga.Selected = true;
                model.ligId = int.Parse(ddlLiga.Value);

                model = _TorneoNuevo_ViewModel_DatosTorneo(model, model.ligId, true, true);
                model = _TorneoNuevo_ViewModel_DatosPago(model, true);
            }
            model.torFechaInicio = db.DateTimeMX();
            return model;
        }

        /// <summary>
        /// Carga los elementos necesarios para mostrar la parte de los datos del torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ligId"></param>
        /// <param name="updateCategoria"></param>
        /// <param name="updateAddress"></param>
        /// <returns></returns>
        protected TorneosViewModel _TorneoNuevo_ViewModel_DatosTorneo(TorneosViewModel model, int ligId, bool updateCategoria, bool updateAddress)
        {
            model.ddlCategorias = _TorneoNuevo_GetDropDownListCategorias(ligId);
            if (model.ddlCategorias.Count() > 0)
            {
                if (updateCategoria)
                {
                    var item = model.ddlCategorias.First();
                    item.Selected = true;
                    model.lctId = int.Parse(item.Value);
                }
                var deporte = db.getDeportaByLigaCategoriasTorneosId(model.lctId);
                model.torDeporteEnEquipo = deporte.depEnEquipo;
                model.listTorneoEstructuras = _TorneoNuevo_GetTorneoEstructuras(model.torDeporteEnEquipo);
            }

            if (updateAddress)
            {
                var ligaDireccion = db.getLastBusinessAddressByLeagueId(ligId);
                model.tblTorneoDireccion.ldcCalle = ligaDireccion.ldcDomicilio;
                model.tblTorneoDireccion.ldcNumeroExtInt = ligaDireccion.ldcNumeroExtInt;
                model.tblTorneoDireccion.ldcColonia = ligaDireccion.ldcColonia;
                model.tblTorneoDireccion.ldcMunicipio = ligaDireccion.ldcMunicipio;
                model.tblTorneoDireccion.ldcEstado = ligaDireccion.ldcEstado;
                model.tblTorneoDireccion.ldcCodigoPostal = ligaDireccion.ldcCodigoPostal;

                model.torCorreoContacto = ligaDireccion.tblLigas.ligCorreoContacto;
                model.torNumeroContacto = ligaDireccion.tblLigas.ligTelefonoContacto;
            }

            return model;
        }

        /// <summary>
        /// Carga los elementos necesarios para mostrar la parte de los datos de pago del torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateListTarifas"></param>
        /// <returns></returns>
        protected TorneosViewModel _TorneoNuevo_ViewModel_DatosPago(TorneosViewModel model, bool updateListTarifas)
        {
            var liga = db.getLigaById(model.ligId);
            model.torLigaFormaPago = "COMISION";
            model.torLigaDescuento = liga.ligPorcentajeDescuento;
            model.listTarifasCfppTiposPago = _TorneoNuevo_GetTarifasTiposPago(4);
            if (model.listTarifasCfppTiposPago.Count > 0)
            {
                if (model.tcfpptpId == 0)
                    model.tcfpptpId = model.listTarifasCfppTiposPago.First().tcfpptpId;
                model.tblTarifasCfppTiposPago = model.listTarifasCfppTiposPago.FirstOrDefault(t => t.tcfpptpId == model.tcfpptpId);
            }
            if (updateListTarifas)
                model.listTarifas = _TorneoNuevo_GetTarifas(model.torId, model.tcfpptpId);
            else
                model.listTarifas = _TorneoNuevo_GetTarifas(0, model.tcfpptpId);

            return model;
        }

        /// <summary>
        /// Carga los elementos necesarios de todo el torneo, cuando se actualiza o carga la informacion.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateCategoria"></param>
        /// <param name="updateAddress"></param>
        /// <param name="updateListTarifas"></param>
        /// <returns></returns>
        protected TorneosViewModel _TorneoNuevo_FillPostModel(TorneosViewModel model, bool updateCategoria, bool updateAddress, bool updateListTarifas, bool? edit = false)
        {
            if ((bool)edit)
                model.ddlLigas = _TorneoNuevo_GetDropDownListLigas(model.ligId, edit);
            else
                model.ddlLigas = _TorneoNuevo_GetDropDownListLigas(model.ligId);
            var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == model.ligId.ToString());
            if (ligaSeleccionada != null)
                ligaSeleccionada.Selected = true;

            if (model.ligId > 0)
            {
                model = _TorneoNuevo_ViewModel_DatosTorneo(model, model.ligId, updateCategoria, updateAddress);
                model = _TorneoNuevo_ViewModel_DatosPago(model, updateListTarifas);
            }

            return model;
        }

        #endregion

        #region Acciones

        /// <summary>
        /// Accion que se ejecuta cuando se ingresa a la pantalla de crear nuevo torneo.
        /// </summary>
        /// <returns></returns>
        public ActionResult _TorneoNuevo(int ligId)
        {
            ModelState.Clear();
            var model = _TorneoNuevo_ViewModel();
            if (ligId > 0)
            {
                model.ligId = ligId;
                model = _TorneoNuevo_FillPostModel(model, true, true, true);
            }

            model.torPrecioTorneo = (model.torPrecioTorneo != null) ? model.torPrecioTorneo : 0;
            model.torDiasParaPago = (model.torDiasParaPago != null) ? model.torDiasParaPago : 0;
            ViewBag.haveCoachingTeam = false;
            return PartialView("Torneos/_NuevoEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se cambia el tipo de torneo (interno/coaching)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _TorneoNuevoEdit_ChangeTipo(TorneosViewModel model)
        {
            ModelState.Clear();
            model = _TorneoNuevo_FillPostModel(model, false, false, false);

            var result = new TorneosNuevoEditPostbackViewModel
            {
                datosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Datos", model),
                estructuraPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Estructura", model),
                contactoPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Contacto", model),
                pagosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Pagos", model)
            };

            return Json(result);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se cambia la liga del torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _TorneoNuevoEdit_ChangeLiga(TorneosViewModel model)
        {
            ModelState.Clear();
            model = _TorneoNuevo_FillPostModel(model, true, true, true);

            var result = new TorneosNuevoEditPostbackViewModel
            {
                datosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Datos", model),
                estructuraPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Estructura", model),
                contactoPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Contacto", model),
                pagosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Pagos", model)
            };

            return Json(result);
        }

        /// <summary>
        /// Accion que se ejecuta cuando se cambia la categoria del torneo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _TorneoNuevoEdit_ChangeLigaCategoria(TorneosViewModel model)
        {
            ModelState.Clear();
            model = _TorneoNuevo_FillPostModel(model, false, false, false);

            var result = new TorneosNuevoEditPostbackViewModel
            {
                datosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Datos", model),
                estructuraPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Estructura", model),
                contactoPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Contacto", model),
                pagosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Pagos", model)
            };

            return Json(result);
        }

        /// <summary>
        /// Se ejecuta cuando cambia el precio del torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _TorneoNuevoEdit_ChangePagoPrecio(TorneosViewModel model)
        {
            ModelState.Clear();
            model = _TorneoNuevo_FillPostModel(model, false, false, true);

            var result = new TorneosNuevoEditPostbackViewModel
            {
                pagosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Pagos", model)
            };

            return Json(result);
        }

        /// <summary>
        /// Accion que se ejecuta cuando cambio el tipo de pago del torneo si es comisión.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _TorneoNuevoEdit_ChangePagoTipo(TorneosViewModel model)
        {
            ModelState.Clear();
            model = _TorneoNuevo_FillPostModel(model, false, false, true);

            var result = new TorneosNuevoEditPostbackViewModel
            {
                pagosPartial = RenderPartialViewToString("Torneos/_NuevoEdit_Pagos", model)
            };

            return Json(result);
        }

        /// <summary>
        /// Accion que guarda un nuevo torneo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _TorneoNuevoEdit_GuardarNuevo(TorneosViewModel model, string teamName)
        {
            bool error = false;
            //if (!error)
            //    return RedirectToAction("_TorneoNuevoEditar", new { Id = 3, usrId = "c64a3d58-b751-4f82-855b-62823401ae69" });
            ViewBag.haveCoachingTeam = false;
            var valTorneo = db.getTorneoByName(model.torNombreTorneo);

            var adminTorneo = db.getTorneosByUser(User.Identity.GetUserId());

            if (valTorneo == null || model.torId != null)
            {

                model.tblTarifasCfppTiposPago = db.getTTarifasCfppTiposPagoById(model.tcfpptpId);
                if (model.torTipo == constClass.torTipoInterno)
                {
                    //Interno
                    if (!model.torDeporteEnEquipo)
                    {
                        ModelState.Remove("torNumeroJuegos");
                        ModelState.Remove("torNumeroEquipos");
                        ModelState.Remove("torPuntosGanar");
                        ModelState.Remove("torPuntosEmpatar");
                        ModelState.Remove("torPuntosPerder");
                    }
                }
                else
                {
                    //Coaching
                    ModelState.Remove("torNumeroJuegos");
                    ModelState.Remove("torNumeroEquipos");
                    ModelState.Remove("torPuntosGanar");
                    ModelState.Remove("torPuntosEmpatar");
                    ModelState.Remove("torPuntosPerder");
                }

                if (model.torLigaFormaPago.ToUpper() != "COMISION")
                {
                    ModelState.Remove("ttpIdTipoPago");
                    ModelState.Remove("listTarifas");
                }
                else
                {
                    if (model.listTarifas == null || model.listTarifas.Count() == 0)
                    {
                        ModelState.AddModelError(constClass.error, "Debes de seleccionar al menos un método de pago.");
                        error = true;
                    }
                    else
                    {
                        if (model.tblTarifasCfppTiposPago.ttpIdTipoPago != null && model.tblTarifasCfppTiposPago.ttpIdTipoPago == "ANTICIPO")
                        {
                            model.listTarifas.ForEach(t => t.tarHabilitado = true);
                        }
                        else
                        {
                            if (model.listTarifas.Count(t => t.tarHabilitado == true) == 0)
                            {
                                ModelState.AddModelError(constClass.error, "Debes de seleccionar al menos un método de pago.");
                                error = true;
                            }
                        }
                    }
                }

                if (ModelState.IsValid && !error)
                {
                    //Guardar la imagen
                    foreach (string file in Request.Files)
                    {
                        string filename = "";
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(fileContent.FileName);
                            if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                            {
                                string imgTorneo = model.torImgUrl;
                                model.torImgUrl = null;

                                string urlPath = Server.MapPath(constClass.urlTorneosImagenes);
                                if (!System.IO.Directory.Exists(urlPath))
                                    System.IO.Directory.CreateDirectory(urlPath);

                                filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + fileContent.FileName;
                                fileContent.SaveAs(filename);
                                bool savedFile = System.IO.File.Exists(filename);

                                if (savedFile)
                                {
                                    model.torImgUrl = filename;
                                    if (System.IO.File.Exists(imgTorneo))
                                        System.IO.File.Delete(imgTorneo);
                                }
                                else
                                {
                                    ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen del torneo.");
                                    model.torImgUrl = imgTorneo;
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                            }
                        }
                    }

                    schemaTorneos torneo = new schemaTorneos();
                    torneo.torTipo = model.torTipo;
                    torneo.torImgUrl = model.torImgUrl;
                    torneo.torComentarios = model.torComentarios;
                    torneo.torNombreTorneo = model.torNombreTorneo;
                    torneo.ligId = model.ligId;
                    torneo.lctId = model.lctId;
                    torneo.torFechaInicio = model.torFechaInicio;
                    torneo.torFechaTermino = model.torFechaTermino;
                    torneo.torFechaLimiteInscripcion = model.torFechaLimiteInscripcion;
                    torneo.torNumeroJuegos = model.torNumeroJuegos;
                    torneo.torNumeroEquipos = model.torNumeroEquipos;
                    torneo.torMaxJugadoresEquipo = model.torMaxJugadoresEquipo;
                    torneo.torPuntosGanar = model.torPuntosGanar;
                    torneo.torPuntosEmpatar = model.torPuntosEmpatar;
                    torneo.torPuntosPerder = model.torPuntosPerder;
                    torneo.tesId = model.tesId;
                    torneo.tblTorneoDireccion = new schemaTorneoDireccion()
                    {
                        ldcCalle = model.tblTorneoDireccion.ldcCalle,
                        ldcNumeroExtInt = model.tblTorneoDireccion.ldcNumeroExtInt,
                        ldcColonia = model.tblTorneoDireccion.ldcColonia,
                        ldcMunicipio = model.tblTorneoDireccion.ldcMunicipio,
                        ldcEstado = model.tblTorneoDireccion.ldcEstado,
                        ldcCodigoPostal = model.tblTorneoDireccion.ldcCodigoPostal
                    };
                    torneo.torNumeroContacto = model.torNumeroContacto;
                    torneo.torCorreoContacto = model.torCorreoContacto;
                    torneo.torPrecioTorneo = model.torPrecioTorneo;
                    torneo.torDiasParaPago = model.torDiasParaPago;
                    if (model.torLigaFormaPago.ToUpper() == "COMISION")
                    {
                        torneo.tcfpptpId = model.tcfpptpId;
                        if (model.listTarifas != null)
                        {
                            torneo.tblTorneoTarifas = new List<schemaTorneoTarifas>();
                            foreach (var item in model.listTarifas.ToList()) //.Where(t => t.tarHabilitado == true)
                            {
                                torneo.tblTorneoTarifas.Add(new schemaTorneoTarifas()
                                {
                                    tarId = item.tarId,
                                    ttaHabilitado = item.tarHabilitado
                                });
                            }
                        }
                    }
                    else
                    {
                        torneo.tcfpptpId = null;
                        torneo.tblTorneoTarifas = null;
                    }
                    torneo.torUserIdCreador = User.Identity.GetUserId();
                    torneo.torFechaCreacionUTC = db.DateTimeMX();
                    torneo.torEstatus = model.torEstatus;
                    torneo.torPrivate = model.torPrivate;
                    torneo.torAprobada = true;
                    if (model.torTipo == constClass.torTipoCoaching)
                        torneo.torEsCoaching = true;
                    else
                        torneo.torEsCoaching = false;
                    torneo.torDeporteEnEquipo = model.torDeporteEnEquipo;

                    if (model.torId == 0)
                    {
                        //Crea un nuevo torneo
                        model.torId = db.setTorneo_Agregar(torneo);
                        if (model.torId > 0)
                        {
                            //Se guardo el torneo
                            ModelState.Clear();
                            ModelState.AddModelError(constClass.success, "El torneo ha sido guardado.");
                            if (teamName != "")
                            {
                                var team = new schemaEquipos();
                                team.equNombreEquipo = teamName;
                                team.equPrecioTorneo = model.torPrecioTorneo;
                                team.equUserIdCreador = torneo.torUserIdCreador;
                                team.torId = model.torId;

                                db.setCoachTeam_Add(team);
                                if (torneo.torEsCoaching)
                                {
                                    var equ = db.getEquipoByTorneo(torneo.torId);
                                    if (equ.Count > 0)
                                    {
                                        ViewBag.haveCoachingTeam = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(constClass.error, "Ocurrio un error guardando el torneo. Intenta nuevamente.");
                            error = true;
                        }
                    }
                    else
                    {
                        //Edita un torneo
                        torneo.torId = model.torId;
                        if (torneo.tblTorneoTarifas != null)
                            torneo.tblTorneoTarifas.ForEach(t => t.torId = torneo.torId);

                        if (torneo.torEsCoaching)
                        {
                            var equ = db.getEquipoByTorneo(torneo.torId);
                            if (equ.Count > 0)
                            {
                                ViewBag.haveCoachingTeam = true;
                            }
                        }
                        bool result = db.setTorneo_Editar(torneo);
                        if (result)
                        {
                            //Se guardo el torneo
                            ModelState.Clear();
                            ModelState.AddModelError(constClass.success, "El torneo ha sido guardado.");
                        }
                        else
                        {
                            ModelState.AddModelError(constClass.error, "Ocurrio un error guardando el torneo. Intenta nuevamente.");
                            error = true;
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Favor de llenar todos los campos.");
                    error = true;
                }


            }
            else
            {
                ModelState.AddModelError(constClass.error, "El nombre del Equipo " + model.torNombreTorneo + " ya Existe. Favor de Cambiar el nombre.");
                error = true;
            }
            if (error)
            {
                model = _TorneoNuevo_FillPostModel(model, false, false, false);
            }
            else
            {
                //Ya se guardo y hay q cargar la version de edicion.
                model = _TorneoNuevo_FillPostModel(model, false, false, true);
            }
            return PartialView("Torneos/_NuevoEdit", model);
        }

        /// <summary>
        /// Accion que obtiene la informacion de un torneo para ser mostrada para su edicion.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="usrId">Funcion como token - parametro de seguridad</param>
        /// <returns></returns>
        /// [HttpPost]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult _TorneoNuevoEditar(int Id, string usrId)
        {
            ModelState.Clear();
            var model = new TorneosViewModel();
            var torneo = new schemaTorneos();
            if (usrId != null)
                torneo = db.getTorneoByIdToken(Id, usrId);
            else
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
                model.torPrivate = torneo.torPrivate;
            }
            else
            {
                return Content("<h3>No se encontro el torneo. Intenta nuevamente.</h3>");
            }
            if (usrId != null)
                model = _TorneoNuevo_FillPostModel(model, false, false, true);

            ViewBag.haveCoachingTeam = false;

            if (model.torEsCoaching)
            {
                var equ = db.getEquipoByTorneo(Id);
                if (equ.Count > 0)
                {
                    ViewBag.haveCoachingTeam = true;
                }
            }

            return PartialView("Torneos/_NuevoEdit", model);
        }

        #endregion

        #endregion

        #region Coadministradores Torneos

        /// <summary>
        /// Regresa la vista de los coadministradores del torneo para editarlos o agregar
        /// </summary>
        /// <returns></returns>
        public ActionResult Torneos_CoadminsGridEdit()
        {
            return PartialView("Torneos/_CoadminsGridEdit");
        }

        /// <summary>
        /// Llena el grid de los coadministradores para su edición.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult _Torneos_CoadminsGridEdit(int torId)
        {
            var model = db.getTorneoCoadministradoresById(torId);
            return PartialView("Torneos/_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que se ejecuta al guardar un nuevo registro en el grid.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <param name="coadmin"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _Torneos_CoadminsGridEdit_AddNewPartial(int torId, TorneosCoAdministradoresViewModel coadmin)
        {
            var model = db.getTorneoCoadministradoresById(torId);
            if (coadmin.tcaEmail.Trim() != "")
            {
                string emailAddress = coadmin.tcaEmail.Replace("\"", "").Trim();
                string errMensaje = "";
                //Valida que el usuario exista, sino lo crea


                bool sendEmail = true;
                schemaTorneoCoAdministradores coAdministrador = new schemaTorneoCoAdministradores();
                if (errMensaje == "")
                {
                    //Revisa que el usuario no se repita en la tabla.
                    coAdministrador.torId = torId;
                    //coAdministrador.tcaUserId = user.Id;
                    //coAdministrador.tblUsuario = user;
                    coAdministrador.userCorreo = emailAddress;
                    coAdministrador.tcaCodigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
                    coAdministrador.tcaConfirmacion = false;

                    var admin = model.Where(m => m.tcaEmail == emailAddress).FirstOrDefault();
                    if (admin != null)
                    {
                        if (admin.tcaConfirmado == true)
                            sendEmail = false;
                    }
                    else
                    {
                        if (!db.setTorneoCoadmin(coAdministrador))
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

                if (errMensaje == "")
                {
                    ModelState.Clear();
                    ViewData["EditResult"] = "ok";
                }
                else
                {
                    ViewData["EditError"] = errMensaje;
                    ViewData["TorneosCoAdministradoresViewModel"] = coadmin;
                }
            }
            else
            {
                ViewData["EditError"] = "Favor de corregir los errores.";
                ViewData["TorneosCoAdministradoresViewModel"] = coadmin;
            }
            model = db.getTorneoCoadministradoresById(torId);
            return PartialView("Torneos/_CoadminsGridEdit", model);
        }

        /// <summary>
        /// Accion que elimina a un coadministrador desde el boton de eliminar del grid.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="key"></param>
        /// <param name="lcaUserId"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult _Torneos_CoadminsGridEdit_Delete(int tcoId)
        {
            var coadmin = db.getTorneoCoAdminByTcoId(tcoId);
            if (!db.setTorneoCoadmin_Delete_tcoId(tcoId, coadmin.userCorreo))
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
            else
                ViewData["EditResult"] = "ok";

            var model = db.getTorneoCoadministradoresById(coadmin.torId);
            return PartialView("Torneos/_CoadminsGridEdit", model);
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
        public bool sendCoAdministratorEmail(schemaTorneoCoAdministradores coAdmin)
        {
            // Send an email with this link
            string code = coAdmin.tcaCodigoConfirmacion;
            var callbackUrl = Url.Action("confirmarCoadmin", "AdminTorneos", new { email = coAdmin.userCorreo, code = code }, protocol: Request.Url.Scheme);
            var rejectUrl = Url.Action("rechazarCoadmin", "AdminTorneos", new { email = coAdmin.userCorreo, code = code }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            if (db.setTorneoCoadminConfirmCode_Update(coAdmin, code))
            {
                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    var user = db.getUserById(User.Identity.GetUserId());
                    var torneo = db.getTorneoById(coAdmin.torId);

                    string body = Global_Functions.getBodyHTML("~/Emails/TorneoConfirmCoAdmin.html");
                    var prof = db.getUserMainProfile(user.Id);
                    if (prof == null)
                        prof = new schemaUsersProfiles();
                    string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    if (usuarioNombre == "")
                        usuarioNombre = user.Email;

                    body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                    body = body.Replace("<%= NombreTorneo %>", torneo.torNombreTorneo);
                    body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                    body = body.Replace("<%= UrlRejectCode %>", rejectUrl);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(coAdmin.userCorreo, siteConfig.scoSenderDisplayEmailName, "Coadministrador de Torneo", body,
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
        /// Accion que se ejecuta cuando el usuario acepta la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult confirmarCoadmin(string email, string code)
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



            //Si el usuario inicio sesión con su cuenta, se procede a validar la información.
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            var torneo = db.getTorneoByCoAdminCodeEmail(email, code);
            if (torneo != null)
            {
                ViewBag.TorneoNombre = torneo.torNombreTorneo;
                var usuarioId = "";
                var user = new ApplicationUser();
                if (usr == null)
                {
                    usuPassword = db.generator_Pass();

                    var modelRegister = new RegisterViewModel();
                    modelRegister.usuPassword = usuPassword;
                    modelRegister.usuEmail = email;

                    var newUser = new AccountController()._RegisterNew(modelRegister, true);
                    if (newUser)
                    {
                        ApplicationUser userId = db.getUserByUserEmail(email);
                        user = userId;
                    }
                    else
                    {
                        usuarioId = null;
                    }
                    //  var userId = RegisterPlayer(user, usuPassword,constClass.rolPlayer);
                    //db.setClearEmailValidation(user);

                    usuarioId = user.Id;
                    //var res = Login(user.Id, user.Email, usuPassword, false);

                    enviarEmailAdmTorneoAviso(user.Email, torneo.torNombreTorneo, usuPassword);
                }
                else
                {
                    usuarioId = usr.Id;
                    user = usr;
                }
                var rolId = db.getRoleByName(constClass.rolAdminTorneos).Id;

                if (user.usuRolActual == null)
                {
                    UserManager.AddToRole(user.Id, constClass.rolAdminTorneos);
                }
                else
                {
                    var hasRolTor = db.getUserRoles(user).Where(l => rolId.Contains(l.rolId));
                    if (hasRolTor == null || hasRolTor.Count() <= 0)
                    {
                        UserManager.AddToRole(user.Id, constClass.rolAdminTorneos);                       
                    }
                }
                db.setCurrentUserRole(user.Id, rolId);


                if (db.setTorneoConfirmCoadmin(UserManager, user, code))
                {
                    enviarEmailAceptacionCoadministracionTorneo(torneo, user);

                    var prof = db.getUserMainProfile(user.Id);
                    if (prof == null)
                        prof = new schemaUsersProfiles();
                    string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    if (usuarioNombre == "")
                        usuarioNombre = user.Email;
                    ViewBag.UsuarioNombre = usuarioNombre;
                    ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                    SignInManager.SignIn(user, true, false);
                    ViewBag.jsScript = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Admin") + "');" +
                                        @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error aceptando la solicitud. Redireccionando . . .");
                    ViewBag.jsScript = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                        @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";
                }


                return View("CoadminConfirmacion");
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error aceptando la solicitud. Redireccionando . . .");
                return View("CoadminError");
            }


        }
        public bool enviarEmailAdmTorneoAviso(string email, string nombreTorneo, string password)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                //string emailTo = nombreJugador+","+ jugador.tblEquipos.tblUsuarioCreador.Email;
                string emailTo = email;
                string body = Global_Functions.getBodyHTML("~/Emails/ParticipanteAvisoConfirmacionPerfil.html");
                body = body.Replace("<%= NombreAdmin %>", email);
                body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                body = body.Replace("<%= usuario %>", email);
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
        /// <summary>
        /// Accion que se ejecuta cuando el usuario rechaza la confirmacion de coadministración
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult rechazarCoadmin(string email, string code)
        {
            var torneo = db.getTorneoByCoAdminCodeEmail(email, code);
            ViewBag.TorneoNombre = torneo.torNombreTorneo;
            if (db.setTorneoCoadmin_Delete(email, code))
            {
                var user = db.getUserByUserEmail(email);
                enviarEmailRechazoCoadministracionTorneo(torneo, user);
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

            return View("CoadminRechazo");
        }

        /// <summary>
        /// Envia los correos de aceptacion de coadministracion de torneo al dueño del torneo.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        public bool enviarEmailAceptacionCoadministracionTorneo(schemaTorneos torneo, ApplicationUser userConfirmado)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
            var toEmail = torneo.tblUserCreador.Email;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var prof = db.getUserMainProfile(userConfirmado.Id);
                if (prof == null)
                    prof = new schemaUsersProfiles();
                string usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                if (usuarioNombre == "")
                    usuarioNombre = userConfirmado.Email;
                string body = Global_Functions.getBodyHTML("~/Emails/TorneoCoadminAceptacion.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreTorneo %>", torneo.torNombreTorneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(toEmail, siteConfig.scoSenderDisplayEmailName, "Aceptación Coadministración de Torneo", body,
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
        public bool enviarEmailRechazoCoadministracionTorneo(schemaTorneos torneo, ApplicationUser userConfirmado)
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
                string body = Global_Functions.getBodyHTML("~/Emails/TorneoCoadminRechazo.html");
                body = body.Replace("<%= NombreAdministrador %>", usuarioNombre);
                body = body.Replace("<%= NombreTorneo %>", torneo.torNombreTorneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                bool mailSended = Global_Functions.sendMail(torneo.tblUserCreador.Email, siteConfig.scoSenderDisplayEmailName, "Rechazo Coadministración de Liga", body,
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

        #region Grid Busquedas

        [AllowAnonymous]
        public ActionResult TorneosGrid(int tipoGrid)
        {
            ModelState.Clear();
            switch (tipoGrid)
            {
                case 0:
                    ViewBag.torneosGridAction = "_TorneosGrid_LigaDetalleAdmin";
                    break;
                case 1:
                    ViewBag.torneosGridAction = "_TorneosGrid_TorneosAdmin";
                    break;
                case 2:
                    ViewBag.torneosGridAction = "_TorneosGrid_LigaDetalle";
                    break;
                case 3:
                    ViewBag.torneosGridAction = "_TorneosGrid_TorneoRefereeDetalle";
                    break;
            }
            return PartialView("Torneos/_TorneosGrid");
        }

        [ValidateInput(false)]
        public ActionResult _TorneosGrid_LigaDetalleAdmin(int? ligId, string ligKey, string torFiltroEstatus, bool admin = true)
        {

            var model = new List<TorneosGridViewModel>();
            if (User.Identity.IsAuthenticated)
            {
                var user = db.getUserById(User.Identity.GetUserId());

                if (user != null && !admin)
                {
                    model = db.getTorneosParaGridByLiga(null, null, user);
                }
                else
                {
                    model = db.getTorneosParaGridByLiga(ligId, ligKey, null);
                }
            }
            else
            {
                model = db.getTorneosParaGridByLiga(ligId, ligKey, null, false);
            }


            if (torFiltroEstatus != null)
            {
                // 0 = torneos activos
                // 1 = torneos finalizados
                switch (int.Parse(torFiltroEstatus))
                {
                    case 0:
                        model = model.Where(t => t.torFechaFinal == null
                                              || t.torFechaFinal >= db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        break;
                    case 1:
                        model = model.Where(t => t.torFechaFinal <= db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                    case 2:
                        model = model.Where(t => t.torEstatus == false)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                }
            }
            if (admin)
                model.ForEach(t => t.torAdminTorneo = true);
            else
            {
                model.ForEach(t => t.torAdminTorneo = false);
                model.ForEach(t => t.referee = true);
            }
            return PartialView("Torneos/_TorneosGrid", model);
        }

        [ValidateInput(false)]
        public ActionResult _TorneosGrid_TorneosAdmin(string torFiltroEstatus)
        {
            string userId = User.Identity.GetUserId();
            var model = db.getTorneosParaGridByUsuario(userId);

            if (torFiltroEstatus != null)
            {
                // 0 = torneos activos
                // 1 = torneos finalizados
                switch (int.Parse(torFiltroEstatus))
                {
                    case 0:
                        model = model.Where(t => t.torFechaFinal == null
                                              || t.torFechaFinal >= db.DateTimeMX()
                                              && t.torEstatus == true)
                                     .ToList();
                        break;
                    case 1:
                        model = model.Where(t => t.torFechaFinal <= db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                    case 2:
                        model = model.Where(t => t.torEstatus == false)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                }
            }

            model.ForEach(t => t.torAdminTorneo = true);

            return PartialView("Torneos/_TorneosGrid", model);
        }

        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult _TorneosGrid_LigaDetalle(int ligId, string ligKey, string torFiltroEstatus, string deporte, int? tipoTorneo = null)
        {
            var model = db.getTorneosParaGridByLiga(ligId, ligKey, null);
            if (torFiltroEstatus != null)
            {
                // 0 = torneos activos
                // 1 = torneos finalizados
                switch (int.Parse(torFiltroEstatus))
                {
                    case 0:
                        model = model.Where(t => t.torFechaFinal == null
                                              || t.torFechaFinal >= db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        break;
                    case 1:
                        model = model.Where(t => t.torFechaFinal <= db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                    case 2:
                        model = model.Where(t => t.torEstatus == false)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                }
            }
            if (deporte != "")
            {
                model = model.Where(l => l.tblCategoria.depNombre.ToUpper() == deporte.ToUpper()).ToList();
            }
            if (tipoTorneo > 0)
            {
                if (tipoTorneo != null)
                {
                    model = model.Where(l => l.tblCategoria.ttoId == tipoTorneo).ToList();
                }
            }
            model = model.Where(l => l.isPrivate == false).ToList();
            model.ForEach(t => t.torAdminTorneo = false);

            return PartialView("Torneos/_TorneosGrid", model);
        }
        [AllowAnonymous]
        public ActionResult _TorneosGrid_TorneoRefereeDetalle(string torFiltroEstatus)
        {
            string userId = User.Identity.GetUserId();
            var model = db.getTorneosParaGridByArbitro(userId);

            if (torFiltroEstatus != null)
            {
                // 0 = torneos activos
                // 1 = torneos finalizados
                switch (int.Parse(torFiltroEstatus))
                {
                    case 0:
                        model = model.Where(t => t.torFechaFinal == null
                                              || t.torFechaFinal >= db.DateTimeMX()
                                              && t.torEstatus == true)
                                     .ToList();
                        break;
                    case 1:
                        model = model.Where(t => t.torFechaFinal < db.DateTimeMX() && t.torEstatus == true)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                    case 2:
                        model = model.Where(t => t.torEstatus == false)
                                     .ToList();
                        model.ForEach(t => t.torTorneoTerminado = true);
                        break;
                }
            }

            model.ForEach(t => t.torAdminTorneo = false);
            model.ForEach(t => t.referee = true);
            return PartialView("Torneos/_TorneosGrid", model);
        }
        [AllowAnonymous]
        public ActionResult TorneoEstadisticas(int Id)
        {
            ViewBag.torId = Id;
            //return RedirectToAction("TorneoDetails","Home",new { Id = Id});
            return PartialView("Torneos/_TorneoEstadisticasPartial", new { Id = Id });
        }
        public ActionResult _ChartPieTorneoEstadisticas(int Id)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            var torneo = db.getTorneoById(Id);
            if (torneo != null)
            {
                //var torneos = db.getTorneosByLiga(liga.ligLiga.ligId).ToList();
                //var ids = torneos.Select(s => new { torId = s.torId, nombre = s.torNombreTorneo });
                var partidos = db.getPartidosByTorneoId(torneo.torId);
                var equipos = db.getEquipoByTorneo(torneo.torId);

                List<int> totalGoles = new List<int>();
                List<string> Equipo = new List<string>();
                int goles = 0;
                foreach (var item in equipos)
                {
                    var partidosEqu = partidos.Where(l => l.equIdUno == item.equId || l.equIdDos == item.equId).ToList();
                    foreach (var parE in partidosEqu)
                    {
                        if (parE.equIdUno == item.equId)
                        {
                            goles += (int)parE.equResultadoUno;
                        }
                        else if (parE.equIdDos == item.equId)
                        {
                            goles += (int)parE.equResultadoDos;
                        }
                    }
                    Equipo.Add(item.equNombreEquipo);
                    totalGoles.Add((int)goles);
                    goles = 0;
                }
                //var nombre = torneos.Select(s => new { nombre = s.torNombreTorneo }).ToList();

                var jsonData = new
                {
                    equipos = Equipo,
                    goles = totalGoles,
                    torneo = torneo.torNombreTorneo
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            return Json("wrong");
        }
        #endregion

        #endregion

        #region Comentarios

        #region Grid Busquedas

        /// <summary>
        /// Action para llamar el grid por primera vez
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult TorneoComentariosGrid()
        {
            ModelState.Clear();
            return PartialView("Torneos/_TorneoComentariosGrid");
        }

        /// <summary>
        /// Accion que se llama en los callback del grid para actualizarse.
        /// </summary>
        /// <param name="tipoGrid">
        /// 0 - Es cuando hay que traer todas los comentarios de la liga.
        /// 1 - Es cuando hay que traer solo los comentarios del torneo.
        /// </param>
        /// <param name="ligId"></param>
        /// <param name="tokenLiga">Es la fecha de creación de la liga, esto para que no se puedan traer cosas que no deben ver.</param>
        /// <param name="torId"></param>
        /// <param name="tokenTorneo">Es la fecha de creación del torneo, esto para que no se puedan traer cosas que no deben ver.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult _TorneoComentariosGrid_Callback(int tipoGrid, int ligId, string tokenLiga, int torId, string tokenTorneo)
        {
            List<TorneosComentariosGridViewModel> model = new List<TorneosComentariosGridViewModel>();

            switch (tipoGrid)
            {
                case 0:
                    model = db.getTorneoComentariosLiga(ligId, tokenLiga);
                    break;
                case 1:
                    model = db.getTorneoComentariosTorneo(torId, tokenTorneo); ;
                    break;
            }

            return PartialView("Torneos/_TorneoComentariosGrid", model);
        }
        [HttpPost]
        public ActionResult _CategoriaFiltro_Refresh(int ligId)
        {
            var model = new TorneosViewModel();
            model.ddlCategorias = _TorneoNuevo_GetDropDownListCategorias(ligId);
            return PartialView("Torneos/_CategoriaFiltro", model);
        }
        #endregion

        #endregion

        /// <summary>
        /// Cuando vienen los parametros del Guardar o Actualizar del grid, vienen con " ej: "hola", esta funcion la elimina.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected PartidosViewModel clearGridCommasPartidos(PartidosViewModel item)
        {
            item.equNombreEquipoUno = item.equNombreEquipoUno.Replace("\"", "").Trim();
            item.equNombreEquipoDos = item.equNombreEquipoDos.Replace("\"", "").Trim();
            return item;
        }

        [HttpPost]
        public ActionResult _Partidos_GuardarNuevo(PartidosViewModel model, string equImgDos, HttpPostedFileBase equImgURLFile, bool addEvent = false)
        {
            bool error = false;
            //if (!error)
            //    return RedirectToAction("_TorneoNuevoEditar", new { Id = 3, usrId = "c64a3d58-b751-4f82-855b-62823401ae69" });

            if (!error)
            {
                schemaPartidos partido = new schemaPartidos();
                var torneo = db.getTorneoById(model.torId);
                if (torneo.torTipo == "INTERNO")
                {
                    schemaArbitros arbitro = new schemaArbitros();

                    if (model.arbId > 0)
                    {
                        arbitro = db.getArbitros().Where(l => l.arbId == model.arbId).FirstOrDefault();
                        model.arbNombre = arbitro.arbNombre;
                        model.arbId = arbitro.arbId;
                    }
                    else
                    {
                        if (model.arbNombre != null && model.arbNombre != "")
                        {
                            schemaArbitros item = new schemaArbitros();
                            item.arbCorreo = model.arbNombre;
                            item.arbNombre = model.arbNombre;
                            var arbId = db.setArbitros(item, model.torId);
                            model.arbId = arbId;

                            arbitro = db.getArbitros().Where(l => l.arbId == model.arbId).FirstOrDefault();
                        }
                    }
                }

                foreach (string file in Request.Files)
                {
                    string filename = "", filenameDB = "";
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        string imgEquipo = model.imgDos;
                        model.imgDos = null;

                        string urlPath = Server.MapPath(constClass.urlEquiposImagenes);
                        if (!System.IO.Directory.Exists(urlPath))
                            System.IO.Directory.CreateDirectory(urlPath);

                        var extension = Path.GetExtension(fileContent.FileName);
                        if (constClass.imgLeaguesAllowedExtensions.Contains(extension.ToUpper()))
                        {
                            filename = urlPath + "\\" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                            filenameDB = constClass.urlEquiposImagenes + "/" + db.DateTimeMX().ToString("yyyyMMdd_hhmmss.fff") + extension;
                            filenameDB = filenameDB.Replace("~", "");
                            fileContent.SaveAs(filename);
                            bool savedFile = System.IO.File.Exists(filename);

                            if (savedFile)
                            {
                                model.imgDos = filenameDB;
                                if (System.IO.File.Exists(imgEquipo))
                                    System.IO.File.Delete(imgEquipo);
                            }
                            else
                            {
                                ModelState.AddModelError(constClass.error, "Hubo un error guardando la imagen del equipo.");
                                model.imgDos = imgEquipo;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(constClass.error, "Solo se aceptan imagenes con la extension permitida en la ventana de dialogo.");
                        }
                    }
                }

                partido.ligId = model.ligId;
                partido.torId = model.torId;
                partido.equIdUno = model.equIdUno;
                partido.equIdDos = model.equIdDos;
                var EquipoUno = db.getEquipoById(model.equIdUno);
                if (EquipoUno != null)
                {
                    partido.equNombreEquipoUno = EquipoUno.equNombreEquipo;
                    var EquipoDos = new schemaEquipos();
                    if (model.equIdDos > 0)
                        EquipoDos = db.getEquipoById(model.equIdDos);
                    else
                    {
                        EquipoDos.equNombreEquipo = model.equNombreEquipoDos;
                    }
                    partido.equNombreEquipoDos = EquipoDos.equNombreEquipo;
                    partido.lcatId = model.canId;
                    partido.parFecha_Inicio = model.parFecha_Inicio;
                    partido.parFecha_Fin = model.parFecha_Inicio.AddHours(model.parHour).AddMinutes(model.parMinutes);
                    partido.arbId = model.arbId;
                    partido.arbNombre = model.arbNombre;
                    partido.imgDos = model.imgDos;
                    partido.parEstatus = true;
                    //Crea un nuevo torneo
                    model.parId = db.setPartido_Agregar(partido);
                    if (model.parId > 0)
                    {
                        string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
                        if (model.arbId > 0)
                        {
                            var arbEmail = db.getArbitroById(partido.arbId).arbCorreo;
                            db.setArbitroPartido(partido.arbId, model.parId, code);
                            sendInvitationEmailRefereeByEvent(arbEmail, partido.arbNombre, partido.ligId, model.parId, partido.equNombreEquipoUno, partido.equNombreEquipoDos, partido.parFecha_Inicio, code,torneo.torNombreTorneo);
                        }
                        //Se guardo el Partido
                        ModelState.Clear();
                        ModelState.AddModelError(constClass.success, "El Partido se creo exitosamente.");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Ocurrio un error guardando el partido. Intenta nuevamente.");
                        error = true;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Ocurrio un error guardando el partido, Seleccione al Equipo Uno.");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Favor de llenar todos los campos.");
                error = true;
            }
            //string userId = User.Identity.GetUserId();
            //var user = db.getUserById(userId);
            // _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
            //var modelP = db.getPartidos(user).Where(l=> l.torId == model.torId).ToList();

            return PartialView("_ModalState_Errors");
            //return PartialView("Torneos/_PartidosGrid", modelP);
        }

        public bool cancelarPartido(int parId, bool status)
        {
            try
            {
                var partido = db.getPartidosById(parId);
                if (partido != null)
                {
                    partido.parEstatus = status;
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        /// <summary>
        /// Accion que se ejecuta cuando se llama al actualizar un nuevo registro desde el grid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public JsonResult _PartidosGridEdit_UpdatePartial(PartidosViewModel item)
        {
            var result = "";
            if (ModelState.IsValid != false)
            {

            }
            else
            {
                result = "Favor de corregir los errores.";
            }

            var team1 = db.getEquipoById(item.equIdUno);

            var team2 = db.getEquipoById(item.equIdDos);
            item.equNombreEquipoUno = team1.equNombreEquipo;
            item.equNombreEquipoDos = (team2 != null) ? team2.equNombreEquipo : item.equNombreEquipoDos;

            var partido = db.getPartidoById(item.parId);

            if (partido.arbId != item.arbId && item.arbId > 0)
            {
                string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
                if (item.arbId > 0)
                {
                    if (partido.arbId > 0)
                    {
                        var arbEmail = db.getArbitroById(partido.arbId).arbCorreo;
                        sendDropEventEmailReferee(arbEmail, arbEmail, partido.ligId, item.parId, partido.equNombreEquipoUno, partido.equNombreEquipoDos, partido.parFecha_Inicio, code);
                    }

                    var arb_new = db.getArbitroById(item.arbId).arbCorreo;

                    db.editArbitroPartido(partido.arbId, item.arbId, item.parId, code);

                    sendInvitationEmailRefereeByEvent(arb_new, arb_new, partido.ligId, item.parId, partido.equNombreEquipoUno, partido.equNombreEquipoDos, partido.parFecha_Inicio, code,partido.tblTorneos.torNombreTorneo);

                    item.arbNombre = arb_new;

                }
            }

            if (db.setPartidos_Edit(item))
            {
                result = "success";
            }
            else
            {
                result = "Hubo un error guardando la Cancha";
            }

            return Json(result);
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult _PartidosEnviarInvitacionArbitro(PartidosViewModel item)
        {
            var result = "success";
            string code = Global_Functions.getSha1(0, Global_Functions.generateCode());
            var partido = db.getPartidoById(item.parId);
            var arb_new = db.getArbitroById(item.arbId).arbCorreo;
            db.editArbitroPartido(partido.arbId, item.arbId, item.parId, code);
            if (!sendInvitationEmailRefereeByEvent(arb_new, arb_new, partido.ligId, item.parId, partido.equNombreEquipoUno, partido.equNombreEquipoDos, partido.parFecha_Inicio, code, partido.tblTorneos.torNombreTorneo))
                result = "Wrong";
            return Json(result);
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

            for (int i = 0; i < (ligas.Count) - 1; i++)
            {
                var aux = ligas[i].ligId;
                Torneos = db.getTorneosByLiga(aux);
                Canchas = db.getCanchasbyLigas(aux);
            }

            for (int i = 0; i < (Torneos.Count) - 1; i++)
            {
                var aux = Torneos[i].torId;
                Equipos = db.getEquiposByTorneo(aux);
                //   Arbitros = db.get
            }

            ViewData["cmbLigas"] = ligas;

            ViewData["cmbTorneos"] = Torneos;

            //ViewData["cmbArbitros"] = Arbitros;

            ViewData["cmbEquipos"] = Equipos;

            ViewData["cmbCanchas"] = Canchas;
        }

        /// <summary>
        /// Accion que se ejecuta cuando se llama al confirmar eliminar un registro desde el grid.
        /// </summary>
        /// <param name="parId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _PartidosGridEdit_Delete(int? parId)
        {
            if (db.setPartidos_Delete((int)parId))
            {
                ModelState.Clear();
                //ViewData["gvtCanchasCallback"] = "ok";
            }
            else
            {
                ViewData["EditError"] = "Hubo un error. Intentalo nuevamente.";
            }
            string userId = User.Identity.GetUserId();
            var user = db.getUserById(userId);
            _PartidosGridEdit_EditViewData(user.Id, user.usuRolActual);
            var model = db.getPartidos(user);
            return PartialView("Torneos/_PartidosGrid", model);
        }

        /// <summary>
        /// Regresa la vista de la pantalla de calendario
        /// </summary>
        /// <returns></returns>
        public ActionResult Calendario()
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var partido = db.getPartidos(user);
            //PartidosViewModel model = adm._Calendario_Event(user);

            var model = new AdminLigasController().filtros_Calendario(user);

            model.numPartidos = partido.Count;

            return View(model);
        }

        /// <summary>
        /// Regresa la vista de la pantalla de pagos
        /// </summary>
        /// <returns></returns>
        public ActionResult Pagos()
        {
            return View();
        }
        public ActionResult _PagosGrid_Callback()
        {
            List<PagosGridViewModel> model = new List<PagosGridViewModel>();
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);
            model = db.getGridPagosTorneos(user.Id);

            return PartialView("Banwire/_GridPagosTorneo", model);
        }
        [HttpPost]
        public JsonResult _DetallesPagoTorneo(int torId)
        {
            var pagos = db.getDetallesTorneo(torId);
            var rows = pagos.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _RealizarPago(int torId)
        {
            var identity = User.Identity.GetUserId();
            var user = db.getUserById(identity);

            var datosUsuario = db.getUserProfile(user);

            var torneo = db.getTorneoById(torId);

            var cust = from e in datosUsuario
                       select new
                       {
                           torId = torneo.torId,
                           torNombre = torneo.torNombreTorneo,
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
                           total = torneo.torPrecioTorneo,
                       };

            var rows = cust.ToArray();
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

        public ActionResult _Filtro_Liga_Torneo(int? ligId = null, int? torId = null, ApplicationUser usuario = null,Boolean? viewTor=true)
        {
            var user = new ApplicationUser();
            if (usuario == null || usuario.usuRolActual == null)
            {
                user = db.getUserById(User.Identity.GetUserId());
            }
            else
            {
                user.Id = usuario.Id;
                user.usuRolActual = usuario.usuRolActual;
            }

            var model = new FiltroLigasTorneosViewModel();
            model.viewTor = (Boolean)viewTor;
            var role = db.getRoles().Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper());

            if (role != null)
            {
                var rolName = role.FirstOrDefault().rolName;
                if (rolName == constClass.rolOwners)
                {
                    model.ddlLigas = db.getUserLeagues(user.Id, user.usuRolActual)
                    .Select(l => new SelectListItem { Text = l.ligNombre.ToUpper(), Value = l.ligId.ToString() })
                    .ToList();
                }
                else if (rolName == constClass.rolAdminTorneos)
                {
                    var torneos = db.getTorneosByUser(user.Id);
                    if (torneos != null)
                    {
                        var lista = new List<SelectListItem>();
                        foreach (var item in torneos)
                        {
                            var ligas = db.getLigaById(item.ligId);
                            var ligaList = new SelectListItem { Text = ligas.ligNombreLiga.ToUpper(), Value = ligas.ligId.ToString() };
                            lista.Add(ligaList);
                        }
                        model.ddlLigas = lista;

                        if (ligId != null)
                        {
                            var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                            if (ligaSeleccionada != null)
                                ligaSeleccionada.Selected = true;
                            model.ligId = (int)ligId;
                        }

                        model.ddlTorneos = torneos
                                       .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                       .ToList();

                        if (torId != null)
                        {
                            var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == torId.ToString());
                            if (torneoSeleccionada != null)
                                torneoSeleccionada.Selected = true;
                        }

                        return PartialView("Ligas/_FlitrosLigaTorneo", model);
                    }
                }
            }

            if (model.ddlLigas.Count > 0)
            {
                if (ligId != null)
                {
                    var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                    if (ligaSeleccionada != null)
                        ligaSeleccionada.Selected = true;
                    model.ligId = (int)ligId;
                }
                else
                {
                    if (torId != null)
                    {
                        var torneo = db.getTorneoById((int)torId);
                        model.ligId = torneo.ligId;
                    }
                    else
                        model.ligId = int.Parse(model.ddlLigas.First().Value);
                    //model.ligId = (int)ligId;
                }
                model.ddlTorneos = db.getTorneosByLiga(model.ligId)
                                       .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                       .ToList();

                if (torId != null)
                {
                    var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == torId.ToString());
                    if (torneoSeleccionada != null)
                        torneoSeleccionada.Selected = true;
                }

            }
            return PartialView("Ligas/_FlitrosLigaTorneo", model);
        }
        public ActionResult _FiltroLigaTorneoGrid(int? ligId = null, int? torId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var model = new FiltroLigasTorneosViewModel();
            var role = db.getRoles().Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper());

            if (role != null)
            {
                var rolName = role.FirstOrDefault().rolName;
                if (rolName == constClass.rolOwners)
                {
                    model.ddlLigas = db.getUserLeagues(user.Id, user.usuRolActual)
                    .Select(l => new SelectListItem { Text = l.ligNombre.ToUpper(), Value = l.ligId.ToString() })
                    .ToList();
                }
                else if (rolName == constClass.rolAdminTorneos)
                {
                    var torneos = db.getTorneosByUser(user.Id);
                    if (torneos != null)
                    {
                        var lista = new List<SelectListItem>();
                        foreach (var item in torneos)
                        {
                            var ligas = db.getLigaById(item.ligId);
                            var ligaList = new SelectListItem { Text = ligas.ligNombreLiga.ToUpper(), Value = ligas.ligId.ToString() };
                            lista.Add(ligaList);
                        }
                        model.ddlLigas = lista;

                        if (ligId != null)
                        {
                            var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                            if (ligaSeleccionada != null)
                                ligaSeleccionada.Selected = true;
                            model.ligId = (int)ligId;
                        }

                        model.ddlTorneos = torneos
                                       .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                       .ToList();

                        if (torId != null)
                        {
                            var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == torId.ToString());
                            if (torneoSeleccionada != null)
                                torneoSeleccionada.Selected = true;
                        }

                        return PartialView("Referee/_arbitroFiltroGrid", model);
                    }

                }
            }


            if (model.ddlLigas.Count > 0)
            {
                if (ligId != null)
                {
                    var ligaSeleccionada = model.ddlLigas.FirstOrDefault(l => l.Value == ligId.ToString());
                    if (ligaSeleccionada != null)
                        ligaSeleccionada.Selected = true;
                    model.ligId = (int)ligId;
                }
                else
                {
                    if (torId != null)
                    {
                        var torneo = db.getTorneoById((int)torId);
                        model.ligId = torneo.ligId;
                    }
                    else
                        model.ligId = int.Parse(model.ddlLigas.First().Value);
                    //model.ligId = (int)ligId;
                }
                model.ddlTorneos = db.getTorneosByLiga(model.ligId)
                                       .Select(t => new SelectListItem { Text = t.torNombreTorneo.ToUpper(), Value = t.torId.ToString() })
                                       .ToList();

                if (torId != null)
                {
                    var torneoSeleccionada = model.ddlTorneos.FirstOrDefault(l => l.Value == torId.ToString());
                    if (torneoSeleccionada != null)
                        torneoSeleccionada.Selected = true;
                }

            }
            return PartialView("Referee/_arbitroFiltroGrid", model);
        }
        public ActionResult ArbitrosGridEdit()
        {
            return PartialView("Torneos/_ArbitrosGridEdit");
        }
        public ActionResult _ArbitrosGridEdit_Callback(int? ligId = null, int? torId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var model = db.getArbitrosLigas(user.Id);
            /*if (torId != null)
            {
               var modelTor = model.Where(l => l.ligId == ligId).ToList();
                return PartialView("Torneos/_ArbitrosGridEdit", modelTor);
            }*/
            if (ligId != null && model != null)
            {
                model = model.Where(l => l.ligId == ligId).ToList();
            }
            return PartialView("Torneos/_ArbitrosGridEdit", model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _ArbitrosGridEdit_AddNewPartial(schemaArbitros item, int ligId, int? arbId = null, bool edit = false)
        {
            if (edit != true)
            {
                var arbLiga = new schemaArbitrosLigas();
                string codigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
                arbLiga.arbCodigoConfirmacion = codigoConfirmacion;
                arbLiga.ligId = ligId;

                if (item.arbNombre == null || item.arbNombre == "")
                    item.arbNombre = item.arbCorreo;

                if (ModelState.IsValid)
                {
                    var arbitroID = db.setArbitros(item, ligId);
                    if (arbitroID > 0)
                    {
                        db.setArbitroLiga(arbitroID, ligId, codigoConfirmacion);
                        ModelState.Clear();
                        sendInvitationEmailReferee(item.arbCorreo, item.arbNombre, ligId, codigoConfirmacion);
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
            }
            else
            {
                var arbitro = db.setArbitros(item, ligId, arbId, edit);
                var aux = arbitro;
            }
            var user = db.getUserById(User.Identity.GetUserId());
            var model = db.getArbitrosLigas(user.Id);

            return PartialView("Torneos/_ArbitrosGridEdit", model);
        }

        public ActionResult _ArbitrosEdit(int arbId)
        {
            var arbitro = db.getArbitrosLigas(null).Where(l => l.arbId == arbId);

            return _Filtro_Liga_Torneo(arbitro.FirstOrDefault().ligId);
        }
        [HttpPost]
        public JsonResult _ArbitrosDelete(int arbId)
        {
            try
            {
                if (db.setDeleteArbitro(arbId))
                {
                    return Json("Usuario Eliminado Exitosamente");
                }
            }
            catch (Exception e)
            {
                return Json("Error " + e);
                throw;
            }
            return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _ArbitrosInvite(int arbId, int ligId)
        {
            var arbLiga = new schemaArbitrosLigas();
            string codigoConfirmacion = Global_Functions.getSha1(0, Global_Functions.generateCode());
            arbLiga.arbCodigoConfirmacion = codigoConfirmacion;
            arbLiga.ligId = ligId;

            var item = db.getArbitroById(arbId);

            db.setArbitroLiga(arbId, ligId, codigoConfirmacion, true);
            ModelState.Clear();
            if (sendInvitationEmailReferee(item.arbCorreo, item.arbNombre, ligId, codigoConfirmacion))
                return Json("Invitacion Enviada Exitosamente !");

            return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Envia los correos a Arbitros para participacion.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        [NonAction]
        public bool sendInvitationEmailReferee(string arbCorreo, string arbNombre, int ligId, string codigoConfirmacion)
        {
            var liga = db.getLigaById(ligId);
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

                // Links del correo
                var confirmarUrl = Url.Action("ArbitroConfirmar", "Admin", new { email = arbCorreo, ligId = ligId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);
                var rechazarUrl = Url.Action("ArbitroRechazar", "Admin", new { email = arbCorreo, ligId = ligId, code = codigoConfirmacion }, protocol: Request.Url.Scheme);

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroInvitacion.html");

                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                body = body.Replace("<%= UrlValidationCode %>", confirmarUrl);
                body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                bool mailSended = Global_Functions.sendMail(arbCorreo, siteConfig.scoSenderDisplayEmailName, "Invitación de Artbitro a Liga", body,
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

        public bool sendInvitationEmailRefereeByEvent(string arbCorreo, string arbNombre, int ligId, int parId, string equUno, string equDos, DateTime fecha, string codigo,string torName)
        {
            var liga = db.getLigaById(ligId);
            
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
                var fechaPartido = db.getPartidoById(parId).parFecha_Fin.ToLongDateString();
                // Links del correo
                var confirmarUrl = Url.Action("ArbitroPartidoConfirmar", "Referee", new { email = arbCorreo, ligId = ligId, parId = parId, code = codigo }, protocol: Request.Url.Scheme);
                var rechazarUrl = Url.Action("ArbitroPartidoRechazar", "Referee", new { email = arbCorreo, ligId = ligId, parId = parId, code = codigo }, protocol: Request.Url.Scheme);

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroPartidoInvitacion.html");

                body = body.Replace("<%= NombreTorneo %>", torName);
                body = body.Replace("<%= NombreLiga %>", liga.ligNombreLiga);
                body = body.Replace("<%= NombreEquipoUno %>", equUno);
                body = body.Replace("<%= NombreEquipoDos %>", equDos);
                body = body.Replace("<%= fechaPartido %>", fechaPartido);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);

                body = body.Replace("<%= UrlValidationCode %>", confirmarUrl);
                body = body.Replace("<%= UrlRejectCode %>", rechazarUrl);

                bool mailSended = Global_Functions.sendMail(arbCorreo, siteConfig.scoSenderDisplayEmailName, "Invitación a Partido", body,
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


        public bool sendDropEventEmailReferee(string arbCorreo, string arbNombre, int ligId, int parId, string equUno, string equDos, DateTime fecha, string codigo)
        {
            var partido = db.getPartidoById(parId);
            var correosEnviados = true;
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                // Links del correo
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);



                // Links del correo

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroPartidoEliminado.html");

                body = body.Replace("<%= NombreTorneo %>", partido.tblTorneos.torNombreTorneo );
                body = body.Replace("<%= NombreLiga %>", partido.tblLigas.ligNombreLiga);
                body = body.Replace("<%= NombreEquipoUno %>", equUno);
                body = body.Replace("<%= NombreEquipoDos %>", equDos);

                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);


                bool mailSended = Global_Functions.sendMail(arbNombre, siteConfig.scoSenderDisplayEmailName, "Cancelaron Partido", body,
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
        [HttpPost]
        [AllowAnonymous]
        public ActionResult VerifySuscribeTournament(int torId)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booSuccess = true;
            resultJson.booHasErrMessagePartialView = true;
            if (User.Identity.IsAuthenticated)
            {
                var user = db.getUserById(User.Identity.GetUserId());
                var torneo = db.getTorneoById(torId);
                if (torneo.torEsCoaching)
                {
                    var jugEquipo = db.getJugadoresByTorneo_UserEmail(torId, user.Email);
                    if (jugEquipo.Any())
                    {
                        if (jugEquipo.Count >= 1)
                        {
                            ModelState.AddModelError(constClass.info, "Lo sentimos, solo se permite inscribirse una ocacion por Torneo ");
                            resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                            resultJson.booSuccess = false;
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError(constClass.info, "Tiene que Crear o Ingresar una cuenta Enligate para poder Inscribirse");
                resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                resultJson.booHasErrMessagePartialView = false;
                resultJson.booSuccess = false;
            }

            return Json(resultJson);
        }

        public ActionResult VerifyTournamentName(string torName, int ligId, int? torId = null)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booSuccess = true;
            var torneos = new List<schemaTorneos>();
            if (torId != null)
                torneos = db.getTorneosByLiga(ligId).Where(l => l.torId != torId && l.torNombreTorneo.ToUpper().Trim() == torName.ToUpper().Trim()).ToList();
            else
                torneos = db.getTorneosByLiga(ligId).Where(l => l.torNombreTorneo.ToUpper().Trim() == torName.ToUpper().Trim()).ToList();


            if (torneos.Any())
            {
                if (torneos.Count >= 1)
                {
                    ModelState.AddModelError(constClass.info, "Lo sentimos, El nombre " + torName + " ya esta registrado en este Torneo !");
                    resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                    resultJson.booSuccess = false;
                }
            }

            return Json(resultJson);
        }
        public ActionResult VerifyTeamNameTournament(int torId, string equName, int? equId = null)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booSuccess = true;
            var torneo = db.getTorneoById(torId);

            var equipos = new List<schemaEquipos>();
            if (equId != null)
                equipos = db.getEquipoByTorneo(torId).Where(l => l.equId != equId && l.equNombreEquipo.ToUpper().Trim() == equName.ToUpper().Trim()).ToList();
            else
                equipos = db.getEquipoByTorneo(torId).Where(l => l.equNombreEquipo.ToUpper().Trim() == equName.ToUpper().Trim()).ToList();


            if (equipos.Any())
            {
                if (equipos.Count >= 1)
                {
                    ModelState.AddModelError(constClass.info, "Lo sentimos, El nombre " + equName + " ya esta registrado en este Torneo !");
                    resultJson.strPartialViewString = RenderPartialViewToString("_ModalState_Errors");
                    resultJson.booSuccess = false;
                }
            }

            return Json(resultJson);
        }
        public ActionResult VerifyPlayerTournament(AgregarJugadorEquipoViewModel modelJugador, int torId)
        {
            var resultJson = new JsonResultViewModel();
            resultJson.booSuccess = true;
            var jugEquipo = db.getJugadoresByTorneo_UserEmail(torId, modelJugador.jugCorreo);
            if (jugEquipo.Any())
            {
                if (jugEquipo.Count >= 1)
                {
                    ModelState.AddModelError(constClass.info, "Lo sentimos,el jugador " + modelJugador.jugCorreo + " esta participando en otro Equipo de este Torneo");
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