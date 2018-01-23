using System;
using System.Linq;
using System.Collections.Generic;
using sw_EnligateWeb.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Mvc;
using sw_EnligateWeb.Models.HelperClasses;
using System.Web;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace sw_EnligateWeb.Engine
{
    public class DatabaseFunctions
    {
        ApplicationDbContext dbApp;
        public string strError;
        static string errorMsg = "Hubo un error al consultar la base de datos";

        public DatabaseFunctions()
        {
            dbApp = new ApplicationDbContext();
        }

        public DatabaseFunctions(ApplicationDbContext _dbApp)
        {
            this.dbApp = _dbApp;
        }

        public void SaveChanges()
        {
            dbApp.SaveChanges();
        }

        public DateTime DateTimeMX()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            Debug.WriteLine("*************DEBUG************");
            var dateMx = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            Debug.WriteLine(TimeZoneInfo.ConvertTime(dateMx, tz).Date);
            return dateMx;
        }

        #region Deportes

        /// <summary>
        /// Regresa la lista de deportes activos.
        /// </summary>
        /// <returns></returns>
        public List<schemaDeportes> getDeportes_Active()
        {
            return dbApp.tblDeportes
                        .Where(d => d.depEstatus == true)
                        .OrderBy(d => d.depPrioridad)
                        .ToList();
        }

        #endregion

        #region Tipo de torneo

        public List<schemaTipoTorneos> getTiposTorneo_Active()
        {
            return dbApp.tblTipoTorneo
                        .Where(t => t.ttoEstatus == true)
                        .OrderBy(t => t.ttoNombre)
                        .ToList();
        }

        #endregion

        #region Ciudad

        public List<schemaCiudades> getCiudades_Active()
        {
            return dbApp.tblCiudades
                        .Where(c => c.ciuEstatus == true)
                        .OrderBy(c => c.ciuNombre)
                        .ToList();
        }

        #endregion

        #region Zona

        public List<schemaZonas> getZonas_Active()
        {
            return dbApp.tblZonas
                        .Where(z => z.zonEstatus == true)
                        .OrderBy(z => z.zonZona)
                        .ToList();
        }

        #endregion

        #region Users-Profile-Config

        /// <summary>
        /// Regresa una lista con los roles del sistema
        /// </summary>
        /// <returns></returns>
        public ICollection<RoleModelViewModel> getRoles()
        {
            strError = "";
            try
            {
                return (from rol in dbApp.Roles
                        select new RoleModelViewModel
                        {
                            rolId = rol.Id,
                            rolName = rol.Name
                        }).ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
                strError = errorMsg;
            }
            return null;
        }

        /// <summary>
        /// Regresa una lista de los roles a los que pertenece el usuario.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<RoleModelViewModel> getUserRoles(ApplicationUser user)
        {
            try
            {                
                return (from usrRoles in user.Roles
                        join r in dbApp.Roles on usrRoles.RoleId equals r.Id
                        select new RoleModelViewModel
                        {
                            rolId = r.Id,
                            rolName = r.Name
                        }).ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Actualiza el rol que esta usando el usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool setCurrentUserRole(string userId, string roleId)
        {
            try
            {
                var usr = getUserById(userId);
                if(usr != null)
                {
                    usr.usuRolActual = roleId;
                    SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Obtiene el rol actual del usuario y la lista de roles del usuario.
        /// </summary>
        /// <param name="usr"></param>
        /// <returns>
        /// Key,object:
        /// currentUsrRoleName, string - Es el nombre del rol que esta usando el usuario actualmente
        /// rolesUser, (List<SelectListItem> - Es la lista de
        /// currentUsrRoleId, string
        /// </returns>
        public Dictionary<string, object> set_getUserCurrentRole(string userName)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            var usr = getUserByUserName(userName);
            List<RoleModelViewModel> usrRoles = getUserRoles(usr);
            var rolesUser = usrRoles.Select(r => new SelectListItem
            {
                Text = r.rolName.ToUpper(),
                Value = r.rolId.ToUpper(),
            }).OrderBy(r => r.Text).ToList();

            var currentUsrRoleId = usr.usuRolActual;
            if (currentUsrRoleId==null)
            {
                var rol_player = getRoleByName(constClass.rolPlayer);
                setCurrentUserRole(usr.Id,rol_player.Id);
            }
            var haveRolActual = rolesUser.Where(l => l.Value == usr.usuRolActual.ToUpper());
            if (!haveRolActual.Any())
            {
                setCurrentUserRole(usr.Id, usr.usuRolActual);
                var rolActual = getRoles().Where(l => l.rolId == usr.usuRolActual).FirstOrDefault();
                rolesUser.Add(new SelectListItem { Text = rolActual.rolName.ToUpper(), Value = rolActual.rolId.ToUpper() });
            }
            if (currentUsrRoleId == null)
            {
                currentUsrRoleId = rolesUser.FirstOrDefault().Value;
                setCurrentUserRole(usr.Id, currentUsrRoleId);
            }
            var currentUsrRoleName = "";
            var rolNameTemp = rolesUser.Where(r => r.Value == currentUsrRoleId.ToUpper());
            if (rolNameTemp.Any())
            {
                currentUsrRoleName = getRoles().Where(l=> l.rolId.ToUpper() == usr.usuRolActual.ToUpper()).First().rolName;
            }else
            {
                currentUsrRoleName = rolNameTemp.First().Text;
            }
            result.Add("currentUsrRoleName", currentUsrRoleName);
            result.Add("currentUsrRoleId", currentUsrRoleId);
            result.Add("rolesUser", rolesUser);

            return result;
        }

        /// <summary>
        /// Regresa las propiedades del rol por su nombre
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public IdentityRole getRoleByName(string roleName)
        {
            strError = "";
            try
            {
                return (from rol in dbApp.Roles
                        where rol.Name.Equals(roleName)
                        select rol).FirstOrDefault();
            }
            catch (Exception ex)
            {
                ex.ToString();
                strError = errorMsg;
            }
            return null;
        }

        /// <summary>
        /// Obtiene los datos del usuario por su id
        /// </summary>
        /// <param name="usuId"></param>
        /// <returns></returns>
        public ApplicationUser getUserById(string usuId)
        {
            return dbApp.Users.Where(u => u.Id.Equals(usuId)).FirstOrDefault();
        }

        public List<ApplicationUser> getUserCountLikeName(string name)
        {
            return dbApp.Users.Where(l => l.UserName.Contains(name)).ToList();
        }
        /// <summary>
        /// Regresa los datos del usuario por su nombre de usuario (correo)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public ApplicationUser getUserByUserName(string name)
        {
            var data = dbApp.Users.Where(u => u.UserName.Equals(name));
            return data.FirstOrDefault();
        }
        public ApplicationUser getUserByUserEmail(string email)
        {
            return dbApp.Users.Where(u => u.Email.Equals(email) || u.UserName.Equals(email)).FirstOrDefault();
        }

        /// <summary>
        /// Regresa una lista con todos los usuarios activos
        /// </summary>
        /// <returns></returns>
        public ICollection<ApplicationUserViewModel> getAllActiveUsers()
        {
            return (from usr in dbApp.Users
                    let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == usr.Id).FirstOrDefault()
                    let nombres = (prof != null) ? prof.uprNombres : ""
                    let apellidos = (prof != null) ? prof.uprApellidos : ""
                    join rol in dbApp.Roles on usr.Roles.FirstOrDefault().RoleId equals rol.Id
                    where usr.usuEstatus.Equals(true)
                    select new ApplicationUserViewModel()
                    {
                        Id = usr.Id,
                        Email = usr.Email,
                        PhoneNumber = usr.PhoneNumber,
                        usuNombre = nombres,
                        usuApellido = apellidos,
                        RoleName = rol.Name,
                        EmailConfirmed = usr.EmailConfirmed
                    }).ToList();
        }

        /// <summary>
        /// Regresa el id del usuario por su id de red social.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        public string getUserIdByProviderKey(string provider, string providerKey)
        {
            try
            {
                string sqlQuery = @"SELECT LOG.UserId FROM AspNetUserLogins LOG 
                                    WHERE LOG.LoginProvider = '" + provider + "'" +
                                    " AND LOG.ProviderKey = '" + providerKey + "'";
                return dbApp.Database.SqlQuery<string>(sqlQuery).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return null;
        }

        /// <summary>
        /// Regresa el ultimo registro de los parametros de configuración
        /// </summary>
        /// <returns></returns>
        public schemaSiteConfigs getLastSiteConfRow()
        {
            return (from sco in dbApp.tblSiteConfigs
                    orderby sco.scoId descending
                    select sco).FirstOrDefault();
        }

       
        public List<ApplicationUser> getAdministrators()
        {
            try
            {
                var list = new List<ApplicationUser>();
                var context = new ApplicationDbContext();
                var rol = getRoleByName(constClass.rolAdmin);
                var users = from u in context.Users
                            where u.Roles.Any(r => r.RoleId == rol.Id)
                            select u;
                foreach (var item in users)
                {
                    list.Add(item);
                }

                return list;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Obtiene el perfil principal del usuario.
        /// </summary>
        /// <param name="usrId"></param>
        /// <returns></returns>
        public schemaUsersProfiles getUserMainProfile(string usrId)
        {
            return (from upr in dbApp.tblUsersProfiles
                    where upr.userIdOwner == usrId
                       && upr.uprPerfilPrincipal == true
                    orderby upr.uprId descending
                    select upr).FirstOrDefault();
        }
        public List<CustViewModel> getUserProfile(ApplicationUser user)
        {
            return (from upr in dbApp.tblUsersProfiles                   
                    where upr.userIdOwner == user.Id
                    select new CustViewModel()
                    {
                        fname = upr.uprNombres,
                        mname = upr.uprApellidos.ToString(),
                        email = user.Email,
                        phone = upr.uprTelefono,
                        addr = upr.direccion,
                        city = upr.uprCiudad,
                        state = upr.uprEstado,
                        country = upr.uprPais,
                        zip = upr.cp                        
                    }
                    ).ToList();
        }
        /// <summary>
        /// Actualiza la confirmación de correo del usuario
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        public bool setUpdateUserProfile_ConfirmEmail(ApplicationUser usr, schemaUsersProfiles upr)
        {
            try
            {
                ApplicationUser user = getUserById(usr.Id);
                if (user != null)
                {
                    var prof = getUserMainProfile(user.Id);
                    if (prof == null)
                    {
                        prof = new schemaUsersProfiles();
                        prof.userIdOwner = user.Id;
                        prof.uprId = 1;
                        dbApp.tblUsersProfiles.Add(prof);
                    }

                    prof.uprNombres = (prof.uprNombres.Trim() == "-") ? upr.uprNombres : prof.uprNombres;
                    prof.uprApellidos = (prof.uprApellidos.Trim() == "-") ? upr.uprApellidos : prof.uprApellidos;
                    user.EmailConfirmed = true;
                    user.usuEmailValidationCode = "";
                    user.usuEmailValidationCodeEndDateUtc = null;

                    SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza los datos del perfil principal del usuario, sino tiene le agrega una cuenta
        /// </summary>
        /// <param name="upr"></param>
        /// <returns></returns>
        public bool setUserProfileMain_UpdateInsert(ApplicationUser usr, schemaUsersProfiles upr)
        {
            try
            {
                ApplicationUser user = getUserById(usr.Id);
                if (user != null)
                {
                    user.PhoneNumber = upr.uprTelefono;
                    //user.Email = usr.Email;

                    var profile = getUserMainProfile(usr.Id);
                    if (profile == null)
                    {
                        profile = new schemaUsersProfiles();
                        profile.userIdOwner = usr.Id;
                        profile.uprId = 1;
                        dbApp.tblUsersProfiles.Add(profile);
                    }

                    if(upr.uprProfileImageURL != null)
                        profile.uprProfileImageURL = upr.uprProfileImageURL;
                    profile.uprNombres = (upr.uprNombres.Trim() == "") ? "-" : upr.uprNombres.Trim();
                    profile.uprApellidos = (upr.uprApellidos.Trim() == "") ? "-" : upr.uprApellidos.Trim();
                    profile.uprGenero = upr.uprGenero;
                    profile.uprFechaNacimiento = upr.uprFechaNacimiento;

                    profile.uprPais = upr.uprPais;
                    profile.uprEstado = upr.uprEstado;
                    profile.uprCiudad = upr.uprCiudad;

                    profile.codeIdPais = upr.codeIdPais;
                    profile.codeIdEstado = upr.codeIdEstado;
                    profile.codeIdCiudad = upr.codeIdCiudad;

                    profile.cp = upr.cp;
                    profile.uprTelefono = upr.uprTelefono;
                    profile.uprPerfilPrincipal = true;
                    profile.uprSubPerfil = false;

                        dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return false;
        }

        //public bool AddUserToRole(ApplicationUserManager userManager, string userId, string roleId)
        //{
        //    try
        //    {
        //        var rolesForUser = userManager.GetRoles(userId).ToArray();
        //        userManager.RemoveFromRoles(userId, rolesForUser);
        //        var result = userManager.AddToRole(userId, dbApp.Roles.Where(r => r.Id.Equals(roleId)).FirstOrDefault().Name);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        /// <summary>
        /// Actualiza la validación de correo electronico
        /// </summary>
        /// <param name="usr"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setUpdateEmailValidation(ApplicationUser usr, string code)
        {
            try
            {
                var user = getUserById(usr.Id);
                user.EmailConfirmed = false;
                user.usuEmailValidationCode = code;
                user.usuEmailValidationCodeEndDateUtc = DateTime.Now.AddDays(10);
                dbApp.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Borra la validación de correo electronico
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        public bool setClearEmailValidation(ApplicationUser usr)
        {
            try
            {
                var user = getUserById(usr.Id);
                usr.EmailConfirmed = true;
                user.usuEmailValidationCode = null;
                user.usuEmailValidationCodeEndDateUtc = null;
                dbApp.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza la recuperación de contraseña del usuario
        /// </summary>
        /// <param name="usr"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setUpdatePasswordRecoveryCode(ApplicationUser usr, string code)
        {
            try
            {
                var user = getUserById(usr.Id);
                user.usuPasswordRecoveryCode = code;
                user.usuPasswordRecoveryCodeEndDateUtc = DateTime.Now.AddDays(10);
                dbApp.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Borra la recuperación de contraseña del usuario.
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        public bool setClearPasswordRecoveryCode(ApplicationUser usr)
        {
            try
            {
                var user = getUserById(usr.Id);
                user.usuPasswordRecoveryCode = null;
                user.usuPasswordRecoveryCodeEndDateUtc = null;
                dbApp.SaveChanges();
                               
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        #endregion

        #region Menu

        /// <summary>
        /// Agrega los menus a la base de datos
        /// </summary>
        /// <param name="menuList"></param>
        /// <param name="submenuList"></param>
        /// <returns></returns>
        public bool addMenus(List<schemaMenus> menuList, List<schemaSubMenus> submenuList)
        {
            try
            {
                dbApp.tblSubMenus.RemoveRange(dbApp.tblSubMenus.ToList());
                dbApp.tblMenus.RemoveRange(dbApp.tblMenus.ToList());
                dbApp.SaveChanges();

                dbApp.tblMenus.AddRange(menuList);
                dbApp.tblSubMenus.AddRange(submenuList);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Obtiene el menu del rol actual.
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<MenusViewModel> getMenusByRoleId(string roleId)
        {
            try
            {
                return (from men in dbApp.tblMenus
                        let smen = dbApp.tblSubMenus.Where(s => s.menId == men.menId).OrderBy(s => s.smeOrden).ToList()
                        where men.rolId == roleId
                          && men.menEstatus == true
                        select new MenusViewModel()
                        {
                            menu = men,
                            submenus = smen
                        })
                        .OrderBy(m => m.menu.menOrden)
                        .ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Obtiene el submenu del menu seleccionado.
        /// </summary>
        /// <param name="menId"></param>
        /// <returns></returns>
        public List<schemaSubMenus> getSubMenusByMenId(string menId)
        {
            try
            {
                return (from smen in dbApp.tblSubMenus
                        where smen.menId == menId
                        && smen.smeEstatus == true
                        select smen)
                        .OrderBy(s => s.smeOrden)
                        .ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        #endregion

        #region Tarifas

        #region TarifasTemp

        //public bool tempSetTarifasConceptos(schemaTarifasConceptos model)
        //{
        //    try
        //    {
        //        if(dbApp.tblTarifasConceptos.Where(m => m.tcoIdConcepto == model.tcoIdConcepto).Count() == 0)
        //            dbApp.tblTarifasConceptos.Add(model);
        //        dbApp.SaveChanges();
        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasFormasPago(schemaTarifasFormasPago model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasFormasPago.Where(m => m.tfpIdFormaPago == model.tfpIdFormaPago).Count() == 0)
        //        {
        //            dbApp.tblTarifasFormasPago.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasPeriodicidades(schemaTarifasPeriodicidades model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasPeriodicidades.Where(m => m.tpeIdPeriodicidad == model.tpeIdPeriodicidad).Count() == 0)
        //        {
        //            dbApp.tblTarifasPeriodicidades.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasTiposPago(schemaTarifasTiposPago model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasTiposPago.Where(m => m.ttpIdTipoPago == model.ttpIdTipoPago).Count() == 0)
        //        {
        //            dbApp.tblTarifasTiposPago.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasMetodosPago(schemaTarifasMetodosPago model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasMetodosPago.Where(m => m.tmpIdMetodoPago == model.tmpIdMetodoPago).Count() == 0)
        //        {
        //            dbApp.tblTarifasMetodosPago.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasConceptosFormasPagoList(schemaTarifasConceptosFormasPago model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasConceptosFormasPago
        //                    .Where(m => m.tcoIdConcepto == model.tcoIdConcepto
        //                            && m.tfpIdFormaPago == model.tfpIdFormaPago).Count() == 0)
        //        {
        //            dbApp.tblTarifasConceptosFormasPago.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasCfpPeriodicidadesList(schemaTarifasCfpPeriodicidades model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasCfpPeriodicidades
        //                    .Where(m => m.tcfpId == model.tcfpId
        //                            && m.tpeIdPeriodicidad == model.tpeIdPeriodicidad).Count() == 0)
        //        {
        //            dbApp.tblTarifasCfpPeriodicidades.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasCfppTiposPagoList(schemaTarifasCfppTiposPago model)
        //{
        //    try
        //    {
        //        if (dbApp.tblTarifasCfppTiposPago
        //                    .Where(m => m.tcfppId == model.tcfppId
        //                            && m.ttpIdTipoPago == model.ttpIdTipoPago).Count() == 0)
        //        {
        //            dbApp.tblTarifasCfppTiposPago.Add(model);
        //            dbApp.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        //public bool tempSetTarifasMetodosPagoList(List<schemaTarifasCfpptpMetodosPago> model)
        //{
        //    try
        //    {
        //        foreach(var item in model)
        //        {
        //            if (dbApp.tblTarifasCfpptpMetodosPago
        //                     .Where(m => m.tcfpptpId == item.tcfpptpId
        //                              && m.tmpIdMetodoPago == item.tmpIdMetodoPago).Count() == 0)
        //            {
        //                dbApp.tblTarifasCfpptpMetodosPago.Add(item);
        //                dbApp.SaveChanges();
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return false;
        //}

        #endregion

        /// <summary>
        /// Obtiene los conceptos activos de las tarifas
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> getFeesConcepto()
        {
            return dbApp.tblTarifasConceptos
                        .Where(m => m.tcoEstatus == true)
                        .OrderBy(m => m.tcoIdConcepto)
                        .Select(m => new SelectListItem()
                        {
                            Text = m.tcoIdConcepto,
                            Value = m.tcoIdConcepto
                        })
                        .ToList();
        }

        /// <summary>
        /// Obtiene las formas de pago activos que son del concepto.
        /// </summary>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public List<SelectListItem> getFeesFormaPago(string concepto)
        {
            return dbApp.tblTarifasConceptosFormasPago
                        .Where(m => m.tblTarifasFormaPago.tfpEstatus == true
                                 && m.tcfpEstatus == true
                                 && m.tcoIdConcepto == concepto)
                        .OrderBy(m => m.tfpIdFormaPago)
                        .Select(m => new SelectListItem()
                        {
                            Text = m.tfpIdFormaPago,
                            Value = m.tcfpId.ToString()
                        })
                        .ToList();
        }

        /// <summary>
        /// Obtiene las periodicidades de pagos activas dependiendo de la forma de pago y concepto.
        /// </summary>
        /// <param name="tcfpId"></param>
        /// <returns></returns>
        public List<SelectListItem> getFeesPeriodicidad(int tcfpId)
        {
            return dbApp.tblTarifasCfpPeriodicidades
                        .Where(m => m.tblTarifasPeriodicidad.tpeEstatus == true
                                 && m.tcfppEstatus == true
                                 && m.tcfpId == tcfpId)
                        .OrderBy(m => m.tpeIdPeriodicidad)
                        .Select(m => new SelectListItem()
                        {
                            Text = m.tpeIdPeriodicidad,
                            Value = m.tcfppId.ToString()
                        })
                        .ToList();
        }

        /// <summary>
        /// Obtiene los tipos de pagos activos de la periodicidad seleccionada.
        /// </summary>
        /// <param name="tcfppId"></param>
        /// <returns></returns>
        public List<SelectListItem> getFeesTipoPago(int tcfppId)
        {
            return dbApp.tblTarifasCfppTiposPago
                        .Where(m => m.tblTarifasTipoPago.ttpEstatus == true
                                 && m.tcfpptpEstatus == true
                                 && m.tcfppId == tcfppId)
                        .OrderBy(m => m.ttpIdTipoPago)
                        .Select(m => new SelectListItem()
                        {
                            Text = m.ttpIdTipoPago,
                            Value = m.tcfpptpId.ToString()
                        })
                        .ToList();
        }

        /// <summary>
        /// Obtiene los metodos de pago que existen para el tipo de pago.
        /// </summary>
        /// <param name="tcfpptpId"></param>
        /// <returns></returns>
        public List<SelectListItem> getFeesMetodoPago(int tcfpptpId)
        {
            return dbApp.tblTarifasCfpptpMetodosPago
                        .Where(m => m.tblTarifasMetodoPago.tmpEstatus == true
                                 && m.tcfpptpmpEstatus == true
                                 && m.tcfpptpId == tcfpptpId)
                        .OrderBy(m => m.tmpIdMetodoPago)
                        .Select(m => new SelectListItem()
                        {
                            Text = m.tmpIdMetodoPago,
                            Value = m.tcfpptpmpId.ToString()
                        })
                        .ToList();
        }

        /// <summary>
        /// Obtiene el metodo de pago por su id.
        /// </summary>
        /// <param name="tcfpptpmpId"></param>
        /// <returns></returns>
        public schemaTarifasCfpptpMetodosPago getFeesMetodoPagoById(int tcfpptpmpId)
        {
            return dbApp.tblTarifasCfpptpMetodosPago
                        .Where(m => m.tcfpptpmpId == tcfpptpmpId)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Regresa el tipo de pago que esta relacionado a una periodicidad
        /// </summary>
        /// <param name="tcfpptpId"></param>
        /// <returns></returns>
        public schemaTarifasCfppTiposPago getTTarifasCfppTiposPagoById(int tcfpptpId)
        {
            return dbApp.tblTarifasCfppTiposPago
                        .Where(m => m.tcfpptpId == tcfpptpId)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Obtiene el metodo de pago que este activo y se va a sustituir por el nuevo metodo de pago y costo
        /// </summary>
        /// <param name="metodoPagoId"></param>
        /// <returns></returns>
        public List<schemaTarifas> getFeeToCancel(int metodoPagoId)
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tarEstatus == true
                       && tar.tcfpptpmpId == metodoPagoId
                    select tar
                   ).ToList();
        }

        /// <summary>
        /// Guarda una nueva tarifa.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setFees(TarifasViewModel model)
        {
            try
            {
                var modelToCancel = getFeeToCancel(model.tarMetodoPago);
                modelToCancel.ForEach(m => m.tarEstatus = false);

                var metodoPago = getFeesMetodoPagoById(model.tarMetodoPago);
                bool esPorcentaje = false;
                if (metodoPago.tblTarifasCfppTipoPago
                              .tblTarifasCfpPeriodicidad
                              .tblTarifasConceptoFormaPago
                              .tfpIdFormaPago == "COMISION")
                    esPorcentaje = true;

                schemaTarifas tarifa = new schemaTarifas();
                tarifa.tcfpptpmpId = model.tarMetodoPago;
                if (!esPorcentaje)
                    tarifa.tarCosto = (decimal)model.tarPrecio;
                else
                    tarifa.tarCosto = (decimal)model.tarPorcentajeComision;
                tarifa.tarEsPorcentaje = esPorcentaje;

                dbApp.tblTarifas.Add(tarifa);
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Obtiene las tarifas activas para mostrarse en el grid de la pantalla.
        /// </summary>
        /// <returns></returns>
        public List<TarifasGridViewModel> getFees()
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tarEstatus == true
                    select new TarifasGridViewModel()
                    {
                        tarId = tar.tarId,
                        tarConcepto = tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tblTarifasConceptoFormaPago.tcoIdConcepto,
                        tarFormaPago = tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tblTarifasConceptoFormaPago.tfpIdFormaPago,
                        tarPeriodicidad = tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tpeIdPeriodicidad,
                        tarTipoPago = tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.ttpIdTipoPago,
                        tarMetodoPago = tar.tblTarifasCfpptpMetodoPago.tmpIdMetodoPago,
                        tarCosto = tar.tarCosto,
                        tarEsPorcentaje = tar.tarEsPorcentaje,
                        tarFechaRegistroUTC = tar.tarFechaRegistroUTC
                    }
                   ).ToList();
        }

        /// <summary>
        /// Obtiene las tarifas que se pueden tener apartir de una forma de pago y periodicidad seleccionada.
        /// </summary>
        /// <param name="tfpIdFormaPago"></param>
        /// <param name="tpeIdPeriodicidad"></param>
        /// <returns></returns>
        public List<schemaTarifas> getFeesByFormaPagoPeriodicidadPago(string tfpIdFormaPago, string tpeIdPeriodicidad)
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tarEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tblTarifasConceptoFormaPago.tfpIdFormaPago == tfpIdFormaPago
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tpeIdPeriodicidad == tpeIdPeriodicidad
                    select tar
                   ).ToList();
        }

        /// <summary>
        /// Regresa la tarifa con mayor costo que puede tener una periodicidad
        /// </summary>
        /// <param name="tcfppId"></param>
        /// <returns></returns>
        public schemaTarifas getMaxFeesByPeriodicidad(int tcfppId)
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tblTarifasCfpPeriodicidad.tcfppId == tcfppId
                       && tar.tarEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tcfpptpmpEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tcfpptpEstatus == true
                    select tar)
                    .OrderByDescending(t => t.tarCosto)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Obtiene la tarifa especifica
        /// </summary>
        /// <returns></returns>
        public schemaTarifas getFeeById(int tarId)
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tarId == tarId
                    select tar).FirstOrDefault();
        }

        /// <summary>
        /// Regresa las tarifas de pago que estan habilitados para la periodicidad
        /// </summary>
        /// <returns></returns>
        public List<schemaTarifasCfppTiposPago> getTarifasTiposPagoByPeriodicidad(int tcfppId)
        {
            return dbApp.tblTarifasCfppTiposPago
                        .Where(m => m.tcfppId == tcfppId)
                        .OrderBy(m => m.ttpIdTipoPago)
                        .ToList();
        }

        /// <summary>
        /// Obtiene las tarifas que se pueden tener tipo de pago
        /// </summary>
        /// <param name="tcfpptpId"></param>
        /// <returns></returns>
        public List<schemaTarifas> getTarifasByTarifasCfppTiposPagoId(int tcfpptpId)
        {
            return (from tar in dbApp.tblTarifas
                    where tar.tarEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tcfpptpId == tcfpptpId
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasCfppTipoPago.tcfpptpEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tcfpptpmpEstatus == true
                       && tar.tblTarifasCfpptpMetodoPago.tblTarifasMetodoPago.tmpEstatus == true
                    select tar
                   ).ToList();
        }

        /// <summary>
        /// Regresa las tarifas que tiene el torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        public List<schemaTorneoTarifas> getFeesByTorneoId(int torId)
        {
            return (from tta in dbApp.tblTorneoTarifas.Include(t => t.tblTarifas)
                    where tta.torId == torId
                    select tta
                   ).ToList();
        }

        #endregion

        #region Liga

        /// <summary>
        /// Elimina y guarda nuevamente la lista de tipos de ligas (Escuela, Club, etc)
        /// </summary>
        /// <param name="categoriasList"></param>
        /// <returns></returns>
        public bool setLigaCategorias(List<schemaLigaCategorias> categoriasList)
        {
            try
            {
                dbApp.tblLigaCategorias.RemoveRange(dbApp.tblLigaCategorias.ToList());
                dbApp.SaveChanges();

                dbApp.tblLigaCategorias.AddRange(categoriasList);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Obtiene los tipos de ligas que esten activos.
        /// </summary>
        /// <returns></returns>
        public List<schemaLigaCategorias> getLigaCategorias_Active()
        {
            return dbApp.tblLigaCategorias
                        .Where(c => c.lcaEstatus == true)
                        .OrderBy(c => c.lcaOrden)
                        .ToList();
        }

        /// <summary>
        /// Obtiene las formas de pago de las tarifas que corresponden a la categoria seleccionada
        /// </summary>
        /// <param name="lcaId"></param>
        /// <returns></returns>
        public List<schemaTarifasFormasPago> getTarifasFormasPagoByLigaCategoria(string lcaId)
        {
            return (from tfp in dbApp.tblTarifasFormasPago
                    join lctfp in dbApp.tblLigaCategoriasTarifasFormasPago on tfp.tfpIdFormaPago equals lctfp.tfpIdFormaPago
                    where lctfp.lcaId == lcaId
                    select tfp)
                    .Distinct()
                    .ToList();
        }

        /// <summary>
        /// Obtiene las periodicidades que existen para la forma de pago
        /// </summary>
        /// <param name="tfpIdFormaPago"></param>
        /// <returns></returns>
        public List<schemaTarifasPeriodicidades> getTarifasPeriodicidadesByFormaPago(string tfpIdFormaPago)
        {
            var data =  (from tpe in dbApp.tblTarifasPeriodicidades
                        join tcfpp in dbApp.tblTarifasCfpPeriodicidades on tpe.tpeIdPeriodicidad equals tcfpp.tpeIdPeriodicidad
                        where tcfpp.tblTarifasConceptoFormaPago.tfpIdFormaPago == tfpIdFormaPago 
                        select tpe)
                        .Distinct()
                        .ToList();
            return data;
        }

        public List<schemaLigas> getLigasByName(string ligName)
        {
            var ligas = dbApp.tblLigas.Where(l => l.ligNombreLiga.ToUpper().Trim() == ligName.ToUpper().Trim()).ToList();
            return ligas;
        }
        /// <summary>
        /// Metodo para guardar la solicitud de una nueva liga.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setAddLeagueRequest(LeagueRegisterViewModel model)
        {
            try
            {
                ApplicationUser user = getUserByUserName(model.lreUserProfile.usuUsername);
                if (user != null)
                {
                    var profile = getUserMainProfile(user.Id);
                    if (profile == null)
                    {
                        profile = new schemaUsersProfiles();
                        profile.userIdOwner = user.Id;
                        profile.uprId = 1;
                        dbApp.tblUsersProfiles.Add(profile);
                    }
                    //Guarda perfil del usuario
                    if (model.lreUserProfile.imgURL != null)
                        profile.uprProfileImageURL = model.lreUserProfile.imgURL;
                    var nombreCompleto = Global_Functions.getName_LastName(model.lreUserProfile.usuNombreCompleto);
                    profile.uprNombres = (nombreCompleto["name"] == "") ? "-" : nombreCompleto["name"].Trim();
                    profile.uprApellidos = (nombreCompleto["lastname"] == "") ? "-" : nombreCompleto["lastname"].Trim();
                    profile.uprGenero = model.lreUserProfile.usuGenero;
                    profile.uprFechaNacimiento = Global_Functions.stringToDate(model.lreUserProfile.usuFechaNacimiento);
                    profile.uprCiudad = model.lreUserProfile.usuCiudad;
                    profile.uprTelefono = model.lreUserProfile.usuTelefono;
                    user.Email = model.lreUserProfile.usuCorreo;

                    //Guarda liga
                    schemaLigas liga = new schemaLigas();
                    liga.ligUserIdCreador = user.Id;
                    liga.ligTipoLiga = model.lreTipoLiga;
                    //liga.tcfppId = model.tcfppId;
                    liga.ligImgUrl = model.lreImgUrl;
                    liga.ligNombreLiga = model.lreNombreLiga.Trim();
                    liga.ligCorreoContacto = model.lreCorreoContacto.Trim();
                    liga.ligTelefonoContacto = model.lreTelefonoContacto.Trim();
                    liga.ligDescripcion = model.lreDescripcion.Trim();
                    liga.ligLatitud = model.lreLigaLatitud;
                    liga.ligLongitud = model.lreLigaLongitud;
                    dbApp.tblLigas.Add(liga);

                    //Revisa Co-Administradores y los guarda
                    //Cuando se registra una solicitud no hay coadministradores
                    //if (model.lreLeagueCoAdmins != null && 
                    //    model.lreLeagueCoAdmins.lcaEmailList != null &&
                    //    model.lreLeagueCoAdmins.lcaEmailList.Count > 0)
                    //{
                    //    foreach (var item in model.lreLeagueCoAdmins.lcaEmailList)
                    //    {
                    //        schemaLigaCoAdministradores ligaCoAdmin = new schemaLigaCoAdministradores();
                    //        ligaCoAdmin.ligCorreoContacto = item.lcaEmail.Trim();
                    //        ligaCoAdmin.tblLigas = liga;

                    //        dbApp.tblLigaCoAdministradores.Add(ligaCoAdmin);
                    //    }
                    //}

                    //Guarda los datos de facturación
                    if (model.lreTaxData.tdaSaveTda)
                    {
                        schemaLigaDatosFiscales ligaDF = new schemaLigaDatosFiscales();
                        ligaDF.ldfRFC = model.lreTaxData.tdaRFC.Trim();
                        ligaDF.ldfRazonSocial = model.lreTaxData.tdaRazonSocial.Trim();
                        ligaDF.ldfDomicilio = model.lreTaxData.tdaDomicilio.Trim();
                        ligaDF.ldfNumeroExtInt = model.lreTaxData.tdaNumeroExtInt.Trim();
                        ligaDF.ldfColonia = model.lreTaxData.tdaColonia.Trim();
                        ligaDF.ldfMunicipio = model.lreTaxData.tdaMunicipio.Trim();
                        ligaDF.ldfEstado = model.lreTaxData.tdaEstado.Trim();
                        ligaDF.ldfCodigoPostal = model.lreTaxData.tdaCodigoPostal.Trim();
                        ligaDF.tblLigas = liga;

                        dbApp.tblLigaDatosFiscales.Add(ligaDF);
                    }

                    //Guarda la dirección comercial de la liga
                    schemaLigaDireccionComercial ligaDC = new schemaLigaDireccionComercial();
                    ligaDC.ldcDomicilio = model.lreBusinessAddress.badDomicilio.Trim();
                    ligaDC.ldcNumeroExtInt = model.lreBusinessAddress.badNumeroExtInt.Trim();
                    ligaDC.ldcColonia = model.lreBusinessAddress.badColonia.Trim();
                    ligaDC.ldcMunicipio = model.lreBusinessAddress.badMunicipio.Trim();
                    ligaDC.ldcEstado = model.lreBusinessAddress.badEstado.Trim();
                    ligaDC.ldcCodigoPostal = model.lreBusinessAddress.badCodigoPostal.Trim();
                    ligaDC.tblLigas = liga;


                    dbApp.tblLigaDireccionComercial.Add(ligaDC);
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa las solicitudes de liga que no hayan sido aprobadas ni rechazadas.
        /// </summary>
        /// <returns></returns>
        public List<RequestLeaguesViewModel> getLeaguesRequestNew()
        {
            return (from lig in dbApp.tblLigas
                    let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == lig.ligUserIdCreador).FirstOrDefault()
                    let nombre = (prof != null) ? prof.uprNombres.Trim() + " " + prof.uprApellidos.Trim() : ""
                    where lig.ligSolicitud == true
                    && lig.ligAprobada == false && lig.ligEstatus == true
                    select new RequestLeaguesViewModel()
                    {
                        ligId = lig.ligId,
                        ligNombreLiga = lig.ligNombreLiga,
                        ligTipoLiga = lig.ligTipoLiga,
                        ligNombreCreador = nombre,
                        ligCreadorId = lig.ligUserIdCreador,
                        ligFechaRegistro = lig.ligFechaRegistroUTC,
                        ligFormaPago = "COMISION",
                        ligEstatus = lig.ligEstatus,
                        ligAprobada = lig.ligAprobada,
                        ligSolicitud = lig.ligSolicitud,
                        ligSolicitudRevisada = lig.ligSolicitudRevisada
                    }
                    )
                    .OrderByDescending(l => l.ligFechaRegistro)
                    .ToList();
        }
        /// <summary>
        /// Regresa las ligas dependiendo de los filtros
        /// </summary>
        /// <param name="fechaIni"></param>
        /// <param name="fechafin"></param>
        /// <param name="filtroEstatus">
        /// 0 = Nuevas (No revisadas)
        /// 1 = Aprobadas
        /// 2 = Todas
        /// 3 = Rechazadas
        /// </param>
        /// <returns></returns>
        public List<RequestLeaguesViewModel> getLeagueGridStatus()
        {
            return (from lig in dbApp.tblLigas
                    let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == lig.ligUserIdCreador).FirstOrDefault()
                    let nombre = (prof != null) ? prof.uprNombres.Trim() + " " + prof.uprApellidos.Trim() : ""
                    where lig.ligNotificacion == true
                    select new RequestLeaguesViewModel()
                    {
                        ligId = lig.ligId,
                        ligNombreLiga = lig.ligNombreLiga,
                        ligTipoLiga = lig.ligTipoLiga,
                        ligNombreCreador = nombre,
                        ligCreadorId = lig.ligUserIdCreador,
                        ligFechaRegistro = lig.ligFechaRegistroUTC,
                        ligFormaPago = "COMISION",
                        ligEstatus = lig.ligEstatus,
                        ligAprobada = lig.ligAprobada,
                        ligSolicitud = lig.ligSolicitud,
                        ligSolicitudRevisada = lig.ligSolicitudRevisada,
                        viewStatus = (lig.ligAprobada==true && lig.ligSolicitudRevisada == true) ? "APROBADA":(lig.ligAprobada ==false && lig.ligEstatus == false)?"RECHAZADO": "EN PROCESO"
                    }
                    )
                    .OrderByDescending(l => l.ligFechaRegistro)
                    .ToList();
        }

        public bool setNotificationLeague(int ligId)
        {
            try
            {
                var liga = getLigaById(ligId);
                if (liga!=null)
                {
                    liga.ligNotificacion = false;
                    dbApp.SaveChanges();
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

        public List<RequestLeaguesViewModel> getLeaguesRequest(DateTime fechaIni, DateTime fechafin, int filtroEstatus)
        {
            var ligas = (from lig in dbApp.tblLigas
                         let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == lig.ligUserIdCreador).FirstOrDefault()
                         let nombre = (prof != null) ? prof.uprNombres.Trim() + " " + prof.uprApellidos.Trim() : ""
                         where lig.ligSolicitud == true
                            && lig.ligFechaRegistroUTC >= fechaIni && lig.ligFechaRegistroUTC < fechafin
                         select new RequestLeaguesViewModel()
                         {
                             ligId = lig.ligId,
                             ligNombreLiga = lig.ligNombreLiga,
                             ligTipoLiga = lig.ligTipoLiga,
                             ligNombreCreador = nombre,
                             ligCreadorId = lig.ligUserIdCreador,
                             ligFechaRegistro = lig.ligFechaRegistroUTC,
                             ligFormaPago = "COMISION",
                             ligEstatus = lig.ligEstatus,
                             ligAprobada = lig.ligAprobada,
                             ligSolicitud = lig.ligSolicitud,
                             ligSolicitudRevisada = lig.ligSolicitudRevisada
                         }
                        )
                        .OrderBy(l => l.ligNombreLiga)
                        .ToList();

            switch (filtroEstatus)
            {
                case 0:
                    ligas = ligas.Where(l => l.ligAprobada == false && l.ligEstatus == true).ToList();
                    break;
                case 1:
                    ligas = ligas.Where(l => l.ligAprobada == true && l.ligEstatus == true).ToList();
                    break;
                case 3:
                    ligas = ligas.Where(l => l.ligEstatus == false).ToList();
                    break;
            }

            return ligas;
        }

        /// <summary>
        /// Regresa el modelo para ver la solicitud de la liga para ser aprobada o rechazada.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public RequestDetailViewModel getLeagueRequestById(int Id)
        {
            var masterModel = new RequestDetailViewModel();
            var model = masterModel.league;

            try
            {
                var liga = getLigaById(Id);
                if(liga != null)
                {
                    liga.ligSolicitudRevisada = true;
                    dbApp.SaveChanges();

                    model.lreId = liga.ligId;

                    //User Profile - Creador
                    ApplicationUser usr = null;
                    var userProfile = getUserMainProfile(liga.ligUserIdCreador);
                    if(userProfile == null)
                    {
                        userProfile = new schemaUsersProfiles();
                        usr = getUserById(liga.ligUserIdCreador);
                    }
                    model.lreUserProfile.usuUsername = (userProfile.tblUsers != null) ? userProfile.tblUsers.UserName : usr.UserName;
                    model.lreUserProfile.imgURL = userProfile.uprProfileImageURL;
                    model.lreUserProfile.usuNombreCompleto = ((userProfile.uprNombres == "-") ? "" : userProfile.uprNombres.Trim()) + " " + ((userProfile.uprApellidos == "-") ? "" : userProfile.uprApellidos.Trim());
                    model.lreUserProfile.usuGenero = (userProfile.uprGenero != null) ? userProfile.uprGenero : "";
                    model.lreUserProfile.usuFechaNacimiento = (userProfile.uprFechaNacimiento != null) ? ((DateTime)userProfile.uprFechaNacimiento).ToString(constClass.formatDateOnly) : "";
                    model.lreUserProfile.usuCiudad = (userProfile.uprCiudad != null) ? userProfile.uprCiudad.Trim() : "";
                    model.lreUserProfile.usuTelefono = (userProfile.uprTelefono != null) ? userProfile.uprTelefono.Trim() : "";
                    model.lreUserProfile.usuCorreo = (userProfile.tblUsers != null) ? userProfile.tblUsers.Email : usr.Email;

                    //Co-administradores
                    //model.lreLeagueCoAdmins.lcaEmail = new LeagueCoAdministratorsViewModel();
                    //model.lreLeagueCoAdmins.lcaEmailList = dbApp.tblLigaCoAdministradores
                    //                                            .Where(l => l.ligId == liga.ligId)
                    //                                            .Select(l => new LeagueCoAdministratorsViewModel(){ lcaEmail = l.ligCorreoContacto })
                    //                                            .ToList();

                    //Tax Address
                    model.lreTaxData = dbApp.tblLigaDatosFiscales
                                            .Where(l => l.ligId == liga.ligId)
                                            .OrderByDescending(l => l.ldfId)
                                            .Select(l => new TaxDataViewModel() { 
                                                tdaRFC = l.ldfRFC,
                                                tdaRazonSocial = l.ldfRazonSocial,
                                                tdaDomicilio = l.ldfDomicilio,
                                                tdaNumeroExtInt = l.ldfNumeroExtInt,
                                                tdaColonia = l.ldfColonia,
                                                tdaMunicipio = l.ldfMunicipio,
                                                tdaEstado = l.ldfEstado,
                                                tdaCodigoPostal = l.ldfCodigoPostal
                                             })
                                            .FirstOrDefault();

                    //Datos de dirección comercial
                    model.lreBusinessAddress = dbApp.tblLigaDireccionComercial
                                               .Where(l => l.ligId == liga.ligId)
                                               .OrderByDescending(l => l.ldcId)
                                               .Select(l => new BusinessAddressViewModel()
                                               {
                                                   badDomicilio = l.ldcDomicilio,
                                                   badNumeroExtInt = l.ldcNumeroExtInt,
                                                   badColonia = l.ldcColonia,
                                                   badMunicipio = l.ldcMunicipio,
                                                   badEstado = l.ldcEstado,
                                                   badCodigoPostal = l.ldcCodigoPostal
                                               })
                                               .FirstOrDefault();

                    //Datos de la liga
                    model.lreTipoLiga = liga.ligTipoLiga;
                    //model.lreFormaPago = liga.tblTarifasPeriodicidad.tblTarifasConceptoFormaPago.tfpIdFormaPago;
                    //model.lrePeriodicidadPago = liga.tblTarifasPeriodicidad.tpeIdPeriodicidad;
                    //model.tcfppId = liga.tcfppId;

                    model.lreImgUrl = liga.ligImgUrl;
                    model.lreNombreLiga = liga.ligNombreLiga;
                    model.lreCorreoContacto = liga.ligCorreoContacto;
                    model.lreTelefonoContacto = liga.ligTelefonoContacto;
                    model.lreDescripcion = liga.ligDescripcion;

                    //model.lrePorcentajeDescuento = liga.ligPorcentajeDescuento;
                    if (liga.tarId != null)
                    {
                        //masterModel.fee = liga.tblTarifa;
                        //model.lreTotalPagar = liga.ligTotalPagar;
                    }
                    else
                    {
                        //masterModel.fee = getMaxFeesByPeriodicidad(liga.tcfppId);

                        //if (model.lrePorcentajeDescuento == 0)
                        //    model.lreTotalPagar = masterModel.fee.tarCosto;
                        //else
                        //{
                        //    decimal importe = (masterModel.fee.tarCosto * liga.ligPorcentajeDescuento) / 100;
                        //    model.lreTotalPagar = masterModel.fee.tarCosto + importe;
                       // }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return masterModel;
        }

        /// <summary>
        /// Regresa los detalles de una liga.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public schemaLigas getLigaById(int Id)
        {
            return (from lig in dbApp.tblLigas
                    where lig.ligId == Id
                    select lig).FirstOrDefault();
        }
        public List<schemaLigas> getLigaByUser(string userId)
        {
            return (from lig in dbApp.tblLigas
                   join lca in dbApp.tblLigaCoAdministradores on lig.ligId equals lca.ligId into lig_lca
                   from lca in lig_lca.DefaultIfEmpty()
                   where lig.ligUserIdCreador == userId || lca.lcaUserId == userId
                   select lig).ToList();
        }
        /// <summary>
        /// Realiza la aceptación de la liga y agrega el rol de dueño de liga al usuario creador.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLeagueRequestAccept(ApplicationUserManager userManager, RequestDetailViewModel model)
        {
            try
            {
                var liga = getLigaById(model.league.lreId);
                if (liga != null)
                {
                   // liga.tarId = model.fee.tarId;
                    liga.ligAprobada = true;
                    liga.ligEstatus = true;
                    //liga.ligPorcentajeDescuento = model.league.lrePorcentajeDescuento;
                    //liga.ligTotalPagar = model.league.lreTotalPagar;
                    dbApp.SaveChanges();

                    if (!userManager.IsInRole(liga.ligUserIdCreador, constClass.rolOwners))
                        userManager.AddToRole(liga.ligUserIdCreador, constClass.rolOwners);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Rechaza una solicitud cambiandole el estatus y la aprobación.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool setLeagueRequestReject(int Id)
        {
            try
            {
                var liga = getLigaById(Id);
                if (liga != null)
                {
                    liga.ligAprobada = false;
                    liga.ligEstatus = false;
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa las ligas que ya fueron aprobadas dependiendo de los filtros
        /// </summary>
        /// <param name="fechaIni"></param>
        /// <param name="fechafin"></param>
        /// <param name="ligTipoLiga"></param>
        /// <returns></returns>
        public List<RequestLeaguesViewModel> getLeaguesRegistered(DateTime fechaIni, DateTime fechafin, string ligTipoLiga)
        {
            var ligas = (from lig in dbApp.tblLigas
                         let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == lig.ligUserIdCreador).FirstOrDefault()
                         let nombre = (prof != null) ? (prof.uprNombres + " " + prof.uprApellidos).Trim() : ""
                         where lig.ligAprobada == true
                            && lig.ligEstatus == true
                            && lig.ligFechaRegistroUTC >= fechaIni && lig.ligFechaRegistroUTC < fechafin
                         select new RequestLeaguesViewModel()
                         {
                             ligId = lig.ligId,
                             ligNombreLiga = lig.ligNombreLiga,
                             ligTipoLiga = lig.ligTipoLiga,
                             ligNombreCreador = nombre,
                             ligCreadorId = lig.ligUserIdCreador,
                             ligFechaRegistro = lig.ligFechaRegistroUTC,
                             ligFormaPago = "COMISION",
                             ligEstatus = lig.ligEstatus,
                             ligAprobada = lig.ligAprobada,
                             ligSolicitud = lig.ligSolicitud,
                             ligSolicitudRevisada = lig.ligSolicitudRevisada
                         }
                        )
                        .OrderBy(l => l.ligNombreLiga)
                        .ToList();

           if(ligTipoLiga != "")
               ligas = ligas.Where(l => l.ligTipoLiga.ToUpper().Trim() == ligTipoLiga.ToUpper().Trim()).ToList();
           

           return ligas;
        }
        public List<schemaLigas> getLigasByDeportes(string depNombre, string userId)
        {
            var deportes = (from lig in dbApp.tblLigas
                            join lca in dbApp.tblLigaCoAdministradores on lig.ligId equals lca.ligId into lig_lca
                            from lca in lig_lca.DefaultIfEmpty()
                            join lcat in dbApp.tblLigaCategoriasTorneos on lig.ligId equals lcat.ligId
                            where (lig.ligUserIdCreador == userId
                            || lca.lcaUserId == userId )
                            && lcat.depNombre == depNombre
                            select lig)
                            .Distinct()
                            .ToList();
            return deportes;
        }
        /// <summary>
        /// Regresa las ligas activas del usuario especifico, regresa el id del usuario del creador para validar que no cambien el javascript
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<LeaguesActiveLOwnerViewModel> getUserLeagues(string userId, string roleId)
        {
            List<schemaLigas> ligas = new List<schemaLigas>();
          
            ligas = (from lig in dbApp.tblLigas
                        join lca in dbApp.tblLigaCoAdministradores on lig.ligId equals lca.ligId into lig_lca
                        from lca in lig_lca.DefaultIfEmpty()
                        where (lig.ligUserIdCreador == userId
                        || lca.lcaUserId == userId)
                        && lig.ligEstatus == true
                        && lig.ligAprobada == true
                        select lig)
                    .Distinct()
                    .ToList();

            return (from lig in ligas
                    let mainLeague = getMainLeague_IsMain(lig.ligId, userId, roleId)
                    let _domCom = dbApp.tblLigaDireccionComercial.Where(l => l.ligId == lig.ligId).OrderByDescending(l => l.ldcId).FirstOrDefault()
                    let _domicilio = (_domCom == null) ? "" : _domCom.ldcDomicilio + " " + _domCom.ldcNumeroExtInt + ", " +
                                                              "Col. " + _domCom.ldcColonia + " " + _domCom.ldcMunicipio + ", " +
                                                              _domCom.ldcEstado + ", C.P. " + _domCom.ldcCodigoPostal
                    select new LeaguesActiveLOwnerViewModel()
                    {
                        ligId = lig.ligId,
                        ligImg = lig.ligImgUrl,
                        ligNombre = lig.ligNombreLiga,
                        ligDomicilio = _domicilio,
                        ligContacto = lig.ligCorreoContacto,
                        ligDescripcion = lig.ligDescripcion,
                        ligCreator = lig.ligUserIdCreador,
                        ligLatitud = lig.ligLatitud,
                        ligLongitud = lig.ligLongitud,
                        ligMainLeague = mainLeague
                    }).ToList();
        }

        public List<LeaguesActiveLOwnerViewModel> getRefereeLeagues(string userId)
        {
            List<schemaLigas> ligas = new List<schemaLigas>();
            var user = getUserById(userId);
            var partido = getPartidosRefeere(user).OrderBy(o=> o.ligId);

            foreach (var item in partido)
            {
                var liga = getLigaById(item.ligId);


                if (ligas.Count > 0)
                {
                    if (liga.ligId.ToString() != ligas.Last().ligId.ToString())
                    {
                        ligas.Add(liga);
                    }
                }
                else
                    ligas.Add(liga);

            }

            return (from lig in ligas
                    let _domCom = dbApp.tblLigaDireccionComercial.Where(l => l.ligId == lig.ligId).OrderByDescending(l => l.ldcId).FirstOrDefault()
                    let _domicilio = (_domCom == null) ? "" : _domCom.ldcDomicilio + " " + _domCom.ldcNumeroExtInt + ", " +
                                                              "Col. " + _domCom.ldcColonia + " " + _domCom.ldcMunicipio + ", " +
                                                              _domCom.ldcEstado + ", C.P. " + _domCom.ldcCodigoPostal
                    select new LeaguesActiveLOwnerViewModel()
                    {
                        ligId = lig.ligId,
                        ligImg = lig.ligImgUrl,
                        ligNombre = lig.ligNombreLiga,
                        ligDomicilio = _domicilio,
                        ligContacto = lig.ligCorreoContacto,
                        ligDescripcion = lig.ligDescripcion,
                        ligCreator = lig.ligUserIdCreador,
                        ligLatitud = lig.ligLatitud,
                        ligLongitud = lig.ligLongitud,
                    }).ToList();
        }

        /// <summary>
        /// Obtiene los datos de la liga para ser mostrados en su detalle
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="userIdCreador"></param>
        /// <returns></returns>
        public LeaguesActiveDetailViewModel getUserLeaguesDetailById(int ligId, string userIdCreador)
        {
            var model = new LeaguesActiveDetailViewModel();
            model.ligLiga = (from lig in dbApp.tblLigas
                             join lca in dbApp.tblLigaCoAdministradores on lig.ligId equals lca.ligId into lig_lca
                             from lca in lig_lca.DefaultIfEmpty()
                             let _domCom = dbApp.tblLigaDireccionComercial.Where(l => l.ligId == lig.ligId).OrderByDescending(l => l.ldcId).FirstOrDefault()
                             let _domicilio = (_domCom == null) ? "" : _domCom.ldcDomicilio + " " + _domCom.ldcNumeroExtInt + ", " +
                                                                        "Col. " + _domCom.ldcColonia + " " + _domCom.ldcMunicipio + ", " +
                                                                        _domCom.ldcEstado + ", C.P. " + _domCom.ldcCodigoPostal
                             where lig.ligId == ligId
                                && lig.ligUserIdCreador == userIdCreador
                             select new LeaguesActiveLOwnerViewModel()
                             {
                                ligId = lig.ligId,
                                ligImg = lig.ligImgUrl,
                                ligNombre = lig.ligNombreLiga,
                                ligDomicilio = _domicilio,
                                ligContacto = lig.ligCorreoContacto,
                                ligDescripcion = lig.ligDescripcion,
                                ligCreator = lig.ligUserIdCreador,
                                ligLatitud = lig.ligLatitud,
                                ligLongitud = lig.ligLongitud
                             })
                             .FirstOrDefault();

            bool directoryExists = false;
            string path = HttpContext.Current.Server.MapPath(constClass.urlLeaguesImages + "/" + ligId.ToString());
            if (!Directory.Exists(path))
            {
                DirectoryInfo result = Directory.CreateDirectory(path);
                if (result.Exists)
                    directoryExists = true;
            }
            else
                directoryExists = true;

            if (directoryExists)
            {
                string[] files = Global_Functions.getFolderFiles(path);
                foreach (string file in files)
                {
                    model.ligFiles.Add(new FileReferenceViewModel
                                        {
                                            fileUrl = file,
                                            fileExtension = Path.GetExtension(file)
                                        });
                }
            }

            return model;
        }

        /// <summary>
        /// Obtiene los ultimos datos fiscales registrados del usuario.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public schemaLigaDatosFiscales getLastLigaDatosFiscalesByUser(string userId)
        {
            return (from ldf in dbApp.tblLigaDatosFiscales
                    where ldf.tblLigas.ligUserIdCreador == userId
                    select ldf
                   ).OrderByDescending(l => l.ligId).ThenBy(l => l.ldfId)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Regresa los datos principales de una liga.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="userIdCreador"></param>
        /// <returns></returns>
        public LeagueDetail_MainData getLeagueMainDataById(int Id, string userIdCreador)
        {
            var model = new LeagueDetail_MainData();
            try
            {
                var liga = getLigaById(Id);
                //Si la llave no corresponde, no regresa nada
                if (liga.ligUserIdCreador != userIdCreador)
                    return null;

                if (liga != null)
                {
                    model.ligId = liga.ligId;

                    model.lreImgUrl = liga.ligImgUrl;
                    model.lreNombreLiga = liga.ligNombreLiga;
                    model.lreCorreoContacto = liga.ligCorreoContacto;
                    model.lreTelefonoContacto = liga.ligTelefonoContacto;
                    model.lreDescripcion = liga.ligDescripcion;
                    model.lreKey = liga.ligUserIdCreador;
                    model.lreStatus = liga.ligEstatus;
                    //Datos de dirección comercial
                    model.lreBusinessAddress = dbApp.tblLigaDireccionComercial
                                               .Where(l => l.ligId == liga.ligId)
                                               .OrderByDescending(l => l.ldcId)
                                               .Select(l => new BusinessAddressViewModel()
                                               {
                                                   badDomicilio = l.ldcDomicilio,
                                                   badNumeroExtInt = l.ldcNumeroExtInt,
                                                   badColonia = l.ldcColonia,
                                                   badMunicipio = l.ldcMunicipio,
                                                   badEstado = l.ldcEstado,
                                                   badCodigoPostal = l.ldcCodigoPostal
                                               })
                                               .FirstOrDefault();

                    //Co-administradores
                    //model.lreLeagueCoAdmins.lcaEmail = new LeagueCoAdministratorsViewModel();
                    //model.lreLeagueCoAdmins.lcaEmailList = dbApp.tblLigaCoAdministradores
                    //                                            .Where(l => l.ligId == liga.ligId)
                    //                                            .Select(l => new LeagueCoAdministratorsViewModel() { lcaEmail = l.ligCorreoContacto })
                    //                                            .ToList();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return model;
        }

        /// <summary>
        /// Regresa la ultima dirección comercial guardada de la liga
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public schemaLigaDireccionComercial getLastBusinessAddressByLeagueId(int Id) {
            return dbApp.tblLigaDireccionComercial.Where(l => l.ligId == Id).OrderByDescending(l => l.ldcId).FirstOrDefault();
        }

        /// <summary>
        /// Guarda la información principal de la liga.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLeagueMainDataById(LeagueDetail_MainData model)
        {
            try
            {
                var lig = getLigaById(model.ligId);
                if (lig != null)
                {
                    if (model.lreImgUrl != null)
                        lig.ligImgUrl = model.lreImgUrl;
                    lig.ligNombreLiga = model.lreNombreLiga;
                    lig.ligCorreoContacto = model.lreCorreoContacto;
                    lig.ligTelefonoContacto = model.lreTelefonoContacto;
                    lig.ligDescripcion = model.lreDescripcion;
                    lig.ligEstatus = model.lreStatus;
                    //Guarda la dirección comercial de la liga si es que hubo un cambio.
                    var lastBusinessAddress = getLastBusinessAddressByLeagueId(lig.ligId);
                    if (lastBusinessAddress.ldcDomicilio != model.lreBusinessAddress.badDomicilio.Trim() ||
                       lastBusinessAddress.ldcNumeroExtInt != model.lreBusinessAddress.badNumeroExtInt.Trim() ||
                       lastBusinessAddress.ldcColonia != model.lreBusinessAddress.badColonia.Trim() ||
                       lastBusinessAddress.ldcMunicipio != model.lreBusinessAddress.badMunicipio.Trim() ||
                       lastBusinessAddress.ldcEstado != model.lreBusinessAddress.badEstado.Trim() ||
                       lastBusinessAddress.ldcCodigoPostal != model.lreBusinessAddress.badCodigoPostal.Trim())
                    {
                        schemaLigaDireccionComercial ligaDC = new schemaLigaDireccionComercial();
                        ligaDC.ldcId = lastBusinessAddress.ldcId + 1;
                        ligaDC.ldcDomicilio = model.lreBusinessAddress.badDomicilio.Trim();
                        ligaDC.ldcNumeroExtInt = model.lreBusinessAddress.badNumeroExtInt.Trim();
                        ligaDC.ldcColonia = model.lreBusinessAddress.badColonia.Trim();
                        ligaDC.ldcMunicipio = model.lreBusinessAddress.badMunicipio.Trim();
                        ligaDC.ldcEstado = model.lreBusinessAddress.badEstado.Trim();
                        ligaDC.ldcCodigoPostal = model.lreBusinessAddress.badCodigoPostal.Trim();
                        ligaDC.tblLigas = lig;
                        dbApp.tblLigaDireccionComercial.Add(ligaDC);
                    }

                    ////Revisa Co-Administradores y los guarda
                    //if (model.lreLeagueCoAdmins != null &&
                    //    model.lreLeagueCoAdmins.lcaEmailList != null &&
                    //    model.lreLeagueCoAdmins.lcaEmailList.Count > 0)
                    //{
                    //    foreach (var item in model.lreLeagueCoAdmins.lcaEmailList)
                    //    {
                    //        schemaLigaCoAdministradores ligaCoAdmin = new schemaLigaCoAdministradores();
                    //        ligaCoAdmin.ligCorreoContacto = item.lcaEmail.Trim();
                    //        ligaCoAdmin.tblLigas = liga;

                    //        dbApp.tblLigaCoAdministradores.Add(ligaCoAdmin);
                    //    }
                    //}

                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa la lista de los coadministradores de la liga.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="UserIdCreator"></param>
        /// <returns></returns>
        public List<LeagueCoAdministratorsViewModel> getLeagueCoadministratorsById(int ligId, string UserIdCreator)
        {
            return (from lca in dbApp.tblLigaCoAdmnInit
                    let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.tblUsers.Email == lca.userEmail).FirstOrDefault()
                    let nombreCompleto = (prof != null) ? (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim() : ""
                    where lca.ligId == ligId
                       && lca.tblLigas.ligUserIdCreador == UserIdCreator
                    select new LeagueCoAdministratorsViewModel()
                    {
                        lcaUserId = prof.tblUsers.Id,
                        lcaNombre = nombreCompleto,
                        lcaEmail = prof.tblUsers.UserName,
                        lcaConfirmado = lca.lcaConfirmacion
                    })
                    .OrderBy(l => l.lcaNombre)
                    .ToList();
        }

        /// <summary>
        /// Guarda al coadministrador de la liga en la tabla
        /// </summary>
        /// <param name="ligaCoAdmin"></param>
        /// <returns></returns>
        public bool setLeagueCoadmin(schemaLigaCoAdminInit ligaCoAdmin)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdmnInit.Where(l => l.ligId == ligaCoAdmin.ligId && l.userEmail == ligaCoAdmin.userEmail).FirstOrDefault();
                if (coadmin == null)
                {
                    dbApp.tblLigaCoAdmnInit.Add(ligaCoAdmin);
                }
                else
                {
                    coadmin.lcaConfirmacion = false;
                    coadmin.lcaCodigoConfirmacion = ligaCoAdmin.lcaCodigoConfirmacion;
                }
                
                dbApp.SaveChanges();
                return true;   
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza el codigo que se va a enviar en el correo de confirmacion para coadministrador
        /// </summary>
        /// <param name="model"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setUpdateCoadminConfirmCode(schemaLigaCoAdministradores model, string code)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdministradores.Where(m => m.ligId == model.ligId && m.lcaUserId == model.lcaUserId).FirstOrDefault();
                coadmin.lcaCodigoConfirmacion = code;
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador de la liga validando la llave de seguridad (UserIdCreator de la liga)
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool setDeleteCoadmin(int ligId, string key, string userId)
        {
            try
            {
                var user = getUserById(userId);
                var coadmin = dbApp.tblLigaCoAdmnInit
                                   .Where(m => m.ligId == ligId && m.userEmail == user.Email).FirstOrDefault();
                if (coadmin != null)
                {
                    dbApp.tblLigaCoAdmnInit.Remove(coadmin);
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador de la liga cuando rechaza desde el correo.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool setDeleteCoadmin(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdmnInit
                                   .Where(m => m.userEmail== userEmail && m.lcaCodigoConfirmacion == code)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    dbApp.tblLigaCoAdmnInit.Remove(coadmin);
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa la liga en donde esta registrado el email y codigo que esta confirmando
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public schemaLigas getLigaByCoAdminCodeEmail(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdmnInit
                                   .Where(m => m.userEmail == userEmail && m.lcaCodigoConfirmacion == code).FirstOrDefault();
                if (coadmin != null)
                {
                    return coadmin.tblLigas;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }
        public schemaLigas getLigaByCoAdminIntCodeEmail(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdmnInit
                                   .Where(m => m.userEmail == userEmail && m.lcaCodigoConfirmacion == code).FirstOrDefault();
                if (coadmin != null)
                {
                    return coadmin.tblLigas;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }
        public List<schemaLigaCoAdministradores> getCoAdminLigasByLigIg(int ligId)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdministradores
                                   .Where(m => m.ligId == ligId).ToList();
                if (coadmin != null)
                {
                    return coadmin;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }
        /// <summary>
        /// Confirma la coadministración de la liga.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        /// 
        public bool setConfirmCoadminInt(ApplicationUser user, string code, int ligId)
        {
            try
            {
                var coadmin = dbApp.tblLigaCoAdmnInit
                                   .Where(m => m.userEmail == user.Email && m.ligId == ligId)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    coadmin.lcaFechaConfirmacionUTC = DateTime.Now;
                    coadmin.lcaCodigoConfirmacion = null;
                    coadmin.lcaConfirmacion = true;
                    dbApp.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                e.ToString();
                return false;
            }
        }
        public bool setConfirmCoadmin(ApplicationUserManager userManager, ApplicationUser user, int ligId)
        {
            try
            {
                var coadmin_val = dbApp.tblLigaCoAdministradores
                                   .Where(m => m.tblUsuario.UserName == user.Email && m.ligId == ligId)
                                   .FirstOrDefault();

                if (coadmin_val == null)
                {
                    schemaLigaCoAdministradores coadmin = new schemaLigaCoAdministradores();
                    coadmin.lcaFechaConfirmacionUTC = DateTime.Now;
                    coadmin.lcaCodigoConfirmacion = null;
                    coadmin.lcaConfirmacion = true;
                    coadmin.lcaUserId = user.Id;
                    coadmin.ligId = ligId;

                    dbApp.tblLigaCoAdministradores.Add(coadmin);
                }
                else
                {
                    coadmin_val.lcaCodigoConfirmacion = null;
                    coadmin_val.lcaConfirmacion = true;
                }

                if (!userManager.IsInRole(user.Id, constClass.rolOwners))
                    userManager.AddToRole(user.Id, constClass.rolOwners);
                dbApp.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza la ubicacion de la liga
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="key"></param>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns></returns>
        public bool setUpdateLeagueLocation(int ligId, string key, string latitud, string longitud)
        {
            try
            {
                var liga = dbApp.tblLigas
                                .Where(m => m.ligId == ligId && m.ligUserIdCreador == key).FirstOrDefault();
                if (liga != null)
                {
                    liga.ligLatitud = latitud;
                    liga.ligLongitud = longitud;
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Valida que el id y el token para realizar alguna acción sea valido.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool getValidateLeagueByIdToken(int Id, string token)
        {
            var liga = getLigaById(Id);
            if(liga != null)
            {
                if (liga.ligUserIdCreador == token)
                    return true;
            }
            return false;
        }

        #region LigaPrincipal

        /// <summary>
        /// Obtiene la liga principal por usuario y rol actual
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<schemaLigaPrincipalUsuario> getMainLeagueBySearch(string userId, string roleId)
        {
            return (from lpu in dbApp.tblLigaPrincipalUsuario
                    where lpu.userId == userId
                        && lpu.roleId == roleId
                    select lpu)
                   .ToList();
        }

        /// <summary>
        /// Regresa la liga principal, si no tiene una establecida, le establece la primera como la principal.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public LeaguesActiveDetailViewModel getMainLeague(string userId, string roleId)
        {
            var liga = getMainLeagueBySearch(userId, roleId).FirstOrDefault();

            //Tiene una liga principal definida
            if (liga != null)
                return getUserLeaguesDetailById(liga.ligId, liga.tblLiga.ligUserIdCreador);
            
            //No tiene una liga principal definida.
            var ligasActivas = getUserLeagues(userId, roleId);
            if(ligasActivas.Count > 0)
            {
                var ligaPrincipal = ligasActivas.OrderBy(l => l.ligId).First();
                if(setMainLeague(ligaPrincipal.ligId, userId, roleId))
                    return getUserLeaguesDetailById(ligaPrincipal.ligId, ligaPrincipal.ligCreator);
            }

            return null;
        }

        public bool getMainLeague_IsMain(int ligId, string userId, string roleId)
        {
            var liga = getMainLeagueBySearch(userId, roleId);
            var mainLeague = liga.Where(l=> l.ligId == ligId).FirstOrDefault();

            if (mainLeague != null)
                return true;

            return false;
        }

        /// <summary>
        /// Establece una liga como la principal del usuario por rol.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool setMainLeague(int ligId, string userId, string roleId)
        {
            try
            {
                var liga = getMainLeagueBySearch(userId, roleId);
                if(liga.Count > 0)
                    dbApp.tblLigaPrincipalUsuario.RemoveRange(liga);

                var ligaPrincipal = new schemaLigaPrincipalUsuario();
                ligaPrincipal.ligId = ligId;
                ligaPrincipal.userId = userId;
                ligaPrincipal.roleId = roleId;
                dbApp.tblLigaPrincipalUsuario.Add(ligaPrincipal);

                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        public List<schemaLigas> getLigasActivasByUser(List<LeaguesActiveLOwnerViewModel> ligasUsuario)
        {
            return (from lus in ligasUsuario
                    join lig in dbApp.tblLigas on lus.ligId equals lig.ligId
                    select lig)
                    .ToList();
        }

        #endregion

        #region Ciudades

        /// <summary>
        /// Obtiene la lista de las distintas ciudades en donde hay un torneo.
        /// </summary>
        /// <returns></returns>
        public List<CiudadEstadoPaisViewModel> getLigaCiudades_DistinctActive()
        {
            var ligas = getLigas_BusquedaInicio("", 0, "");

            var direccion = (from lig in ligas
                             let ldc = dbApp.tblLigaDireccionComercial.Where(d => d.ligId == lig.ligId).OrderByDescending(d => d.ldcId).FirstOrDefault()
                             where ldc != null
                             select ldc)
                             .Distinct();
            var ciudades = direccion.Select(ldc => new CiudadEstadoPaisViewModel()
                                            {   
                                                direccionCompleta = ldc.ldcMunicipio + "," + ldc.ldcEstado,
                                                Municipio = ldc.ldcMunicipio,
                                                Estado = ldc.ldcEstado
                                            })
                                    .GroupBy(x => x.direccionCompleta)
                                    .Select(x => x.FirstOrDefault())
                                    .Distinct()
                                    .ToList();

            return ciudades;
        }

        #endregion

        #region BusquedaLigas

        public List<LeagueGridBusquedasInicio> getLigas_BusquedaInicio(string depNombre, int ttoId, string ldcMunicipio)
        {
            DateTime fechaActual = DateTime.Now.AddDays(-1);

            var torneos = (from tor in dbApp.tblTorneos
                         join lig in dbApp.tblLigas on tor.ligId equals lig.ligId into torLig
                         from lig in torLig.DefaultIfEmpty()
                         where lig.ligEstatus == true
                            && lig.ligAprobada == true
                            && tor.torEstatus == true
                            && tor.torAprobada == true
                            && tor.torPrivate == false
                            && (tor.torFechaTermino == null || tor.torFechaTermino > fechaActual)
                         select tor)
                         .Distinct()
                         .ToList();

            if(depNombre != "")
                torneos = (from tor in torneos
                           from lct in dbApp.tblLigaCategoriasTorneos.Where(c => c.lctId == tor.lctId)
                           where lct.depNombre.ToUpper() == depNombre.ToUpper()
                           select tor)
                          .Distinct()
                          .ToList();

            if (ttoId != 0)
                torneos = (from tor in torneos
                           from lct in dbApp.tblLigaCategoriasTorneos.Where(c => c.lctId == tor.lctId)
                           where lct.ttoId == ttoId
                           select tor)
                          .Distinct()
                          .ToList();

            if (ldcMunicipio != "")
                torneos = (from tor in torneos
                           let ldc = dbApp.tblLigaDireccionComercial.Where(d => d.ligId == tor.ligId).OrderByDescending(d => d.ldcId).FirstOrDefault()
                           let municipio = (ldc != null) ? ldc.ldcMunicipio.ToUpper() : ""
                           where ldc.ldcMunicipio.ToUpper() == ldcMunicipio.ToUpper()
                           select tor)
                          .Distinct()
                          .ToList();

            var ligas = torneos.Select(t => t.tblLiga).Distinct().ToList();

            return (from lig in ligas
                    let precio = dbApp.tblTorneos.Where(t => t.ligId == lig.ligId).OrderBy(t => t.torPrecioTorneo).FirstOrDefault()
                    let precioMenor = (precio != null) ? precio.torPrecioTorneo : 0
                    let comentarios = dbApp.tblTorneoComentarios.Where(t => t.tblTorneo.ligId == lig.ligId && t.tcoEstatus == true && t.tblTorneo.torEstatus == true).ToList()
                    let sumaCalificacionComentarios = (comentarios.Count > 0) ? comentarios.Sum(c => c.tcoCalificacion) : 0
                    let totalComentarios = comentarios.Count
                    let promedioLiga = (totalComentarios == 0) ? 5 : sumaCalificacionComentarios / totalComentarios
                    let totalJugadores = dbApp.tblJugadorEquipos.Where(j => j.tblTorneos.ligId == lig.ligId && j.tblTorneos.torEstatus == true && j.jugEstatus == true && j.jugConfirmado == true).Count()
                    //  let latitud = (lig.ligLatitud != null) ? double.Parse(lig.ligLatitud) : 0
                    // let longitud = (lig.ligLongitud != null) ? double.Parse(lig.ligLongitud) : 0
                    let latitud = (lig.ligLatitud != null && lig.ligLatitud != "") ? Convert.ToDouble(lig.ligLatitud) : 0.0
                    let longitud = (lig.ligLongitud != null && lig.ligLongitud != "") ? Convert.ToDouble(lig.ligLongitud) : 0.0
                    select new LeagueGridBusquedasInicio()
                    {
                        ligId = lig.ligId,
                        ligCreadorId = lig.ligUserIdCreador,
                        ligFechaCreacionUTC = lig.ligFechaRegistroUTC,
                        ligImg = lig.ligImgUrl,
                        ligNombre = lig.ligNombreLiga,
                        ligTipoLiga = lig.ligTipoLiga,
                        ligDescripcion = lig.ligDescripcion,
                        ligCalificacion = promedioLiga,
                        ligPrecioDesde = (precioMenor != null)? (decimal)precioMenor:0,
                        ligLatitud = latitud,
                        ligLongitud = longitud,
                        ligLatLngDistancia = 0,
                        ligTotalJugadores = totalJugadores
                    })
                    .ToList();
        }

        #endregion

        #endregion

        public List<ViewClients> getClients()
        {
            try
            {
                
                var userList = new List<ViewClients>();
                var users = dbApp.Users.ToList();

                foreach (var item in users)
                {
                    var roles = getUserRoles(item);
                    if (roles!=null || roles.Count>0)
                    {
                        foreach (var rol in roles)
                        {
                            var user_rol = new ViewClients();
                            user_rol.roles = rol.rolName;
                            user_rol.userId = item.Id;
                            user_rol.UserName = item.UserName;
                            user_rol.PhoneNumber = item.PhoneNumber;
                            user_rol.Email = item.Email;
                            user_rol.created_at = item.created_at;
                            userList.Add(user_rol);
                        }
                    }
                }

                return userList;
                        
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }

        #region Torneos

        #region Ciudades

        /// <summary>
        /// Obtiene la lista de las distintas ciudades en donde hay un torneo.
        /// </summary>
        /// <returns></returns>
        public List<CiudadEstadoPaisViewModel> getTorneoCiudades_DistinctActive()
        {
            var fechaActual = DateTime.Now.AddDays(-1);

            return (from ldc in dbApp.tblTorneoDireccion
                    where ldc.tblTorneo.tblLiga.ligEstatus == true
                       && ldc.tblTorneo.tblLiga.ligAprobada == true
                       && ldc.tblTorneo.torEstatus == true
                       && ldc.tblTorneo.torAprobada == true
                       && ldc.tblTorneo.torEsCoaching == false
                    select ldc)
                    .Where(l => l.tblTorneo.torFechaTermino == null || (DateTime)l.tblTorneo.torFechaTermino > fechaActual)
                    .Select(l => new CiudadEstadoPaisViewModel()
                    {
                        Municipio = l.ldcMunicipio,
                        Estado = l.ldcEstado
                    })
                    .Distinct()
                    .ToList();
        }

        #endregion

        #region Categorias

        /// <summary>
        /// Obtiene las categorias de las ligas que tiene el usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IEnumerable<TorneoCategoriasViewModel> getLigasCategoriasTorneos(string userId, string roleId)
        {
            try
            {
                var ligas = getUserLeagues(userId, roleId);
                if (ligas.Count() > 0)
                {
                    var categorias = (from lig in ligas
                                      join lct in dbApp.tblLigaCategoriasTorneos on lig.ligId equals lct.ligId 
                                      where lct.lctEstatus == true
                                      select new TorneoCategoriasViewModel()
                                      {
                                          lctId = lct.lctId,
                                          ligId = lct.ligId,
                                          lctNombre = lct.lctNombre,
                                          depNombre = lct.depNombre,
                                          ttoId = lct.ttoId,
                                          tblTipoTorneo = lct.tblTipoTorneo,
                                          lctDescripcion = lct.lctDescripcion,
                                          lctEdadMin = lct.lctEdadMin,
                                          lctEdadMax = lct.lctEdadMax
                                      })
                                       .ToList();
                    return categorias;
                }
            }
            catch(Exception ex)
            {
                ex.ToString();
            }

            return null;
        }

        /// <summary>
        /// Regresa el deporte a la que pertenece la categoria.
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        public schemaDeportes getDeportaByLigaCategoriasTorneosId(int lctId)
        {
            return (from lct in dbApp.tblLigaCategoriasTorneos
                    join dep in dbApp.tblDeportes on lct.depNombre equals dep.depNombre
                    where lct.lctId == lctId
                    select dep)
                   .FirstOrDefault();
        }

        /// <summary>
        /// Regresa la categoria por su id para los torneos de una liga
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        public schemaLigaCategoriasTorneos getLigaCategoriasTorneosById(int lctId)
        {
            return dbApp.tblLigaCategoriasTorneos
                        .Where(l => l.lctId == lctId)
                        .FirstOrDefault();
        }

        public List<schemaLigaCategoriasTorneos> getLigaCategoriasTorneosActivosByLigaId(int ligId)
        {
            return dbApp.tblLigaCategoriasTorneos
                        .Where(l => l.ligId == ligId
                                 && l.lctEstatus == true)
                        .ToList();
        }

        /// <summary>
        /// Da de alta una nueva categoria para los torneos de una liga
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLigaCategoriasTorneos_Add(TorneoCategoriasViewModel model)
        {
            try
            {
                var item = new schemaLigaCategoriasTorneos();
                item.ligId = model.ligId;
                item.lctNombre = model.lctNombre;
                item.depNombre = model.depNombre;
                item.ttoId = model.ttoId;
                item.lctDescripcion = model.lctDescripcion;
                item.lctEdadMin = model.lctEdadMin;
                item.lctEdadMax = model.lctEdadMax;

                dbApp.tblLigaCategoriasTorneos.Add(item);
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza la informacion de una categoria para los torneos de una liga
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLigaCategoriasTorneos_Edit(TorneoCategoriasViewModel model)
        {
            try
            {
                var item = getLigaCategoriasTorneosById(model.lctId);
                if (item != null)
                {
                    item.ligId = model.ligId;
                    item.lctNombre = model.lctNombre;
                    item.depNombre = model.depNombre;
                    item.ttoId = model.ttoId;
                    item.lctDescripcion = model.lctDescripcion;
                    item.lctEdadMin = model.lctEdadMin;
                    item.lctEdadMax = model.lctEdadMax;

                    dbApp.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Da de baja una cancha
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        public bool setLigaCategoriasTorneos_Delete(int lctId)
        {
            try
            {
                var item = getLigaCategoriasTorneosById(lctId);
                item.lctEstatus = false;
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        public bool setTipoTorneos_Add(TipoTorneoViewModel model)
        {
            try
            {
                var item = new schemaTipoTorneos();

                item.ttoNombre = model.ttoNombre;
                
                dbApp.tblTipoTorneo.Add(item);
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public bool setTipoTorneos_Edit(TipoTorneoViewModel model)
        {
            try
            {
                var item = getTiposTorneo_Active().Where(l => l.ttoId == model.ttoId).FirstOrDefault();
                if (item != null)
                {
                    item.ttoNombre = model.ttoNombre;

                    dbApp.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public bool setTipoTorneos_Delete(int ttoId)
        {
            try
            {
                var item = getTiposTorneo_Active().Where(l => l.ttoId == ttoId).FirstOrDefault();
                item.ttoEstatus = false;
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        #endregion

        #region Canchas
        public List<schemaLigaCanchasTorneos> getCanchasbyLigas(int liga)
        {
            return (from can in dbApp.tblLigaCanchasTorneos
                    where (can.ligId == liga)
                    select can)
                   .Distinct()
                   .ToList();

        }
        /// <summary>
        /// Obtiene las canchas de las ligas que tiene el usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IEnumerable<TorneoCanchasViewModel> getLigasCanchasTorneos(string userId, string roleId)
        {
            try
            {
                var ligas = new List<LeaguesActiveLOwnerViewModel>();
                var rol = getRoles().Where(l => l.rolId.ToUpper() == roleId.ToUpper()).First();

                if (rol.rolName.ToUpper() == constClass.rolAdminTorneos )
                {
                    var torneo = getTorneosByUser(userId);
                    if (torneo!=null)
                    {
                        foreach (var item in torneo)
                        {
                            var newLig = new LeaguesActiveLOwnerViewModel();
                            var lig = getLigaById(item.ligId);

                            var exist = ligas.Where(l=> l.ligId == lig.ligId);

                            if (exist == null || exist.Count() <= 0)
                            {
                                newLig.ligId = lig.ligId;
                                newLig.ligNombre = lig.ligNombreLiga;
                                ligas.Add(newLig);
                            }                            
                        }
                    }

                }else
                {
                    ligas = getUserLeagues(userId, roleId);
                }
                if (ligas.Count() > 0)
                {
                    var Canchas = (from lig in ligas
                                      join lct in dbApp.tblLigaCanchasTorneos on lig.ligId equals lct.ligId
                                      where lct.lcatEstatus == true
                                      select new TorneoCanchasViewModel()
                                      {
                                          ligaNombre = lig.ligNombre,
                                          lcatId = lct.lcatId,
                                          ligId = lig.ligId,
                                          lcatNombre = lct.lcatNombre,
                                          lcatDescripcion = lct.lcatDescripcion,
                                          lcatdomicilio= lct.lcatdomicilio,
                                          lcatNumExtInt =lct.lcatNumExtInt,
                                          lcatColonia=lct.lcatColonia,
                                          lcatMunicipio=lct.lcatMunicipio,
                                          lcatEstado=lct.lcatEstado,
                                          lcatCodigoPostal=lct.lcatCodigoPostal,
                                          lcatDomicilio = lct.lcatdomicilio + " " + lct.lcatNumExtInt + ", " +
                                                              "Col. " + lct.lcatColonia + " " + lct.lcatMunicipio + ", " +
                                                              lct.lcatEstado + ", C.P. " + lct.lcatCodigoPostal,
                                          lcatLatitud =lct.lcatLatitud,
                                          lcatLongitud=lct.lcatLongitud,
                                          fechaCreacion = lct.torFechaCreacionUTC
                                      })
                                       .ToList();
                    
                    return Canchas;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return null;
        }

        /// <summary>
        /// Regresa la cancha por su id.
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        public schemaLigaCanchasTorneos getLigaCanchasTorneosById(int lcatId)
        {
            return dbApp.tblLigaCanchasTorneos
                        .Where(l => l.lcatId == lcatId)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Da de alta una nueva cancha
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLigaCanchasTorneos_Add(TorneoCanchasViewModel model)
        {
            try
            {
                var item = new schemaLigaCanchasTorneos();
                if (model.edit)
                    item = getLigaCanchasTorneosById(model.lcatId);
                item.ligId = model.ligId;
                item.lcatNombre = model.lcatNombre;
                item.lcatDescripcion = model.lcatDescripcion;
                item.lcatdomicilio = model.lcatdomicilio;
                item.lcatNumExtInt = model.lcatNumExtInt;
                item.lcatColonia = model.lcatColonia;
                item.lcatMunicipio = model.lcatMunicipio;
                item.lcatEstado = model.lcatEstado;
                item.lcatCodigoPostal = model.lcatCodigoPostal;
                item.lcatLatitud = model.lcatLatitud;
                item.lcatLongitud = model.lcatLongitud;
                if (model.edit!=true)
                    dbApp.tblLigaCanchasTorneos.Add(item);
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza la informacion de una cancha
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setLigaCanchasTorneos_Edit(TorneoCanchasViewModel model)
        {
            try
            {
                var item = getLigaCanchasTorneosById(model.lcatId);
                if (item != null)
                {
                    item.ligId = model.ligId;
                    item.lcatNombre = model.lcatNombre;
                    item.lcatDescripcion = model.lcatDescripcion;

                    dbApp.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Da de baja una cancha
        /// </summary>
        /// <param name="lctId"></param>
        /// <returns></returns>
        public bool setLigaCanchasTorneos_Delete(int lctId)
        {
            try
            {
                var item = getLigaCanchasTorneosById(lctId);
                item.lcatEstatus = false;
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Regresa el tipo de estructuras del torneo.
        /// </summary>
        /// <param name="esDeporteEnEquipo"></param>
        /// <returns></returns>
        public List<schemaTorneoEstructura> getTorneoEstructurasByTipoDeporte(bool esDeporteEnEquipo)
        {
            return dbApp.tblTorneoEstructura
                        .Where(e => e.tcsDeporteEnEquipo == esDeporteEnEquipo
                                 && e.tcsEstatus == true)
                        .ToList();
        }

        #region Torneos

        /// <summary>
        /// Agrega un nuevo torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int setTorneo_Agregar(schemaTorneos model)
        {
            try
            {
                dbApp.tblTorneos.Add(model);
                dbApp.SaveChanges();
                return model.torId;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return 0;
        }
        public int setCoachTeam_Add(schemaEquipos equipo)
        {
            try
            {
                var team = new schemaEquipos();
                team.equNombreEquipo = equipo.equNombreEquipo;
                team.equPrecioTorneo = equipo.equPrecioTorneo;
                team.equUserIdCreador = equipo.equUserIdCreador;
                team.torId = equipo.torId;

                dbApp.tblEquipos.Add(team);
                dbApp.SaveChanges();
                return team.equId;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return 0;
        }
        
        /// <summary>
        /// Edita un torneo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setTorneo_Editar(schemaTorneos model)
        {
            try
            {
                var torneo = getTorneoById(model.torId);
                if (torneo != null)
                {
                    dbApp.tblTorneoTarifas.RemoveRange(torneo.tblTorneoTarifas);
                    dbApp.tblTorneoDireccion.Remove(torneo.tblTorneoDireccion);
                    //dbApp.SaveChanges();

                    torneo.torTipo = model.torTipo;
                    if(model.torImgUrl != null)
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
                    torneo.tblTorneoDireccion = model.tblTorneoDireccion;
                    torneo.torNumeroContacto = model.torNumeroContacto;
                    torneo.torCorreoContacto = model.torCorreoContacto;
                    torneo.torPrecioTorneo = model.torPrecioTorneo;
                    torneo.torDiasParaPago = model.torDiasParaPago;
                    torneo.tcfpptpId = model.tcfpptpId;
                    torneo.tblTorneoTarifas = model.tblTorneoTarifas;
                    torneo.torEsCoaching = model.torEsCoaching;
                    torneo.torDeporteEnEquipo = model.torDeporteEnEquipo;
                    torneo.torEstatus = model.torEstatus;
                    torneo.torPrivate = model.torPrivate;
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return false;
        }

        /// <summary>
        /// Regresa el torneo por su id.
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        public schemaTorneos getTorneoById(int torId)
        {
            return dbApp.tblTorneos
                        .FirstOrDefault(t => t.torId == torId);
        }
        /// <summary>
        /// Regresa el torneo por su nombre.
        /// </summary>
        /// <param name="torName"></param>
        /// <returns></returns>
        public schemaTorneos getTorneoByName(string torName)
        {
            return dbApp.tblTorneos
                        .FirstOrDefault(t => t.torNombreTorneo.ToUpper() == torName.ToUpper());
        }
        /// <summary>
        /// Regresa el torneo por su id y su token
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="usrId"></param>
        /// <returns></returns>
        public schemaTorneos getTorneoByIdToken(int torId, string usrId)
        {
            return dbApp.tblTorneos
                        .FirstOrDefault(t => t.torId == torId
                                          && t.torUserIdCreador.ToUpper() == usrId.ToUpper());
        }

        /// <summary>
        /// Regresa los torneos que pertenecen a la liga.
        /// </summary>
        /// <param name="ligId"></param>
        /// <returns></returns>
        public List<schemaTorneos> getTorneosByLiga(int ligId)
        {
            return (from tor in dbApp.tblTorneos
                    where (tor.ligId == ligId)
                    select tor)
                    .Distinct()
                    .ToList();
        }

        /// <summary>
        /// Regresa los torneos que pertenecen a la liga.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="userIdCreadorLiga"></param>
        /// <returns></returns>
        public List<TorneosGridViewModel> getTorneosParaGridByLiga(int? ligId, string userIdCreadorLiga , ApplicationUser user, bool admin=true)
        {
            //Falta afinar los lugares disponibles del torneo.
            var torneos = new List<schemaTorneos>();
            if (user!=null)
            {
               var roles = getRoles();
                switch (roles.Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper()).First().rolName)
                {
                    case constClass.rolPlayer:
                        torneos = getTorneoByPlayer(user.Id);
                        break;
                    case constClass.rolCoach:
                        var equipos = getEquipoByAdmin(user.Id);
                        torneos = (from equ in equipos
                                   join tor in dbApp.tblTorneos on equ.torId equals tor.torId
                                   where equ.equUserIdCreador == user.Id
                                   select tor).Distinct().ToList();
                        break;
                }
            }else
            {
                if (admin)
                {
                    torneos = (from tor in dbApp.tblTorneos
                               where (tor.ligId == (int)ligId)
                               select tor)
                               .Distinct()
                               .ToList();
                }
                else
                {
                    torneos = (from tor in dbApp.tblTorneos
                               where (tor.ligId == (int)ligId && tor.torPrivate == false)
                               select tor)
                               .Distinct()
                               .ToList();
                }
                
            }
            

            var torneoList = (from tor in torneos
                    let equiposInscritos = dbApp.tblEquipos.Where(e => e.torId == tor.torId ).Count()
                    let jugadoresInscritos = (tor.torEsCoaching == true) ? dbApp.tblJugadorEquipos.Where(e => e.torId == tor.torId).Count() : dbApp.tblJugadorEquipos.Where(e => e.torId == tor.torId && e.equId == 0)
                                              .Count()
                    let tieneEquipos = (tor.torTipo == constClass.torTipoCoaching || !tor.torDeporteEnEquipo) ? false : true
                    let comentarios = dbApp.tblTorneoComentarios.Where(t => t.torId == tor.torId && t.tcoEstatus == true).ToList()
                    let sumaCalificacionComentarios = (comentarios.Count > 0) ? comentarios.Sum(c => c.tcoCalificacion) : 0
                    let totalComentarios = comentarios.Count
                    let promedioTorneo = (totalComentarios == 0) ? 5 : sumaCalificacionComentarios / totalComentarios
                    select new TorneosGridViewModel()
                    {
                        torId = tor.torId,
                        ligNombre = tor.tblLiga.ligNombreLiga,
                        torImg = tor.torImgUrl,
                        torComentarios = tor.torComentarios,
                        torNombre = tor.torNombreTorneo,
                        tblCategoria = tor.tblCategoriaTorneo,
                        coaching = tor.torEsCoaching,
                        //torDescripcion = tor.torDescripcion,
                        torFechaInicio = tor.torFechaInicio,
                        torFechaFinal = tor.torFechaTermino,
                        torFechaLimiteInscripcion = tor.torFechaLimiteInscripcion,
                        torNumeroEquipos = tor.torNumeroEquipos,
                        torNumeroEquiposInscritos = equiposInscritos,
                        torNumeroJugadores = tor.torMaxJugadoresEquipo,
                        torNumeroJugadoresInscritos = jugadoresInscritos,
                        torPrecioTorneo = tor.torPrecioTorneo,
                        torCreador = tor.torUserIdCreador,
                        torEstatus = tor.torEstatus,
                        torEnEquipo = tieneEquipos,
                        torFechaCreacion = tor.torFechaCreacionUTC,
                        torCalificacion = promedioTorneo,
                        isPrivate = tor.torPrivate
                    }).ToList();
            return torneoList;
        }

        /// <summary>
        /// Regresa los torneos que administra o pertenecen al usuario.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<TorneosGridViewModel> getTorneosParaGridByUsuario(string userId)
        {
            var user = getUserById(userId);
            var rolActual = getRoles().Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper()).FirstOrDefault();
            var torneosAll = new List<schemaTorneos>();
            if (rolActual.rolName == constClass.rolOwners)
            {
                var ligas = getLigaByUser(userId).Select(s => s.ligId).ToList();
                //Falta afinar los lugares disponibles del torneo.
                torneosAll = dbApp.tblTorneos.Where(l => ligas.Contains(l.ligId)).ToList();
            }            
                        
            var torneos = torneosAll;

            var torCoAdm = getTorneosByUser(userId);

            if (torCoAdm.Any())
            {
                foreach (var torC in torCoAdm)
                {
                    var newTor = torneos.Where(l => l.torId == torC.torId);
                    if (!newTor.Any())
                    {
                        torneos.Add(torC);
                    }
                }
            }


            return (from tor in torneos
                    let equiposInscritos = dbApp.tblEquipos.Where(e => e.torId == tor.torId && e.equEstatus == true)
                                                           .Where(e => e.equPagado == true || e.equFechaVencimientoPagoUTC > DateTime.Now).Count()
                    let jugadoresInscritos = dbApp.tblJugadorEquipos.Where(e => e.torId == tor.torId && e.equId == 0 && e.jugEstatus == true)
                                                               .Where(e => e.jugPagado == true || e.jugFechaVencimientoPagoUTC > DateTime.Now).Count()
                    let tieneEquipos = (tor.torTipo == constClass.torTipoCoaching || !tor.torDeporteEnEquipo) ? false : true
                    let comentarios = dbApp.tblTorneoComentarios.Where(t => t.torId == tor.torId && t.tcoEstatus == true).ToList()
                    let sumaCalificacionComentarios = (comentarios.Count > 0) ? comentarios.Sum(c => c.tcoCalificacion) : 0
                    let totalComentarios = comentarios.Count
                    let promedioTorneo = (totalComentarios == 0) ? 5 : sumaCalificacionComentarios / totalComentarios
                    select new TorneosGridViewModel()
                    {
                        torId = tor.torId,
                        ligNombre = tor.tblLiga.ligNombreLiga,
                        torImg = tor.torImgUrl,
                        torComentarios = tor.torComentarios,
                        torNombre = tor.torNombreTorneo,
                        tblCategoria = tor.tblCategoriaTorneo,
                        coaching = tor.torEsCoaching,
                        //torDescripcion = tor.torDescripcion,
                        torFechaInicio = tor.torFechaInicio,
                        torFechaFinal = tor.torFechaTermino,
                        torFechaLimiteInscripcion = tor.torFechaLimiteInscripcion,
                        torNumeroEquipos = tor.torNumeroEquipos,
                        torNumeroEquiposInscritos = equiposInscritos,
                        torNumeroJugadores = tor.torMaxJugadoresEquipo,
                        torNumeroJugadoresInscritos = jugadoresInscritos,
                        torPrecioTorneo = tor.torPrecioTorneo,
                        torCreador = tor.torUserIdCreador,
                        torEstatus = tor.torEstatus,
                        torEnEquipo = tieneEquipos,
                        torFechaCreacion = tor.torFechaCreacionUTC,
                        torCalificacion = promedioTorneo
                    }).ToList();
        }

        public List<TorneosGridViewModel> getTorneosParaGridByArbitro(string userId)
        {
            var arbtitros = (from arb in dbApp.tblArbitros
                             join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                             where arb.arbUserId == userId
                             select arbLig).ToList();
            var user = getUserById(userId);
            var partidos = getPartidosRefeere(user).OrderBy(o=> o.ligId).ToList();

            var ligas = new List<schemaLigas>();
            foreach (var item in partidos)
            {
                var liga = getLigaById(item.ligId);


                if (ligas.Count > 0)
                {
                    if (liga.ligId != ligas.Last().ligId)
                    {
                        ligas.Add(liga);
                    }
                }
                else
                    ligas.Add(liga);

            }
            //Falta afinar los lugares disponibles del torneo.

            var torneos = (from lig in ligas
                    join tor in dbApp.tblTorneos on lig.ligId equals tor.ligId
                    let equiposInscritos = dbApp.tblEquipos.Where(e => e.tblTorneos.ligId == lig.ligId && e.equEstatus == true)
                                                           .Where(e => e.equPagado == true || e.equFechaVencimientoPagoUTC > DateTime.Now).Count()
                    let jugadoresInscritos = dbApp.tblJugadorEquipos.Where(e => e.torId == lig.ligId && e.equId == 0 && e.jugEstatus == true)
                                                               .Where(e => e.jugPagado == true || e.jugFechaVencimientoPagoUTC > DateTime.Now).Count()
                    let tieneEquipos = (tor.torTipo == constClass.torTipoCoaching || !tor.torDeporteEnEquipo) ? false : true
                    let comentarios = dbApp.tblTorneoComentarios.Where(t => t.torId == tor.torId && t.tcoEstatus == true).ToList()
                    let sumaCalificacionComentarios = (comentarios.Count > 0) ? comentarios.Sum(c => c.tcoCalificacion) : 0
                    let totalComentarios = comentarios.Count
                    let promedioTorneo = (totalComentarios == 0) ? 5 : sumaCalificacionComentarios / totalComentarios
                    select new TorneosGridViewModel()
                    {
                        torId = tor.torId,
                        ligNombre = lig.ligNombreLiga,
                        torImg = tor.torImgUrl,
                        torComentarios = tor.torComentarios,
                        torNombre = tor.torNombreTorneo,
                        tblCategoria = tor.tblCategoriaTorneo,
                        //torDescripcion = tor.torDescripcion,
                        torFechaInicio = tor.torFechaInicio,
                        torFechaFinal = tor.torFechaTermino,
                        torFechaLimiteInscripcion = tor.torFechaLimiteInscripcion,
                        torNumeroEquipos = tor.torNumeroEquipos,
                        torNumeroEquiposInscritos = equiposInscritos,
                        torNumeroJugadores = tor.torMaxJugadoresEquipo,
                        torNumeroJugadoresInscritos = jugadoresInscritos,
                        torPrecioTorneo = tor.torPrecioTorneo,
                        torCreador = tor.torUserIdCreador,
                        torEstatus = tor.torEstatus,
                        torEnEquipo = tieneEquipos,
                        torFechaCreacion = tor.torFechaCreacionUTC,
                        torCalificacion = promedioTorneo
                    }).ToList();
            var part = partidos.GroupBy(g => g.torId);
            var list = (from par in part
                        join tor in torneos on par.Key equals tor.torId
                        select tor).ToList();
            return list;
        }
        #endregion

        #region Coadministradores

        /// <summary>
        /// Regresa la lista de los coadministradores del torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="UserIdCreator"></param>
        /// <returns></returns>
        public List<TorneosCoAdministradoresViewModel> getTorneoCoadministradoresById(int torId)
        {
            var torneoCoadmin = (from tca in dbApp.tblTorneoCoAdministradores
                    let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.tblUsers.Email == tca.userCorreo).FirstOrDefault()
                    let nombreCompleto = (prof != null) ? (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim() : ""
                    where tca.torId == torId
                    select new TorneosCoAdministradoresViewModel()
                    {
                        tcaNombre = (nombreCompleto==null || nombreCompleto=="") ? tca.userCorreo : nombreCompleto,
                        tcaEmail = tca.userCorreo,
                        tcaConfirmado = tca.tcaConfirmacion,
                        tcoId = tca.tcoId
                    })
                    .OrderBy(l => l.tcaNombre)
                    .ToList();
            return torneoCoadmin;
        }
        public schemaTorneoCoAdministradores getTorneoCoAdminByTcoId(int tcoId)
        {
            return dbApp.tblTorneoCoAdministradores.Where(l => l.tcoId == tcoId).FirstOrDefault();
        }
        /// <summary>
        /// Guarda al coadministrador del torneo en la tabla
        /// </summary>
        /// <param name="torneoCoAdmin"></param>
        /// <returns></returns>
        public bool setTorneoCoadmin(schemaTorneoCoAdministradores torneoCoAdmin)
        {
            try
            {
                dbApp.tblTorneoCoAdministradores.Add(torneoCoAdmin);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza el codigo que se va a enviar en el correo de confirmacion para coadministrador
        /// </summary>
        /// <param name="model"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setTorneoCoadminConfirmCode_Update(schemaTorneoCoAdministradores model, string code)
        {
            try
            {
                var coadmin = dbApp.tblTorneoCoAdministradores.Where(m => m.torId == model.torId && m.userCorreo == model.userCorreo).FirstOrDefault();
                coadmin.tcaCodigoConfirmacion = code;
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador del torneo validando la llave de seguridad (UserIdCreator de la liga)
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="key"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool setTorneoCoadmin_Delete_tcoId(int tcoId,string emailCoadmin)
        {
            try
            {
                var coadmin = dbApp.tblTorneoCoAdministradores
                                   .Where(m => m.tcoId == tcoId && m.userCorreo == emailCoadmin).FirstOrDefault();
                if (coadmin != null)
                {
                    dbApp.tblTorneoCoAdministradores.Remove(coadmin);
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador del torneo cuando rechaza desde el correo.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool setTorneoCoadmin_Delete(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblTorneoCoAdministradores
                                   .Where(m => m.userCorreo == userEmail && m.tcaCodigoConfirmacion == code)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    dbApp.tblTorneoCoAdministradores.Remove(coadmin);
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa el torneo en donde esta registrado el email y codigo que esta confirmando
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public schemaTorneos getTorneoByCoAdminCodeEmail(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblTorneoCoAdministradores
                                   .Where(m => m.userCorreo == userEmail && m.tcaCodigoConfirmacion == code).FirstOrDefault();
                if (coadmin != null)
                {
                    return coadmin.tblTorneo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Confirma la coadministración del torneo.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setTorneoConfirmCoadmin(ApplicationUserManager userManager, ApplicationUser user, string code)
        {
            try
            {
                var coadmin = dbApp.tblTorneoCoAdministradores
                                   .Where(m => m.userCorreo == user.Email && m.tcaCodigoConfirmacion == code)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    coadmin.tcaFechaConfirmacionUTC = DateTime.Now;
                    coadmin.tcaCodigoConfirmacion = null;
                    coadmin.tcaConfirmacion = true;
                    dbApp.SaveChanges();

                    if (!userManager.IsInRole(user.Id, constClass.rolAdminTorneos))
                        userManager.AddToRole(user.Id, constClass.rolAdminTorneos);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        #endregion

        #region Comentarios

        /// <summary>
        /// Regresa los comentarios de los torneos de la liga.
        /// </summary>
        /// <param name="ligId"></param>
        /// <param name="ligFechaRegistroUTC">Token. Es la fecha de registro de la liga en formato ddMMyyyyHHmmssfff</param>
        /// <returns></returns>
        public List<TorneosComentariosGridViewModel> getTorneoComentariosLiga(int ligId, string ligFechaRegistroUTC)
        {
            DateTime ligFechaRegistro = (DateTime)Global_Functions.stringFormatToDate(ligFechaRegistroUTC);

            if (ligFechaRegistroUTC != null)
            {
                var comentarios = (from tco in dbApp.tblTorneoComentarios
                                   let fechaCreacionLiga = tco.tblTorneo.tblLiga.ligFechaRegistroUTC
                                   let equNombre = (tco.tblEquipo != null) ? tco.tblEquipo.equNombreEquipo : ""
                                   let usuPerfil = dbApp.tblUsersProfiles.Where(p => p.userIdOwner == tco.tcoUserIdComenta && p.uprPerfilPrincipal == true).FirstOrDefault()
                                   let usuNombre = (usuPerfil != null) ? usuPerfil.uprNombres : ""
                                   where tco.tblTorneo.ligId == ligId
                                      && fechaCreacionLiga == ligFechaRegistro
                                      && tco.tcoEstatus == true
                                   select new TorneosComentariosGridViewModel()
                                   {
                                       //tcgvId = index++,
                                       tcoId = tco.tcoId,
                                       torId = tco.torId,
                                       torNombre = tco.tblTorneo.torNombreTorneo,
                                       equNombre = equNombre,
                                       usuNombre = usuNombre,
                                       tcoComentario = tco.tcoComentario,
                                       tcoCalificacion = tco.tcoCalificacion,
                                       tcoFechaComentarioUTC = tco.tcoFechaComentarioUTC,
                                       tcoEstatus = tco.tcoEstatus
                                   })
                                    .ToList();

                comentarios = comentarios.Select((c, index) => new TorneosComentariosGridViewModel()
                                                {
                                                    tcgvId = index,
                                                    tcoId = c.tcoId,
                                                    torId = c.torId,
                                                    torNombre = c.torNombre,
                                                    equNombre = c.equNombre,
                                                    usuNombre = c.usuNombre,
                                                    tcoComentario = c.tcoComentario,
                                                    tcoCalificacion = c.tcoCalificacion,
                                                    tcoFechaComentarioUTC = c.tcoFechaComentarioUTC,
                                                    tcoEstatus = c.tcoEstatus
                                                })
                                        .ToList();

                return comentarios;
            }
            return null;
        }

        /// <summary>
        /// Regresa los comentarios del torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="torFechaCreacionUTC">Token. Es la fecha de registro de la liga en formato ddMMyyyyHHmmssfff</param>
        /// <returns></returns>
        public List<TorneosComentariosGridViewModel> getTorneoComentariosTorneo(int torId, string torFechaCreacionUTC)
        {
            DateTime torFechaRegistro = (DateTime)Global_Functions.stringFormatToDate(torFechaCreacionUTC);

            if (torFechaRegistro != null)
            {
                var comentarios = (from tco in dbApp.tblTorneoComentarios
                                   let fechaCreacion = tco.tblTorneo.torFechaCreacionUTC
                                   let equNombre = (tco.tblEquipo != null) ? tco.tblEquipo.equNombreEquipo : ""
                                   let usuPerfil = dbApp.tblUsersProfiles.Where(p => p.userIdOwner == tco.tcoUserIdComenta && p.uprPerfilPrincipal == true).FirstOrDefault()
                                   let usuNombre = (usuPerfil != null) ? usuPerfil.uprNombres : ""
                                   where tco.torId == torId
                                      && fechaCreacion == torFechaRegistro
                                      && tco.tcoEstatus == true
                                   select new TorneosComentariosGridViewModel()
                                   {
                                       //tcgvId = index++,
                                       tcoId = tco.tcoId,
                                       torId = tco.torId,
                                       torNombre = tco.tblTorneo.torNombreTorneo,
                                       equNombre = equNombre,
                                       usuNombre = usuNombre,
                                       tcoComentario = tco.tcoComentario,
                                       tcoCalificacion = tco.tcoCalificacion,
                                       tcoFechaComentarioUTC = tco.tcoFechaComentarioUTC,
                                       tcoEstatus = tco.tcoEstatus
                                   })
                                    .ToList();

                comentarios = comentarios.Select((c, index) => new TorneosComentariosGridViewModel()
                                                {
                                                    tcgvId = index,
                                                    tcoId = c.tcoId,
                                                    torId = c.torId,
                                                    torNombre = c.torNombre,
                                                    equNombre = c.equNombre,
                                                    usuNombre = c.usuNombre,
                                                    tcoComentario = c.tcoComentario,
                                                    tcoCalificacion = c.tcoCalificacion,
                                                    tcoFechaComentarioUTC = c.tcoFechaComentarioUTC,
                                                    tcoEstatus = c.tcoEstatus
                                                })
                                        .ToList();

                return comentarios;
            }
            return null;
        }

        #endregion

        #endregion

        #region Equipos

        /// <summary>
        /// Regresa el máximo de equipos y jugadores de un torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        public EquiposJugadoresTotalTorneos getEquiposJugadoresTotalByTorneo(int torId)
        {
            var totales = new EquiposJugadoresTotalTorneos();
            //var equipos = dbApp.tblEquipos.Where(e => e.torId == torId && e.equEstatus == true)
            //.Where(e => e.equPagado == true || e.equFechaVencimientoPagoUTC > DateTime.Now).ToList();
            var equipos = dbApp.tblEquipos.Where(e => e.torId == torId && e.equDelete == false ).Select(s => s.equId).ToList();

            totales.totalEquipos = equipos.Count;
            //var jugadores = dbApp.tblJugadorEquipos.Where(e => e.torId == torId && e.equId == 0 && e.jugEstatus == true).ToList();
            //                                   .Where(e => e.jugPagado == true || e.jugFechaVencimientoPagoUTC > DateTime.Now).ToList();
            var jugadores = dbApp.tblJugadorEquipos.Where(e => e.torId == torId && e.equId == 0).ToList();
            //Falta afinar los lugares disponibles del torneo.
            jugadores = jugadores.Where(l => equipos.Contains(l.equIdRef)).ToList();

            totales.totalJugadores = jugadores.Count;

            return totales;
        }
        public List<schemaEquipos> getEquipoByTorneo(int torId)
        {
            var equipos = dbApp.tblEquipos.Where(e => e.torId == torId).ToList();
            
            return equipos;
        }
        public Boolean setDeleteTeam(int equId)
        {
            try
            {
                var equipo = dbApp.tblEquipos.Where(l => l.equId == equId).FirstOrDefault();
                if (equipo != null)
                {
                    equipo.equDelete = true;

                    dbApp.SaveChanges();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                throw;
            }         
            
            return true;
        }
        public List<JugadorEquiposModel> getJugadoresByTorneo_EquipoId(int torId, int equId)
        {
            var jugadores = (from jugE in dbApp.tblJugadorEquipos
                         where jugE.torId == torId
                         && jugE.equId == equId
                         && jugE.jugEstatus == true
                        select new JugadorEquiposModel{
                            jugUserId = jugE.jugUserId,
                            jugCorreo = jugE.jugCorrreo,
                            jugNombre = jugE.jugCorrreo,
                            jugConfirmado = jugE.jugConfirmado,
                            jugCodigoConfirmacion = jugE.jugCodigoConfirmacion,
                            jugEstatus = jugE.jugEstatus
                        }).ToList();

            //var jugadores = dbApp.tblJugadores.Where(e => e.torId == torId && e.equId == equId ).ToList();

            return jugadores;
        }
        public List<JugadorEquiposModel> getJugadoresByTorneo_UserEmail(int torId, string userEmail)
        {
            var jugadores = (from jugE in dbApp.tblJugadorEquipos
                             join equipos in dbApp.tblEquipos on jugE.equId equals equipos.equId
                             where jugE.torId == torId
                               && jugE.jugCorrreo == userEmail
                               && equipos.equDelete == false
                             select new JugadorEquiposModel
                             {
                                 jugUserId = jugE.jugUserId,
                                 jugCorreo = jugE.jugCorrreo,
                                 jugNombre = jugE.jugCorrreo,
                                 jugConfirmado = jugE.jugConfirmado,
                                 jugCodigoConfirmacion = jugE.jugCodigoConfirmacion,
                                 jugEstatus = jugE.jugEstatus
                             }).ToList();

            //var jugadores = dbApp.tblJugadores.Where(e => e.torId == torId && e.equId == equId ).ToList();

            return jugadores;
        }
        public schemaLigas getLigaByJugador(string userId)
        {
            var liga = (from lig in dbApp.tblLigas
                        join jugE in dbApp.tblJugadorEquipos on lig.ligId equals jugE.tblTorneos.ligId
                        where jugE.jugUserId == userId
                        select lig).FirstOrDefault();
            return liga;
        }
        /// <summary>
        /// Regresa el equipo especificado
        /// </summary>
        /// <param name="equId"></param>
        /// <returns></returns>
        public schemaEquipos getEquipoById(int equId)
        {
            return dbApp.tblEquipos
                        .Where(e => e.equId == equId)
                        .FirstOrDefault();
        }

        #region Coadministradores

        /// <summary>
        /// Regresa la lista de los coadministradores del equipo.
        /// </summary>
        /// <param name="equId"></param>
        /// <param name="equUserIdCreador"></param>
        /// <returns></returns>
        public List<schemaEquipos> getEquipoByAdmin(string userId)
        {
            var user = dbApp.Users.Where(l => l.Id == userId).FirstOrDefault();
            var model = (from equ in dbApp.tblEquipos
                         join equCo in dbApp.tblEquiposCoAdministradores on equ.equId equals equCo.equId into equ_CoAdm
                         from equCo in equ_CoAdm.DefaultIfEmpty()
                         where equ.equUserIdCreador == userId
                         || equCo.equUserId == userId || equ.equAdminCorreo == user.Email && equ.equEstatus == true && equ.equDelete==false
                         select equ).ToList();
            return model;

        }
        public List<schemaEquipos> getEquipoByPlayer(string userId)
        {
            var model = (from equ in dbApp.tblEquipos
                         join jug in dbApp.tblJugadorEquipos on equ.equId equals jug.equId
                         where jug.jugUserId == userId && jug.jugConfirmado == true
                         select equ).ToList();
            return model;
        }
        public List<schemaEquipos> getEquipoCoachingByTorneo(int torId,string user_id)
        {
            var model = (from equ in dbApp.tblEquipos
                         join tor in dbApp.tblTorneos on equ.torId equals tor.torId
                         where equ.equUserIdCreador== user_id && tor.torId == torId && equ.equDelete == false
                         select equ).ToList();
            return model;
        }
        public List<EquiposCoAdministradoresViewModel> getEquipoCoadministradoresById(int equId, string equUserIdCreador)
        {
            var coAdmins =  (from eca in dbApp.tblEquiposCoAdministradores
                            let prof = (eca.tblUsuario != null) ? dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true 
                                                                                                 && p.userIdOwner == eca.equUserId).FirstOrDefault() 
                                                                : null
                            let nombreCompleto = (prof != null) ? (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim() : ""
                            where eca.equId == equId
                               && eca.tblEquipo.equUserIdCreador == equUserIdCreador
                            select new EquiposCoAdministradoresViewModel()
                            {
                                ecaEmail = eca.ecaCorreoId,
                                ecaNombre = nombreCompleto,
                                ecaConfirmado = eca.equConfirmado,
                                ecaEstatus = eca.equEstatus
                            })
                            .OrderBy(l => l.ecaNombre)
                            .ToList();

            return coAdmins;
        }

        public bool setAdminEquipo_ConfirmarParticipacion(ApplicationUser user, int torId, int equId)
        {
            try
            {
                var admin = dbApp.tblEquipos.Where(l => l.equId == equId && l.torId == torId);
                if (admin!=null)
                {
                    var equipo = admin.FirstOrDefault();
                    equipo.equEstatus = true;
                    if (equipo.equUserIdCreador == null)
                    {
                        equipo.equUserIdCreador = user.Id;
                    }
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
                throw;
            }
            return false;
        }

        /// <summary>
        /// Guarda al coadministrador del equipo en la tabla
        /// </summary>
        /// <param name="equipoCoAdmin"></param>
        /// <returns></returns>
        public bool setEquipoCoadmin(schemaEquiposCoAdministradores equipoCoAdmin)
        {
            try
            {
                dbApp.tblEquiposCoAdministradores.Add(equipoCoAdmin);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Actualiza el codigo que se va a enviar en el correo de confirmacion para coadministrador
        /// </summary>
        /// <param name="model"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setEquipoCoadminConfirmCode_Update(schemaEquiposCoAdministradores model, string code)
        {
            try
            {
                var coadmin = dbApp.tblEquiposCoAdministradores.Where(m => m.equId == model.equId && m.ecaCorreoId == model.ecaCorreoId).FirstOrDefault();
                coadmin.equCodigoConfirmacion = code;
                coadmin.equConfirmado = false;
                coadmin.equEstatus = true;

                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador del equipo validando la llave de seguridad (UserIdCreator de la liga)
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="key"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool setEquipoCoadmin_Delete(int equId, string equUserIdCreador, string ecaCorreoId)
        {
            try
            {
                var coadmin = dbApp.tblEquiposCoAdministradores
                                   .Where(m => m.equId == equId && m.tblEquipo.equUserIdCreador == equUserIdCreador && m.ecaCorreoId == ecaCorreoId).FirstOrDefault();
                if (coadmin != null)
                {
                    coadmin.equConfirmado = false;
                    coadmin.equCodigoConfirmacion = null;
                    coadmin.equFechaConfirmacionUTC = null;
                    coadmin.equEstatus = false;
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Eliminar a un coadministrador del equipo cuando rechaza desde el correo.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setEquipoCoadmin_Delete(string ecaCorreoId, string code)
        {
            try
            {
                var coadmin = dbApp.tblEquiposCoAdministradores
                                   .Where(m => m.ecaCorreoId == ecaCorreoId && m.equCodigoConfirmacion == code)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    coadmin.equConfirmado = false;
                    coadmin.equCodigoConfirmacion = null;
                    coadmin.equFechaConfirmacionUTC = null;
                    coadmin.equEstatus = false;
                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Regresa el equipo en donde esta registrado el email y codigo que esta confirmando
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public schemaEquipos getEquipoByCoAdminCodeEmail(string userEmail, string code)
        {
            try
            {
                var coadmin = dbApp.tblEquiposCoAdministradores
                                   .Where(m => m.ecaCorreoId == userEmail && m.equCodigoConfirmacion == code && m.equEstatus == true).FirstOrDefault();
                if (coadmin != null)
                {
                    return coadmin.tblEquipo;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Confirma la coadministración del equipo.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool setEquipoConfirmCoadmin(ApplicationUserManager userManager, ApplicationUser user, string code)
        {
            try
            {
                var coadmin = dbApp.tblEquiposCoAdministradores
                                   .Where(m => m.ecaCorreoId == user.UserName
                                            && m.equCodigoConfirmacion == code)
                                   .FirstOrDefault();
                if (coadmin != null)
                {
                    coadmin.equFechaConfirmacionUTC = DateTime.Now;
                    coadmin.equCodigoConfirmacion = null;
                    coadmin.equConfirmado = true;
                    coadmin.equUserId = user.Id;

                    if (!userManager.IsInRole(user.Id, constClass.rolCoach))
                        userManager.AddToRole(user.Id, constClass.rolCoach);

                    dbApp.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        #endregion

        #endregion

        #region Jugadores

        /// <summary>
        /// Busca en la base de datos si existen los jugadores registrados con el correo 
        /// y regresa la info con el id del usuario.
        /// </summary>
        /// <param name="jugList"></param>
        /// <returns></returns>
        /// 

        public List<schemaTorneos> getTorneoByPlayer(string userId)
        {
            return (from tor in dbApp.tblTorneos
             join jugE in dbApp.tblJugadorEquipos on tor.torId equals jugE.torId
             where jugE.jugUserId == userId
             select tor).Distinct().ToList();

        }
        public List<JugadoresViewModel> getJugadoreIdByListEquipo(List<JugadoresViewModel> jugList)
        {
            return (from jli in jugList
                    join usr in dbApp.Users on jli.jugCorreo.ToUpper() equals usr.UserName.ToUpper() into jli_usr
                    from usr in jli_usr.DefaultIfEmpty()
                    let val = (jli_usr != null) ? jli_usr.FirstOrDefault() : null
                    select new JugadoresViewModel()
                    {
                        jugUserId = (val != null) ? val.Id : null,
                        jugCorreo = jli.jugCorreo,
                        jugNombre = jli.jugNombre,
                        jugConfirmado = jli.jugConfirmado,
                        jugCodConfirmacion = jli.jugCodConfirmacion,
                        jugNuevo = jli.jugNuevo,
                        jugEstatus = jli.jugEstatus
                    }).ToList();
        }

        /// <summary>
        /// Devuelve la lista de los jugadores con la informacion de perfil del contacto.
        /// </summary>
        /// <param name="jugList"></param>
        /// <returns></returns>
        public List<JugadorPerfilViewModel> getJugadoresByListEquipo(List<JugadoresViewModel> jugList)
        {
            return (from jli in jugList
                    let val = dbApp.tblUsersProfiles.Where(up => up.userIdOwner == jli.jugUserId && up.uprPerfilPrincipal == true).FirstOrDefault()
                    select new JugadorPerfilViewModel()
                    {
                        jugImg = (val != null) ? val.uprProfileImageURL : null,
                        jugNombre = (val != null) ? (val.uprNombres + " " + val.uprApellidos).Replace("-","").Trim() : jli.jugNombre,
                        jugGenero = (val != null) ? val.uprGenero : null,
                        jugFechaNacimiento = (val != null) ? val.uprFechaNacimiento : null,
                        jugCiudad = (val != null) ? val.uprCiudad : null,
                        jugTelefono = (val != null) ? val.uprTelefono : null,
                        jugCorreo = jli.jugCorreo,
                        jugConfirmado = jli.jugConfirmado,
                        jugNuevo = jli.jugNuevo,
                        jugEstatus = jli.jugEstatus,
                        jugCodConfirmacion = jli.jugCodConfirmacion
                    }).ToList();
        }

        /// <summary>
        /// Devuelve la lista de los jugadores que pertenecen a un torneo y/o equipo.
        /// </summary>
        /// <param name="jugList"></param>
        /// <returns></returns>
        public List<JugadoresViewModel> getJugadoresListaByTorneo_EquipoId(int torId, int equId)
        {
            var juga = (from jugE in dbApp.tblJugadorEquipos 
                        join equipos in dbApp.tblEquipos on jugE.equId equals equipos.equId
                        let nuevo = (jugE.jugCodigoConfirmacion != null) ? true : false
                        where jugE.torId == torId
                               && jugE.equId == equId
                               && jugE.jugEstatus == true
                               && equipos.equDelete == false
                        select new JugadoresViewModel()
                        {
                            jugCorreo = jugE.jugCorrreo,
                            jugNombre = jugE.jugCorrreo,
                            jugUserId = jugE.jugUserId,
                            jugConfirmado = jugE.jugConfirmado,
                            jugNuevo = nuevo,
                            jugCodConfirmacion = nuevo,
                            jugEstatus = jugE.jugEstatus
                        }).ToList();
            return juga;
           // return getJugadoreIdByListEquipo(juga);
        }

        /// <summary>
        /// Devuelve la lista de los jugadores que pertenecen a un torneo y/o a un equipo.
        /// </summary>
        /// <param name="jugList"></param>
        /// <returns></returns>
        public List<schemaJugadorEquipos> getJugadoresByTorneo_Equipo(int torId, int equId)
        {
            return dbApp.tblJugadorEquipos.Where(l => l.equId == equId && l.torId == torId).ToList();
        }

        public schemaJugadorEquipos getJugadorByUserId(string jugUserId, string email)
        {
            var jugador = dbApp.tblJugadorEquipos.Where(l => l.jugUserId == jugUserId || l.jugCorrreo == email).FirstOrDefault();
            return jugador;
        }
        public List<schemaLigas> getLigasByJugador(string userId)
        {
            var ligas = (from lig in dbApp.tblLigas
                         join jugE in dbApp.tblJugadorEquipos on lig.ligId equals jugE.tblTorneos.ligId
                         where jugE.jugUserId == userId
                         select lig).Distinct().ToList();
            return ligas;
        }

        /// <summary>
        /// Devuelve al jugador especifico de un torneo y/o equipo.
        /// </summary>
        /// <param name="jugCorreo"></param>
        /// <param name="torId"></param>
        /// <param name="equId"></param>
        /// <returns></returns>
        public schemaJugadorEquipos getJugadorByIds(string jugCorreo, int torId, int equId)
        {
            var jug = dbApp.tblJugadorEquipos.Where(l => l.jugCorrreo == jugCorreo && l.torId == torId && l.equId == equId).FirstOrDefault();
            return jug;
        }

        public schemaJugadorEquipos getJugadorEquipoByIds(string jugCorreo, int torId, int equId)
        {
            return dbApp.tblJugadorEquipos.Where(l => l.jugCorrreo == jugCorreo && l.torId == torId && l.equId == equId ).First();
            

           /* return (from jug in dbApp.tblJugadores
                    join jugE in dbApp.tblJugadorEquipos on jug.jugId equals jugE.jugId
                    where jug.jugCorreo == jugCorreo
                       && jugE.torId == torId
                       && jugE.equId == equId
                       && jugE.jugId == jugador.jugId
                    select new JugadorEquiposModel()
                    {
                        equId = equId,
                        torId = torId,
                        tblTorneos = jugE.tblTorneos,
                        tblEquipos = jugE.tblEquipos,
                        jugCorreo = jug.jugCorreo,
                        jugNombre = jug.jugNombre,
                        jugUserId = jug.jugUserId,
                        jugCodigoConfirmacion = jugE.jugCodigoConfirmacion,
                        jugConfirmado = jugE.jugConfirmado,
                        jugEstatus = jugE.jugEstatus
                    }).FirstOrDefault();*/
        }

        /// <summary>
        /// Metodo que sirve para agregar un nuevo equipo 
        /// <param name="model"></param>
        /// <returns></returns>
        public int setJugadoresEquipo_AgregarEditar(EquiposJugadoresViewModel model, string codigoConfirmacion, string userId, bool? inscTorneo=true)
        {
            try
            {
                DateTime fechaHoyUTC = DateTime.Now;
                int dias = (model.tblTorneo.torDiasParaPago == null) ? 1 : (int)model.tblTorneo.torDiasParaPago;
                DateTime equiFechaVencimientoPagoUTC = DateTime.Now.AddDays(dias);
                
                schemaEquipos modelEquipo = null;
                if (model.mostrarDatosEquipo)
                {
                    try
                    {
                        modelEquipo = new schemaEquipos();
                        if (model.equId == 0)
                        {                            
                            modelEquipo.equFechaCreacionUTC = fechaHoyUTC;
                            modelEquipo.equFechaVencimientoPagoUTC = equiFechaVencimientoPagoUTC;
                        }
                        else
                            modelEquipo = getEquipoById(model.equId);
                        modelEquipo.torId = model.torId;
                        modelEquipo.equAdminCorreo = model.equCorreoAdministrador;
                        modelEquipo.equUserIdCreador = model.equCreadorEquipoId;
                        if (model.equImg != null)
                            modelEquipo.equImgUrl = model.equImg;
                        modelEquipo.equNombreEquipo = model.equNombre;
                        
                        if (model.equCreadorEquipoId != null)
                        {
                            if (model.equCreadorEquipoId.ToUpper() != userId.ToUpper())
                                modelEquipo.equEstatus = false;
                            else
                                modelEquipo.equEstatus = true;
                        }
                        else
                            modelEquipo.equAdminCorreo = model.equCorreoAdministrador;

                        if ((bool)inscTorneo)
                        {
                            modelEquipo.equEstatus = true;
                        }
                        else
                        {
                            modelEquipo.equEstatus = false;
                        }


                        modelEquipo.equPrecioTorneo = model.torPrecio;
                        //Si el equipo no existe lo crea para poder agregarle los jugadores.
                        //Si el equipo si existe va a guardar todo con el savechanges del final del metodo.
                        if (model.equId == 0)
                        {
                            dbApp.tblEquipos.Add(modelEquipo);
                            dbApp.SaveChanges();
                            model.equId = modelEquipo.equId;
                            
                        }
                        else
                        {
                            var edit_team = dbApp.tblEquipos.Where(l => l.equId == model.equId).FirstOrDefault();
                            edit_team = modelEquipo;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        Global_Functions.saveErrors(ex.ToString(), false);
                        throw;
                    }
                    
                }

                if (model.mostrarDatosEquipo)
                    model.torPrecio = null;
                var timeUTC = DateTime.Now;
                //Obtiene los jugadores que ya estan en la base de datos del torneo y/o equipo
                var jugadoresBD = getJugadoresByTorneo_Equipo(model.torId, model.equId);

                //Le cambia el estatus a todos aquellos que van a ser dados de baja
               /* (from jugBD in jugadoresBD
                where jugBD.jugEstatus == true
                   && (from jugEL in model.listJugadores 
                       where jugEL.jugEstatus == false
                       select jugEL.jugCorreo).Contains(jugBD.jugCorreo)
                 select jugBD).ToList().ForEach(j => { j.jugEstatus = false; j.jugCodigoConfirmacion = null; });
                 */
                //Va a cambiar el estatus a todos esos jugadores que habian sido dados de baja pero seran nuevamente integrados en el equipo.
              /*  var jugReintegrar = (from jugBD in jugadoresBD
                                     where (jugBD.jugPagado == false && jugBD.jugFechaVencimientoPagoUTC < DateTime.Now)
                                        || jugBD.jugEstatus == false
                                        && (from jugEL in model.listJugadores
                                            where jugEL.jugEstatus == true
                                            select jugEL.jugCorreo).Contains(jugBD.jugCorreo)
                                     select jugBD).ToList();*/
                /*var jugReintegrarJoin = jugReintegrar.Join(model.listJugadores, l1 => l1.jugCorreo, l2 => l2.jugCorreo, (l1, l2) =>
                                                          {
                                                              l1.jugUserId = l2.jugUserId;
                                                              l1.jugNombre = l2.jugNombre;
                                                              l1.jugConfirmado = false;
                                                              l1.jugCodigoConfirmacion = codigoConfirmacion;
                                                              l1.jugEstatus = true;
                                                              l1.jugFechaVencimientoPagoUTC = equiFechaVencimientoPagoUTC;
                                                              l1.jugPrecioTorneo = model.torPrecio;
                                                              return l1;
                                                          })
                                                     .ToList();
                                                     */
                //Va a dar de alta a todos aquellos jugadores nuevos que no estan en la base de datos.
                int? equId = null;
                if (model.equId > 0)
                    equId = model.equId;

                var addJug = (from jugNu in model.listJugadores
                           where jugNu.jugEstatus == true
                              && !(from jugBD in jugadoresBD
                                       where jugBD.jugEstatus == true
                                   select jugBD.jugCorrreo).Contains(jugNu.jugCorreo)
                           select jugNu).ToList();


                var deleteJug = (from jugNu in model.listJugadores
                              where jugNu.jugEstatus == false                               
                              select jugNu).ToList();

                model.listJugadores.RemoveAll(r => r.jugEstatus == false);

                if ( addJug != null && addJug.Count > 0 )
                {
                    foreach (var item in addJug)
                    {
                        /*
                        var jugador = new schemaJugadorEquipos();
                        if (item.jugUserId != null)
                            jugador = getJugadorByUserId(item.jugUserId,item.jugCorreo);
                        else
                            jugador = null;
                        var id= 0;
                        
                        if(jugador==null)
                        {
                            var jug = new schemaJugadores();
                            jug.jugCorreo = item.jugCorreo;
                            jug.jugEstatus = item.jugEstatus;
                            jug.jugFechaCreacionUTC = DateTime.Now;
                            jug.jugNombre = item.jugNombre;
                            jug.jugUserId = item.jugUserId;
                            
                            dbApp.tblJugadores.Add(jug);
                            dbApp.SaveChanges();
                            id =jug.jugId;
                        }else
                        {
                            id = jugador.jugId;
                        }*/
                        try
                        {
                            var jugEqu = new schemaJugadorEquipos();
                            var jugE = dbApp.tblJugadorEquipos.Where(l => l.equId == equId && l.jugCorrreo == item.jugCorreo).ToList();
                            if (jugE.Count() > 0 && jugE != null)
                            {
                                jugE.ForEach(l => l.jugEstatus = true);
                            }
                            else
                            {
                                jugEqu.jugUserId = item.jugUserId;
                                jugEqu.torId = model.torId;
                                jugEqu.equId = model.equId;
                                jugEqu.equIdRef = model.equId;
                                jugEqu.jugCodigoConfirmacion = codigoConfirmacion;
                                jugEqu.jugConfirmado = false;
                                jugEqu.jugEstatus = true;
                                jugEqu.jugPagado = false;
                                jugEqu.jugFechaVencimientoPagoUTC = equiFechaVencimientoPagoUTC;
                                jugEqu.jugPrecioTorneo = model.torPrecio;
                                jugEqu.jugCorrreo = item.jugCorreo;
                                dbApp.tblJugadorEquipos.Add(jugEqu);
                            }

                            dbApp.SaveChanges();
                            var jid = jugEqu.jugEquId;
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            Global_Functions.saveErrors(ex.ToString(), false);
                            throw;
                        }
                       
                    }
                }

                if (deleteJug.Count > 0)
                {
                     foreach (var item in deleteJug)
                    {
                        try
                        {
                            var jugDelete = dbApp.tblJugadorEquipos.Where(l => l.jugCorrreo == item.jugCorreo && l.equId == model.equId).ToList();
                            if (jugDelete.Any())
                            {
                                dbApp.tblJugadorEquipos.RemoveRange(jugDelete);

                                dbApp.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            Global_Functions.saveErrors(ex.ToString(), false);
                            throw;
                        }

                    }
                }

                /*
                if (jugReintegrar.Count > 0)
                {
                    try
                    {
                        foreach (var item in jugReintegrar)
                        {
                            var jugRe = dbApp.tblJugadores.Where(l => l.jugCorreo== item.jugCorreo).FirstOrDefault();
                            if (jugRe!=null)
                            {

                                var ReintJug = dbApp.tblJugadorEquipos.Where(l => l.equId == equId).ToList();

                                if (ReintJug!=null)
                                {
                                    var jugByUId = ReintJug.Where(l => l.jugUserId == item.jugUserId).FirstOrDefault();
                                    if (jugByUId!=null)
                                    {
                                        jugByUId.jugEstatus = item.jugEstatus;
                                        dbApp.SaveChanges();
                                    }
                                    else
                                    {
                                        var jugById = ReintJug.Where(l => l.jugId == jugRe.jugId).FirstOrDefault();
                                        if (jugById != null)
                                        {
                                            jugById.jugEstatus = item.jugEstatus;
                                            dbApp.SaveChanges();
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        Global_Functions.saveErrors(ex.ToString(), false);
                        throw;
                    }
                }*/
                dbApp.SaveChanges();
                return model.equId;
            }
            catch(Exception ex)
            {
                ex.ToString();
                Global_Functions.saveErrors(ex.ToString(), false);
            }

            return 0;
        }
        public List<schemaUsersMisCuentas> getMainPerfilById(string jugUserId)
        {
            try
            {
                var jug = dbApp.tblUsersMisCuentas.Where(l => l.userId.Equals(jugUserId)).ToList();
                return jug;
            }
            catch (Exception ex)
            {
                ex.ToString();
                Global_Functions.saveErrors(ex.ToString(), false);
                return null;
                throw;
            }
        }

        /// <summary>
        /// Confirma la invitacion de pertenecer a un torneo y/o equipo.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="torId"></param>
        /// <param name="equId"></param>
        /// <returns></returns>
        public bool setJugadoresEquipo_ConfirmarParticipacion(string email, int torId, int equId,string userId,string code ,bool pay = false)
        {
            try
            {
                var jugador = getJugadorByIds(email, torId, equId);
                var tor = dbApp.tblTorneos.Where(l => l.torId == torId).FirstOrDefault();
               
                if (jugador != null)
                {
                    if (jugador.jugUserId == "" || jugador.jugUserId == null)
                    {
                        jugador.jugUserId = userId;
                    }
                    if(jugador.jugCorrreo == "" || jugador.jugCorrreo== null)
                    {
                        jugador.jugCorrreo= dbApp.tblUsersProfiles.Where(l => l.userIdOwner == userId).First().uprNombres;
                    }
                    var player = new schemaJugadorEquipos();
                   
                    if (code!="")
                    {
                        player = dbApp.tblJugadorEquipos.Where(l => l.torId == torId && l.equId == equId && l.jugCodigoConfirmacion == code).FirstOrDefault();
                    }else
                    {
                        player = dbApp.tblJugadorEquipos.Where(l => l.torId == torId && l.equId == equId && l.jugUserId == userId).FirstOrDefault();
                    }


                    if (player != null)
                    {
                        player.jugCodigoConfirmacion = null;
                        player.jugConfirmado = true;
                        if (pay)
                            player.jugPagado = true;
                        player.jugUserId = userId;
                        player.jugPrecioTorneo = tor.torPrecioTorneo;
                        dbApp.SaveChanges();
                        return true;
                    }else
                    {
                        var jugC = dbApp.tblJugadores.Where(l => l.jugCorreo.Equals(email)).FirstOrDefault();
                        int diasPago = (tor.torDiasParaPago == 0 || tor.torDiasParaPago == null) ? 1 : (int)tor.torDiasParaPago;
                        var jugEqu = new schemaJugadorEquipos();
                        jugEqu.jugUserId = userId;
                        jugEqu.equId = equId;
                        jugEqu.torId = torId;
                        jugEqu.jugCodigoConfirmacion = null;
                        jugEqu.jugConfirmado = true;
                        if (pay)
                            jugEqu.jugPagado = true;
                        jugEqu.jugFechaVencimientoPagoUTC = DateTime.Now.AddDays(diasPago);
                        jugEqu.jugPrecioTorneo = (tor.torPrecioTorneo != null) ? tor.torPrecioTorneo : 0;

                        dbApp.tblJugadorEquipos.Add(jugEqu);

                        dbApp.SaveChanges();
                        return true;
                    }               
                                  
                }else
                {
                    int diasPago = (tor.torDiasParaPago == 0 || tor.torDiasParaPago == null) ? 1 : (int)tor.torDiasParaPago;
                    
                    var jugEqu = new schemaJugadorEquipos();
                    jugEqu.jugUserId = userId;
                    jugEqu.equId = equId;
                    jugEqu.torId = torId;
                    jugEqu.jugCodigoConfirmacion = null;
                    jugEqu.jugCorrreo = email;
                    jugEqu.jugConfirmado = true;
                    if (pay)
                        jugEqu.jugPagado = true;
                    jugEqu.jugFechaVencimientoPagoUTC = DateTime.Now.AddDays(diasPago);
                    jugEqu.jugPrecioTorneo = (tor.torPrecioTorneo!=null)? tor.torPrecioTorneo:0;                       
                    
                    dbApp.tblJugadorEquipos.Add(jugEqu);                    

                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }

        /// <summary>
        /// Rechaza la invitacion de pertenecer a un torneo y/o equipo.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="torId"></param>
        /// <param name="equId"></param>
        /// <returns></returns>
        public bool setJugadoresEquipo_RechazarParticipacion(string email, int torId, int equId)
        {
            try
            {
                var jugador = getJugadorByIds(email, torId, equId);
                var jugadorE = dbApp.tblJugadorEquipos.Where(l => l.equId == equId && l.torId == torId && l.jugCorrreo == email).FirstOrDefault();
                if (jugadorE != null)
                {
                    jugadorE.jugCodigoConfirmacion = null;
                    jugadorE.jugConfirmado = false;

                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public bool setArbitroTorneo_RechazarParticipacion(string email, int torId)
        {
            try
            {
                var arbitro = getArbitroLigId(email, torId).First();
                if (arbitro != null)
                {
                    arbitro.tblArbitros.arbEstatus = false;
                    arbitro.arbCodigoConfirmacion = null;
                    arbitro.arbConfirmado = false;

                    dbApp.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        /// <summary>
        /// Metodo que busca al participante especifico y le cambia el codigo de invitación de participación en el torneo y/o equipo.
        /// </summary>
        /// <param name="jugCorreo"></param>
        /// <param name="torId"></param>
        /// <param name="equId"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public schemaJugadorEquipos setJugadoresEquipo_VolverInvitar(string jugCorreo, int torId, int equId, string codigo)
        {
            try
            {
                var jugador = getJugadorByIds(jugCorreo, torId, equId);
                if (jugador != null)
                {
                    int diasPago = (jugador.tblTorneos.torDiasParaPago == 0) ? 1 : (int)jugador.tblTorneos.torDiasParaPago;

                    var jugEqu = dbApp.tblJugadorEquipos.Where(l => l.jugUserId == jugador.jugUserId && l.torId == torId && l.equId == equId).FirstOrDefault();

                    jugEqu.jugFechaVencimientoPagoUTC = DateTime.Now.AddDays(diasPago);
                    jugEqu.jugCodigoConfirmacion = codigo;

                    dbApp.SaveChanges();
                }else
                {
                    var tor = dbApp.tblTorneos.Where(l => l.torId == torId).FirstOrDefault();
                    int diasPago = (tor.torDiasParaPago == 0) ? 1 : (int)tor.torDiasParaPago;
                    
                    var jugEqu = new schemaJugadorEquipos();
                    jugEqu.equId = equId;
                    jugEqu.torId = torId;
                    jugEqu.jugCodigoConfirmacion = codigo;
                    jugEqu.jugConfirmado = false;
                    jugEqu.jugFechaVencimientoPagoUTC = DateTime.Now.AddDays(diasPago);
                    jugEqu.jugPrecioTorneo = tor.torPrecioTorneo;

                    dbApp.tblJugadorEquipos.Add(jugEqu);

                    dbApp.SaveChanges();
                }
                return jugador;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Agrega un nuevo Partido.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int setPartido_Agregar(schemaPartidos model)
        {
            try
            {
                if (model!=null)
                {
                   /* var partido = new schemaPartidos();
                    partido.arbId = (model.arbId != null)? model.arbId:0;
                    partido.arbNombre = (model.arbNombre != null) ? model.arbNombre : "";
                    partido.equIdDos = (model.equIdDos != null) ? model.equIdDos : 0;
                    partido.equIdUno = model.equIdUno;
                    partido.equNombreEquipoDos = (model.equNombreEquipoDos != null) ? model.equNombreEquipoDos : "";
                    partido.equNombreEquipoUno = model.equNombreEquipoUno;
                    partido.equResultadoDos = model.equResultadoDos;
                    partido.equResultadoUno = model.equResultadoUno;
                    partido.lcatId = (model.lcatId != null) ? model.lcatId : 0;
                    partido.ligId = model.ligId;
                    partido.parEstado = model.parEstado;
                    partido.parEstatus = model.parEstatus;
                    partido.parFecha_Fin = model.parFecha_Fin;
                    partido.parFecha_Inicio = model.parFecha_Inicio;
                    partido.torId = model.torId;
                    */
                    dbApp.tblPartidos.Add(model);
                    dbApp.SaveChanges();
                    return model.parId;
                }
              
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
            }
            return 0;
        }

        /// <summary>
        /// Da de baja un Partido.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setPartidos_Delete(int parId)
        {
            try
            {
                var item = getPartidosById(parId);
                item.parEstatus = false;
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public schemaPartidos getPartidosById(int parId)
        {
            return dbApp.tblPartidos.Where(l => l.parId == parId).FirstOrDefault();
        }
        /// <summary>
        /// Regresa los equipo que pertenecen al torneo.
        /// </summary>
        /// <param name="torId"></param>
        /// <returns></returns>
        public List<schemaEquipos> getEquiposByTorneo(int torId)
        {
            var Equipos = (from equ in dbApp.tblEquipos
                    where (equ.torId == torId && equ.equDelete == false)
                    select equ)
                    .Distinct()
                    .ToList();
            return Equipos;
        }
        public List<PartidosViewModel> getPartidosPlayer(ApplicationUser user)
        {
            var jugEqu = dbApp.tblJugadorEquipos.Where(l => l.jugUserId == user.Id && l.equId != null && l.jugConfirmado==true).ToList();
           // if (jugEqu.Any())
           // {
                var equipo = new List<schemaEquipos>();
                var partidos = new List<PartidosViewModel>();
                foreach (var item in jugEqu)
                {
                    var team = dbApp.tblEquipos.Where(l => l.equId == item.equId).FirstOrDefault();
                    equipo.Add(team);
                }
                
                partidos = (from equ in equipo
                            join par in dbApp.tblPartidos on equ.torId equals par.torId
                            let can = (par.lcatId != 0) ? dbApp.tblLigaCanchasTorneos.Where(l => l.lcatId == par.lcatId).FirstOrDefault() : null
                            where par.equIdUno == equ.equId ||
                                    par.equIdDos == equ.equId
                            select new PartidosViewModel()
                            {
                                parId = par.parId,
                                ligId = par.ligId,
                                ligNombre = par.tblLigas.ligNombreLiga,
                                torId = par.torId,
                                torNombre = par.tblTorneos.torNombreTorneo,
                                arbId = par.arbId,
                                equIdUno = par.equIdUno,
                                equIdDos = par.equIdDos,
                                equNombreEquipoUno = par.equNombreEquipoUno,
                                equNombreEquipoDos = (par.equNombreEquipoDos!=null)? par.equNombreEquipoDos:"",
                                imgUno = (dbApp.tblEquipos.Where(e => e.equId == par.equIdUno).FirstOrDefault().equImgUrl!=null)? dbApp.tblEquipos.Where(e => e.equId == par.equIdUno).FirstOrDefault().equImgUrl:"",
                                imgDos = (par.imgDos != null) ? par.imgDos : (dbApp.tblEquipos.Where(e => e.equId == par.equIdDos).FirstOrDefault() != null)? dbApp.tblEquipos.Where(e => e.equId == par.equIdDos).FirstOrDefault().equImgUrl:"",
                                canId = (can != null)? can.lcatId:0,
                                canNombre = (can != null)? can.lcatNombre:"",
                                parFecha_Inicio = par.parFecha_Inicio,
                                parFecha_Fin = par.parFecha_Fin,
                                colDeporte = par.tblTorneos.tblCategoriaTorneo.depNombre,
                                arbNombre = par.arbNombre,
                                equResultadoUno = par.equResultadoUno,
                                equResultadoDos = par.equResultadoDos
                            }).ToList();
                return partidos;
            //}
            //return null;
        }
        public List<PartidosViewModel> getPartidosRefeere(ApplicationUser user)
        {
            var partidos = getPartidos(user);
            //var arbId = getArbitroByEmail(user.Email).FirstOrDefault().arbId;
            //var partidos = (from par in dbApp.tblPartidos
            //                join arb in dbApp.tblArbitros on par.arbId equals arbId
            //                select par).Distinct().ToList();
            return partidos;
        }
        /*
         * 
         */
        public List<PartidosViewModel> getPartidos(ApplicationUser user)
        {
            //List<schemaLigas> ligas = new List<schemaLigas>();
            var roles = getRoles();
            var listRol = "";
            if (user!=null)
            {
                listRol = roles.Where(l => l.rolId.ToUpper() == user.usuRolActual.ToUpper()).First().rolName.ToString();
            }

            if (user != null)
            {
                if (listRol.ToUpper() == constClass.rolOwners)
                {
                    var matchList = new List<PartidosViewModel>();

                    var liga = getPartidosByLigas(user).ToList();

                    matchList.AddRange(liga);
                    return matchList;
                }               
                
                var partidos = getListPartidos().ToList();

                if (listRol.ToUpper() == constClass.rolReferee)
                {
                    return partidos.Where(l => l.arUserId == user.Id).ToList();
                }
                if (listRol.ToUpper() == constClass.rolAdminTorneos)
                {
                    var torneo = (from tor in dbApp.tblTorneos
                                  join tca in dbApp.tblTorneoCoAdministradores on tor.torId equals tca.torId into tor_tca
                                  from tca in tor_tca.DefaultIfEmpty()
                                  where (tor.torUserIdCreador == user.Id
                                      || tca.userCorreo == dbApp.Users.Where(l=> l.Id == user.Id).FirstOrDefault().Email)
                                  select tor).ToList();

                    var list = (from tor in torneo
                                join part in partidos on tor.torId equals part.torId                                
                                select part).Distinct().ToList();

                    return list;
                }
                if (listRol.ToUpper() == constClass.rolPlayer)
                {
                    var matchList = new List<PartidosViewModel>();
                    var equipos = getEquipoByPlayer(user.Id);

                    var model = (from part in partidos
                                 join equ in equipos on part.torId equals equ.torId
                                 where part.equIdUno == equ.equId ||
                                        part.equIdDos == equ.equId
                                 select part).ToList();

                    var cuentas = getMisCuentas(user.Id);

                    if (cuentas.Count > 0)
                    {
                        foreach (var subCounts in cuentas)
                        {
                            var team = getEquipoByPlayer(subCounts.userId);
                            if (team.Count > 0)
                            {
                                var subMatch = (from part in getListPartidos()
                                                join equ in team on part.torId equals equ.torId
                                                where part.equIdUno == equ.equId ||
                                                       part.equIdDos == equ.equId
                                                select part).ToList();

                                if (subMatch.Count != 0)
                                {
                                    model.AddRange(subMatch);
                                }
                            }
                        }
                    }
                                                            
                    
                    return model;
                }
                else if (listRol.ToUpper() == constClass.rolCoach)
                {
                    var partCoach = getEquipoByAdmin(user.Id).OrderBy(o=> o.equId).Distinct().ToList();
                    List<int> ids = new List<int> ();
                    foreach (var item in partCoach)
                    {
                        ids.Add(item.equId);
                    }

                    var par2 = partidos.Where(l => ids.Contains(l.equIdUno) || ids.Contains(l.equIdDos)).ToList();
                    return par2;
                }
                else
                    return partidos;
            }
            return getListPartidos().ToList();
        }

        public List<PartidosViewModel> getPartidosByLigas(ApplicationUser user)
        {
            return (from lig in dbApp.tblLigas
                    join lca in dbApp.tblLigaCoAdministradores on lig.ligId equals lca.ligId into lig_lca
                    from lca in lig_lca.DefaultIfEmpty()
                    join par in dbApp.tblPartidos on lig.ligId equals par.ligId
                    let can = (par.lcatId != 0 )? dbApp.tblLigaCanchasTorneos.Where(l => l.lcatId == par.lcatId).FirstOrDefault() : null
                        where (lig.ligUserIdCreador == user.Id
                         || lca.lcaUserId == user.Id)
                         && lig.ligEstatus == true
                         && lig.ligAprobada == true
                         select new PartidosViewModel()
                         {
                             userId = lig.ligUserIdCreador,
                             parId = par.parId,
                             ligId = par.ligId,
                             ligNombre = lig.ligNombreLiga,
                             torId = par.torId,
                             torNombre = par.tblTorneos.torNombreTorneo,
                             arbId = par.arbId,
                             equIdUno = par.equIdUno,
                             equIdDos = par.equIdDos,
                             equNombreEquipoUno = par.equNombreEquipoUno,
                             equNombreEquipoDos = par.equNombreEquipoDos,
                             imgUno = dbApp.tblEquipos.Where(e => e.equId == par.equIdUno).FirstOrDefault().equImgUrl,
                             imgDos = (par.imgDos != null) ? par.imgDos : dbApp.tblEquipos.Where(e => e.equId == par.equIdDos).FirstOrDefault().equImgUrl,
                             canId = can != null ? can.lcatId : 0,
                             canNombre = can != null ? can.lcatNombre : "",
                             parFecha_Inicio = par.parFecha_Inicio,
                             parFecha_Fin = par.parFecha_Fin,
                             colDeporte = par.tblTorneos.tblCategoriaTorneo.depNombre,
                             arbNombre = par.arbNombre,
                             arUserId = (par.arbId > 0) ? dbApp.tblArbitros.Where(l => l.arbId == par.arbId).FirstOrDefault().arbUserId : "",
                             equResultadoUno = par.equResultadoUno,
                             equResultadoDos = par.equResultadoDos,
                             parEstatus = par.parEstatus,
                             parCheck = par.parCheck
                         }).Distinct().ToList();
        }
        public List<PartidosViewModel> getListPartidos()
        {
            var partidos = (from par in dbApp.tblPartidos
                            join lig in dbApp.tblLigas on par.ligId equals lig.ligId
                            join tor in dbApp.tblTorneos on par.torId equals tor.torId                            
                            join lcat in dbApp.tblLigaCategoriasTorneos on lig.ligId equals lcat.ligId
                            let can = (par.lcatId != 0) ? dbApp.tblLigaCanchasTorneos.Where(l => l.lcatId == par.lcatId).FirstOrDefault() : null
                            let arbConf = dbApp.tblArbitrosPartidos.Where(l=> l.parId == par.parId && l.arbConfirmado==true).FirstOrDefault()
                            where tor.lctId == lcat.lctId
                            select new PartidosViewModel()
                            {
                                userId = lig.ligUserIdCreador,
                                parId = par.parId,
                                ligId = par.ligId,
                                ligNombre = lig.ligNombreLiga,
                                torId = par.torId,
                                torNombre = tor.torNombreTorneo,
                                arbId = par.arbId,
                                equIdUno = par.equIdUno,
                                equIdDos = par.equIdDos,
                                equNombreEquipoUno = par.equNombreEquipoUno,
                                equNombreEquipoDos = par.equNombreEquipoDos,
                                imgUno = dbApp.tblEquipos.Where(e => e.equId == par.equIdUno).FirstOrDefault().equImgUrl,
                                imgDos = (par.imgDos != null) ? par.imgDos : dbApp.tblEquipos.Where(e => e.equId == par.equIdDos).FirstOrDefault().equImgUrl,
                                canId = (can != null)?can.lcatId:0,
                                canNombre = (can != null) ? can.lcatNombre:"",
                                parFecha_Inicio = par.parFecha_Inicio,
                                parFecha_Fin = par.parFecha_Fin,
                                colDeporte = lcat.depNombre,
                                arbNombre = par.arbNombre,
                                arUserId = (par.arbId > 0 && arbConf!=null) ? dbApp.tblArbitros.Where(l => l.arbId == par.arbId).FirstOrDefault().arbUserId : "",
                                equResultadoUno = par.equResultadoUno,
                                equResultadoDos = par.equResultadoDos,
                                parEstatus = par.parEstatus,
                                parCheck = par.parCheck
                            }).Distinct().ToList();
            return partidos;
        }

        public List<PartidosViewModel> getPartidosEquipos(ApplicationUser user)
        {
            try
            {
                var partidos = getPartidos(user);
                
                return partidos;
            }
            catch (Exception ex)
            {
                ex.ToString();
                Global_Functions.saveErrors(ex.ToString(), false);
                throw;
            }

            
        }

        public schemaPartidos getPartidoById(int parId)
        {
            return dbApp.tblPartidos
                        .Where(l => l.parId == parId)
                        .FirstOrDefault();
        }
        public List<schemaJuegosFutbolEstadisticasJugador> getJugadorPartidosById(int parId)
        {
            return dbApp.tblJuegosFutbolEstadisticasJugador
                   .Where(l => l.parId == parId).ToList();
        }


        public List<schemaPartidos> getPartidosByTorneoId(int torId)
        {
            return dbApp.tblPartidos
                        .Where(l => l.torId == torId)
                        .ToList();
        }

        public bool setPartidos_Edit(PartidosViewModel model)
        {
            try
            {
                var item = getPartidosById(model.parId);
                if (item != null)
                {
                    //var arbNombre = dbApp.tblArbitros.Where(l => l.arbId == item.arbId).First().arbNombre;
                    if (model.torTipo == constClass.torTipoInterno)
                    {
                        if (model.arbNombre != null)
                        {
                            if (model.arbId <= 0)
                            {
                                var arbid = dbApp.tblArbitros.Where(l => l.arbCorreo == model.arbNombre).FirstOrDefault().arbId;
                                model.arbId = arbid;
                            }
                            model.arbNombre = (model.arbNombre != "") ? model.arbNombre : null;
                        }
                    }                    
                   
                    item.ligId = model.ligId;
                    item.torId = model.torId;
                    item.lcatId = model.canId;
                    item.equIdUno = model.equIdUno;
                    item.equNombreEquipoUno = model.equNombreEquipoUno;
                    item.equIdDos = model.equIdDos;
                    item.equNombreEquipoDos = model.equNombreEquipoDos;

                    item.parFecha_Inicio = model.parFecha_Inicio;
                    item.parFecha_Fin = model.parFecha_Inicio.AddHours(model.parHour).AddMinutes(model.parMinutes);

                    item.parEstado = model.parEstado;
                    item.parEstatus = model.parEstatus;
                    item.equResultadoUno = model.equResultadoUno;
                    item.equResultadoDos = model.equResultadoDos;
                    item.arbId = model.arbId;
                    item.arbNombre = model.arbNombre;
                    dbApp.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public bool setPartidos_UpdateScore(PartidosViewModel model)
        {
            try
            {
                var item = getPartidosById(model.parId);
                if (item != null)
                {
                    item.parEstado = model.parEstado;
                    item.equResultadoUno = model.equResultadoUno;
                    item.equResultadoDos = model.equResultadoDos;
                    item.parCheck = true;
                    dbApp.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
        public bool setFutbolEstadisticasJugador(int parId , int team, int score,int asis,int faltas,int amarillas,int rojas,int parSusp,string jugIds)
        {
            try
            {
                var FutEstadistica = dbApp.tblJuegosFutbolEstadisticasJugador.Where(l => l.parId == parId && l.equId == team && l.UserIdJugador == jugIds);
                var total = FutEstadistica.Count();
                if (FutEstadistica.Count() > 0 || FutEstadistica == null)
                {
                    FutEstadistica.First().jfejGoles = score;
                    FutEstadistica.First().UserIdJugador = jugIds;
                    FutEstadistica.First().jfejAsistencias = asis;
                    FutEstadistica.First().jfejFaltas = faltas;
                    FutEstadistica.First().jfejTarjetasAmarillas = amarillas;
                    FutEstadistica.First().jfejTarjetasRojas = rojas;
                    FutEstadistica.First().jfejPartidosSuspendidos = parSusp;
                    SaveChanges();
                    return true;
                }
                else
                {
                    var model = new schemaJuegosFutbolEstadisticasJugador();
                    model.equId = team;
                    model.parId = parId;
                    model.jfejGoles = score;
                    model.jfejAsistencias = asis;
                    model.jfejFaltas = faltas;
                    model.jfejTarjetasAmarillas = amarillas;
                    model.jfejTarjetasRojas = rojas;
                    model.jfejPartidosSuspendidos = parSusp;
                    model.UserIdJugador = jugIds;

                    dbApp.tblJuegosFutbolEstadisticasJugador.Add(model);
                    dbApp.SaveChanges();
                    return true;
                }
                
            }
            catch (Exception)
            {

                return false;
                throw;
            }
        }
        public List<schemaJuegosFutbolEstadisticasJugador> getEstadisticaFutbolByPartido(int parId)
        {
            try
            {
                var estadisticas = dbApp.tblJuegosFutbolEstadisticasJugador.Where(p => p.parId.Equals(parId)).ToList();

                return estadisticas;
            }
            catch (Exception)
            {
                return null;
                throw;
            }

        }
        //Devuelve las estadisticas de un torneo
        public List<TorneoEstGoleador> getEstadisticasLiderGoleo(int torId)
        {
            var result = (from futEst in dbApp.tblJuegosFutbolEstadisticasJugador
                          join par in dbApp.tblPartidos on futEst.parId equals par.parId
                          join equ in dbApp.tblEquipos on futEst.equId equals equ.equId
                          join jug in dbApp.tblJugadorEquipos on futEst.UserIdJugador equals jug.jugUserId
                          let jugNombre = (jug.jugUserId != null)? (dbApp.tblUsersProfiles.Where(l=> l.userIdOwner == jug.jugUserId && l.uprNombres != "-").Any())? dbApp.tblUsersProfiles.Where(l => l.userIdOwner == jug.jugUserId).FirstOrDefault().uprNombres : jug.jugCorrreo : jug.jugCorrreo
                          where jug.equId == futEst.equId
                          && par.torId == torId && par.parCheck == true && equ.equDelete == false
                          select new TorneoEstGoleador()
                          {
                              equId = futEst.equId,
                              equNombre = futEst.tblEquipo.equNombreEquipo,
                              jugNombre = jugNombre,
                              goles = futEst.jfejGoles,
                              faltas = futEst.jfejFaltas,
                              amarillas = futEst.jfejTarjetasAmarillas,
                              rojas = futEst.jfejTarjetasRojas,
                              parId = par.parId
                          }).Distinct().ToList();
            
            return result;
         }

        public List<TablaGeneralTorneo> getEstadisticasTorneo(int torId, int? equId = null, int? parId=null)
        {
            var result = new List<TablaGeneralTorneo>();
            if (equId != null)
            {
                result = (from par in dbApp.tblPartidos
                          join equ in dbApp.tblEquipos on par.torId equals equ.torId
                          where equ.torId == torId && equ.equId == equId && equ.equDelete == false
                          select new TablaGeneralTorneo()
                          {
                              equId = equ.equId,
                              equNombre = equ.equNombreEquipo,
                              torId = torId,
                              scoreUno = par.equResultadoUno,
                              scoreDos = par.equResultadoDos
                          }).ToList();
            }
            else if (parId != null)
            {
                result = (from par in dbApp.tblPartidos
                          where par.parId == parId
                          select new TablaGeneralTorneo()
                          {
                              
                          }
                          ).ToList();
            }else
            {
                result = (from equ in dbApp.tblEquipos
                          where equ.torId == torId && equ.equDelete == false
                          select new TablaGeneralTorneo()
                          {
                              equId = equ.equId,
                              equNombre = equ.equNombreEquipo,
                              torId = torId
                          }).ToList();
            }

            foreach (var item in result)
            {
                var goles = dbApp.tblJuegosFutbolEstadisticasJugador.Where(l => l.equId == item.equId);
                var golVal = goles.Any();
                if (goles.Count()>0 && golVal)
                    item.golFavor = goles.Sum(s => s.jfejGoles);

                var partido = dbApp.tblPartidos.Where(l => l.equIdUno == item.equId || l.equIdDos == item.equId && l.parEstatus == true && l.torId == torId).ToList();
                partido = partido.Where(l => l.parCheck == true).ToList();
                if (partido.Count()>0)
                    item.parJugados = partido.Count();

                var parGan = 0; var parEmp = 0; var parPer = 0; var golF = 0; var golC = 0;

                var partGanEmpPer = dbApp.tblPartidos.Where(l => l.equIdUno == item.equId && l.parCheck == true);
                
                if (partGanEmpPer.Count() > 0)
                {                    
                    foreach (var par in partGanEmpPer)
                    {
                        if (par.equResultadoUno > par.equResultadoDos)
                        {
                            parGan++;
                            golF = golF + par.equResultadoUno;
                            golC = golC + par.equResultadoDos;
                        }
                        else if (par.equResultadoUno == par.equResultadoDos)
                        {
                            parEmp++;
                            golF = golF + par.equResultadoUno;
                            golC = golC + par.equResultadoDos;
                        }
                        else if (par.equResultadoUno < par.equResultadoDos)
                        {
                            parPer++;
                            golF = golF + par.equResultadoUno;
                            golC = golC + par.equResultadoDos;
                        }
                    }
                }
                var partGanEmpPer2 = dbApp.tblPartidos.Where(l => l.equIdDos == item.equId && l.parCheck == true);
                if (partGanEmpPer2.Count() > 0)
                {
                    foreach (var par in partGanEmpPer2)
                    {
                        if (par.equResultadoDos > par.equResultadoUno)
                        {
                            parGan++;
                            golF = golF + par.equResultadoDos;
                            golC = golC + par.equResultadoUno;
                        }
                            
                        else if (par.equResultadoDos == par.equResultadoUno)
                        {
                            parEmp++;
                            golF = golF + par.equResultadoDos;
                            golC = golC + par.equResultadoUno;
                        }
                            
                        else if (par.equResultadoDos < par.equResultadoUno)
                        {
                            parPer++;
                            golF = golF + par.equResultadoDos;
                            golC = golC + par.equResultadoUno;
                        }                           
                    }
                }
                var parJugados = (item.parJugados > 0) ? (int)item.parJugados : 0;
                var puntos = dbApp.tblTorneos.Where(l => l.torId == torId).FirstOrDefault();
                int parRest = (puntos.torNumeroJuegos!=null)? (int)puntos.torNumeroJuegos : 0 - (int)parJugados;
                int ptsGan = (puntos.torPuntosGanar!=null)?(int)puntos.torPuntosGanar:0;
                int ptsEmp = (puntos.torPuntosEmpatar != null) ? (int)puntos.torPuntosEmpatar : 0;
                int ptsPer = (puntos.torPuntosPerder!=null)? (int)puntos.torPuntosPerder:0;

                var per = (ptsPer == 0) ? 0 : ptsPer*parPer;

                item.parGanados = parGan;
                item.parEmpatados = parEmp;
                item.parPerdidos = parPer;
                item.golContra = golC;
                item.difGoles = golF - golC;
                                
                item.puntos = ( (parGan * ptsGan) + (parEmp * ptsEmp) + per);

                item.partidosRest = parRest;
            }
           return result;
        }
        public schemaArbitros getArbitroById(int arbId)
        {
            return dbApp.tblArbitros.Where(l => l.arbId == arbId).FirstOrDefault();
        }
        public int setArbitros(schemaArbitros model,int torId, int? arbId = null,bool edit=false)
         {
             try
             {
                 if (model != null)
                 {
                     if (edit==true)
                     {
                         var item = getArbitros().Where(l=> l.arbId == arbId).FirstOrDefault();
                         item.arbNombre = model.arbNombre;
                         item.arbCorreo = model.arbCorreo;
                         model.arbId = (int)arbId;
                    }
                    else
                    {
                        var arb = dbApp.tblArbitros.Where(l => l.arbCorreo == model.arbCorreo).FirstOrDefault();
                        if (arb!=null)
                        {
                            return arb.arbId;
                        }else
                            dbApp.tblArbitros.Add(model);
                    }    
                    dbApp.SaveChanges();
                    return model.arbId;                   
                 }
             }
             catch (Exception ex)
             {
                 ex.ToString();
             }
             return 0;
         }

        public bool setArbitroLiga(int arbId,int ligId, string codigo,bool edit=false)
        {
            try
            {
                if (edit==true)
                {
                    var arbLiga = dbApp.tblArbitrosLigas.Where(l=> l.arbId == arbId && l.ligId == ligId).FirstOrDefault();
                    arbLiga.ligId=ligId;
                    if (codigo!="")
                    {
                        arbLiga.arbCodigoConfirmacion = codigo;
                    }
                }
                else
                {
                    schemaArbitrosLigas arbLig = new schemaArbitrosLigas();
                    arbLig.arbId = arbId;
                    arbLig.ligId = ligId;
                    arbLig.arbCodigoConfirmacion = codigo;
                    dbApp.tblArbitrosLigas.Add(arbLig);
                }
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }

        public bool setArbitroPartido(int arbId, int parId, string codigo)
        {
            try
            {
                schemaArbitrosPartidos arbPar = new schemaArbitrosPartidos();
                arbPar.arbId = arbId;
                arbPar.parId = parId;
                arbPar.arbCodigoConfirmacion = codigo;
                dbApp.tblArbitrosPartidos.Add(arbPar);
           
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            
        }
        public bool editArbitroPartido(int arbId, int new_arbId ,int parId, string codigo)
        {
            try
            {

                var arbPar = dbApp.tblArbitrosPartidos.Where(l => l.parId == parId && l.arbId == arbId).FirstOrDefault();

                dbApp.tblArbitrosPartidos.Remove(arbPar);

                var arb_partido = new schemaArbitrosPartidos();

                arb_partido.arbId = new_arbId;
                arb_partido.parId = parId;
                arb_partido.arbConfirmado = false;
                arb_partido.arbRechazar = false;
                arb_partido.arbCodigoConfirmacion = codigo;

                dbApp.tblArbitrosPartidos.Add(arb_partido);

                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }

        }
        public bool setDeleteArbitro(int arbId)
        {
            try
            {
                var arbitro = getArbitroById(arbId);
                if (arbitro != null)
                {
                    dbApp.tblArbitros.Remove(arbitro);
                    var arbLigas = dbApp.tblArbitrosLigas.Where(c => c.arbId == arbId).ToList();
                    if (arbLigas != null)
                    {
                        dbApp.tblArbitrosLigas.RemoveRange(arbLigas);
                    }
                    dbApp.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;

                throw;
            }
        }
        public schemaArbitrosPartidos getArbitroPartidoByParId(int parId)
        {
            return (from arbPar in dbApp.tblArbitrosPartidos
                    where arbPar.parId == parId
                    select arbPar).First();
        }

        public List<ArbitrosViewModel> getArbitroPartidoRechazado()
        {
            var arbitro = (from par in dbApp.tblPartidos
                         join arbPar in dbApp.tblArbitrosPartidos on par.parId equals arbPar.parId
                         join arb in dbApp.tblArbitros on arbPar.arbId equals arb.arbId
                         join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                         where arbPar.arbRechazar == true
                         select new ArbitrosViewModel()
                         {
                             arbCorreo = arb.arbCorreo,
                             arbId = arb.arbId,
                             parId = par.parId,
                             ligId = arbLig.ligId,
                             torNombre = par.tblTorneos.torNombreTorneo
                         }
                         ).ToList();

            return arbitro;
        }
        public List<ArbitrosViewModel> getArbitroPartidoAceptado()
        {
            var arbitro = (from par in dbApp.tblPartidos
                           join arbPar in dbApp.tblArbitrosPartidos on par.parId equals arbPar.parId
                           join arb in dbApp.tblArbitros on arbPar.arbId equals arb.arbId
                           join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                           where arbPar.arbConfirmado == true && arbPar.arbCodigoConfirmacion == null
                           select new ArbitrosViewModel()
                           {
                               arbCorreo = arb.arbCorreo,
                               arbId = arb.arbId,
                               parId = par.parId,
                               ligId = arbLig.ligId,
                               torNombre = par.tblTorneos.torNombreTorneo,
                               partido = par.equNombreEquipoUno + " vs " + par.equNombreEquipoDos
                           }
                         ).ToList();

            return arbitro;
        }
        public bool setConfirmarArbitroPartido(int arbId, int parId)
        {
            try
            {
                var arb = dbApp.tblArbitrosPartidos.Where(l => l.arbId == arbId && l.parId == parId).FirstOrDefault();
                if (arb != null)
                {
                    arb.arbCodigoConfirmacion = null;
                    arb.arbConfirmado = true;
                    dbApp.SaveChanges();
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
        public bool setUsuarioArbitro (int arbId,string userId)
        {
            try
            {
                var arb = dbApp.tblArbitros.Where(l => l.arbId == arbId).FirstOrDefault();
                if (arb != null)
                {
                    arb.arbUserId = userId;
                    dbApp.SaveChanges();
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
        public bool setRechazarArbitroPartido(int arbId, int parId)
        {
            try
            {
                var arb = dbApp.tblArbitrosPartidos.Where(l => l.arbId == arbId && l.parId == parId).FirstOrDefault();
                if (arb != null)
                {
                    arb.arbCodigoConfirmacion = null;
                    arb.arbConfirmado = false;
                    arb.arbRechazar = true;
                    dbApp.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
               
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }
         public bool setArbitroUserId(string userId, string email)
         {
            try
            {
                var arb = getArbitroByEmail(email).FirstOrDefault();

                if (arb != null)
                {
                    arb.arbUserId = userId;
                    dbApp.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }             
         }
         public List<schemaArbitros> getArbitroByEmail(string email)
         {
             var arbitro = dbApp.tblArbitros.Where(l => l.arbCorreo == email).ToList();
                              
             return arbitro;
         }
        public List<schemaArbitrosLigas> getArbitroLigId(string email,int ligId)
        {

            return (from arb in dbApp.tblArbitros.Where(l=> l.arbCorreo == email)
                    join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                    where arbLig.ligId == ligId
                    select arbLig)
                         .Distinct().ToList();
        }
         public List<schemaArbitros> getArbitros(int? ligId=null)
         {
             if (ligId != null)
             {
                 return (from arb in dbApp.tblArbitros
                         join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                         where arbLig.ligId == ligId
                         select arb)
                         .Distinct()
                         .ToList();
             }

             return dbApp.tblArbitros.OrderBy(l => l.arbFechaCreacionUTC).ToList();               
         }
         public List<ArbitrosViewModel> getArbitrosLigas(string userId)
         {
            // ArbitrosViewModel model = new ArbitrosViewModel();
            var arbitrosNuevo_Submit = getUserById(userId);

            var arbLigas = dbApp.tblArbitrosLigas.Count();
            if (arbLigas>0)
            {
                var data = (from arb in dbApp.tblArbitros
                            join arbLig in dbApp.tblArbitrosLigas on arb.arbId equals arbLig.arbId
                            select new ArbitrosViewModel()
                            {
                                arbId = arb.arbId,
                                ligId = arbLig.ligId,
                                arbCorreo = arb.arbCorreo,
                                arbNombre = arb.arbNombre,
                                ligNombre = arbLig.tblLigas.ligNombreLiga,
                                estado = (arbLig.arbConfirmado == false && arbLig.arbCodigoConfirmacion==null) ? "Rechazado" : (arbLig.arbConfirmado == true) ? "Aceptado" : "Pendiente",
                            }).Distinct().ToList();
                return data;
            }
            
           /*  if (userId!=null)
             {
                var torneos = getTorneosByUser(userId);
                 var arbUser = (from arb in data
                                join tor in torneos on arb.torId equals tor.torId
                                select arb).Distinct().ToList();
                 return arbUser;
             }
             */

             return null;
         }
        public List<schemaTorneos> getTorneosByUser(string userId)
        {
            var torneos = (from tor in dbApp.tblTorneos
                           join tca in dbApp.tblTorneoCoAdministradores on tor.torId equals tca.torId into tor_tca
                           from tca in tor_tca.DefaultIfEmpty()
                           where (tor.torUserIdCreador == userId
                               || tca.userCorreo == dbApp.Users.Where(l=> l.Id == userId).FirstOrDefault().Email )
                           select tor)
                               .Distinct()
                               .ToList();
            return torneos;
        }
        public List<schemaTorneos> getTorneosByTeam(string userId)
        {
            var user = getUserById(userId);
            var part = getPartidosEquipos(user).OrderBy(o => o.torId);

            var torIds = new List<int>();
            var ligIds = new List<int>();
            var tor = new List<schemaTorneos>();
            var lig = new List<schemaLigas>();
            foreach (var item in part)
            {
                if (!torIds.Contains(item.torId))
                {
                    torIds.Add(item.torId);
                    tor.Add(getTorneoById(item.torId));                    
                }
            }
            return tor;
        }
        public bool setArbitros_ConfirmarParticipacion(string userId,string email, int ligId)
         {
             try
             {
                 var arbitro = getArbitroLigId(email, ligId).FirstOrDefault();
                 if (arbitro != null)
                 {
                     arbitro.arbCodigoConfirmacion = null;
                     arbitro.arbConfirmado = true;                     
                     dbApp.SaveChanges();

                    // if (!userManager.IsInRole(user.Id, constClass.rolReferee))
                    //     userManager.AddToRole(user.Id, constClass.rolReferee);

                     return true;
                 }
             }
             catch (Exception ex)
             {
                 ex.ToString();
             }
             return false;
         }

         public List<PartidosViewModel> getEvents(ApplicationUser user, int? ligId = null, int? torId = null)
         {
             List<PartidosViewModel> Event = new List<PartidosViewModel>();
             var partidosData = getPartidos(user);

             if (ligId != null)
                 partidosData = partidosData.Where(l => l.ligId == ligId).ToList();
            if (torId != null && torId >0)
                partidosData = partidosData.Where(l => l.torId == torId).ToList();
            
             foreach (var par in partidosData)
             {                                
                 var imgUno = dbApp.tblEquipos.Where(l => l.equId == par.equIdUno).FirstOrDefault();
                 var imgDos = dbApp.tblEquipos.Where(l => l.equId == par.equIdDos).FirstOrDefault();
                 PartidosViewModel rec = new PartidosViewModel();
                 rec.parId = par.parId;
                 rec.ligId = par.ligId;
                 rec.ligNombre = par.ligNombre;
                 rec.torNombre = par.torNombre;
                 rec.equIdUno = par.equIdUno;
                 rec.imgUno = (imgUno != null) ? imgUno.equImgUrl : null;
                 rec.imgDos = (imgDos != null) ? imgDos.equImgUrl : null;
                 rec.equIdDos = par.equIdDos;
                 rec.equNombreEquipoUno = par.equNombreEquipoUno;
                 rec.equNombreEquipoDos = par.equNombreEquipoDos;
                 rec.canId = par.canId;
                 rec.canNombre = par.canNombre;
                 rec.parFecha_Inicio = par.parFecha_Inicio;
                 rec.parFecha_Fin = par.parFecha_Fin;
                rec.parEstatus = par.parEstatus;
                 string deporte = par.colDeporte.ToLower();
                 switch (deporte)
                 {
                     case "basquetbol":
                         rec.colDeporte = constClass.colBasquetbol;
                         break;
                     case "beisbol":
                         rec.colDeporte = constClass.colBeisbol;
                         break;
                     case "corredores":
                         rec.colDeporte = constClass.colCarreras;
                         break;
                     case "futbol":
                         rec.colDeporte = constClass.colFutbol;
                         break;
                     case "futbol americano":
                         rec.colDeporte = constClass.colAmericano;
                         break;
                     case "voleibol":
                         rec.colDeporte = constClass.colVoleibol;
                         break;
                     default:
                         rec.colDeporte = constClass.colDefault;
                         break;
                 }
                 Event.Add(rec);
             }
             return Event;
         }

        public List<PagosGridViewModel> getGridPagos(ApplicationUser user ,bool admin=false)
         {
             List<PagosGridViewModel> list = new List<PagosGridViewModel>();

            var rolActual = user.usuRolActual.ToUpper();
            var rolAdmin = getRoleByName(constClass.rolAdmin).Id.ToUpper();
            var rolLiga = getRoleByName(constClass.rolOwners).Id.ToUpper();
            var rolTorneo = getRoleByName(constClass.rolAdminTorneos).Id.ToUpper();
            var rolEquipo = getRoleByName(constClass.rolCoach).Id.ToUpper();
            var rolJugador = getRoleByName(constClass.rolPlayer).Id.ToUpper();
            if (rolAdmin == rolActual)
            {
                return getGridPagosAdmin();
            }
            if (rolTorneo == rolActual)
            {
               return getGridPagosTorneos(user.Id);
            } else if (rolEquipo == rolActual)
            {
                return getGridPagosEquipo().Where(l => l.userId == user.Id).ToList();
            }
            else if (rolJugador == rolActual)
            {
                return getGridPagosJugador().Where(l => l.userId == user.Id).ToList();
            }
            else if (rolLiga == rolActual)
            {
                var ligas = (from lig in dbApp.tblLigas
                             join usr in dbApp.tblUsersProfiles on lig.ligUserIdCreador equals usr.userIdOwner
                             where usr.userIdOwner == user.Id &&
                             lig.ligSolicitudRevisada == true &&
                             lig.ligAprobada == true                             
                             select lig
                             ).ToList();
                foreach (var lig in ligas)
                {
                    var view = new PagosGridViewModel();
                    var usuario = dbApp.tblUsersProfiles.Where(l => l.userIdOwner == lig.ligUserIdCreador).FirstOrDefault();
                    var pagos = dbApp.tblPagos.Where(l => l.conceptoId == lig.ligId && l.concepto == "Liga");

                    if (pagos.Count() > 0 && pagos != null)
                    {
                        var det = dbApp.tblDetallesPago.Where(l => l.IdPago == pagos.FirstOrDefault().IdPago).FirstOrDefault();

                        view.fechaPago = (lig.statusPago == "paid") ? det.FechaCreacionUTC.ToString() : lig.ligFechaRegistroUTC.ToString();
                    }
                    else
                    {
                        view.fechaPago = (lig.statusPago == "paid") ? DateTime.Now.ToString() : lig.ligFechaRegistroUTC.ToString();
                    }
                    view.userId = lig.ligUserIdCreador;
                    view.userName = usuario.uprNombres + " " + usuario.uprApellidos;
                    view.ligId = lig.ligId;
                    view.conceptoNombre = lig.ligNombreLiga.ToUpper();
                    view.total = "$ " + lig.ligTotalPagar + " MXN";
                    view.estado = (lig.statusPago == "paid") ? "Pagado" : "Pendiente";
                    view.conceptoId = lig.ligId;
                    view.adminView = (admin) ? "true" : "false";
                    list.Add(view);
                }
            }

           

             return list;
         }
        public List<PagosGridViewModel> getGridPagosAdmin()
        {
            var view = new PagosGridViewModel();
            List<PagosGridViewModel> list = new List<PagosGridViewModel>();

            var pagos = (from pag in dbApp.tblPagos
                         join dPag in dbApp.tblDetallesPago on pag.IdPago equals dPag.IdPago
                         let prof = dbApp.tblUsersProfiles.Where(p => p.uprPerfilPrincipal == true && p.userIdOwner == pag.userId).FirstOrDefault()
                         let nombre = (prof != null) ? prof.uprNombres.Trim() + " " + prof.uprApellidos.Trim() : ""
                         let conNombre = (pag.concepto=="Liga")?dbApp.tblLigas.Where(l=>l.ligId == pag.conceptoId).FirstOrDefault().ligNombreLiga: (pag.concepto == "Equipo" || pag.concepto == "Jugador") ?dbApp.tblEquipos.Where(e=> e.equId == pag.conceptoId).FirstOrDefault().equNombreEquipo: (pag.concepto=="Torneo")?dbApp.tblTorneos.Where(t=> t.torId == pag.conceptoId).FirstOrDefault().torNombreTorneo:""
                         select new PagosGridViewModel()
                         {
                             userId = pag.userId,
                             userName = nombre,
                             conceptoNombre = conNombre,
                             conceptoId = pag.conceptoId,
                             total = "$ " + dPag.total+ " MXN",
                             estado = "Pagado" ,
                             fechaPago = dPag.FechaCreacionUTC.ToString()
                         }
                        ).Distinct().ToList();

            return pagos;
        }
        public List<PagosGridViewModel> getGridPagosTorneos(string userId)
        {
            var pendientes = (from tor in dbApp.tblTorneos
                           join tca in dbApp.tblTorneoCoAdministradores on tor.torId equals tca.torId into tor_tca
                           from tca in tor_tca.DefaultIfEmpty()
                           where (tor.torUserIdCreador == userId
                               || tca.userCorreo == dbApp.Users.Where(l=> l.Id == userId).FirstOrDefault().Email ) && tor.torPagado == false
                           select tor)
                                .Distinct()
                                .ToList();

            var pagos = (from pag in dbApp.tblPagos
                         join pagDet in dbApp.tblDetallesPago on pag.IdTransaccion equals pagDet.IdTransaccion
                         join tor in dbApp.tblTorneos on pag.conceptoId equals tor.torId
                         join tca in dbApp.tblTorneoCoAdministradores on tor.torId equals tca.torId into tor_tca
                         from tca in tor_tca.DefaultIfEmpty()
                         where pag.concepto == "Torneo" &&
                               (tor.torUserIdCreador == userId
                               || tca.userCorreo == dbApp.Users.Where(l=> l.Id == userId).FirstOrDefault().Email ) && tor.torPagado == true
                         select new PagosGridViewModel()
                         {
                             userId = tor.torUserIdCreador,
                             userName = tor.tblUserCreador.UserName,
                             conceptoNombre = tor.torNombreTorneo,
                             conceptoId = tor.torId,
                             total = "$ " + tor.torPrecioTorneo+ " MXN",
                             estado = (tor.torPagado != false) ? "Pagado" : "Pendiente",
                             fechaPago = (tor.torPagado != false)? pagDet.FechaCreacionUTC.ToString() : "---"
                         }
                         ).Distinct().ToList();

            /*var pendientes = (from tor in torneos
                              where tor.torPagado == false
                              && tor.torUserIdCreador == userId
                              select tor).ToList();
                              */
            foreach (var item in pendientes)
            {
                var aux = new PagosGridViewModel();
                aux.userId = item.torUserIdCreador;
                aux.userName = item.tblUserCreador.UserName;
                aux.conceptoNombre = item.torNombreTorneo;
                aux.conceptoId = item.torId;
                aux.total = "$ " + item.torPrecioTorneo + " MXN";
                aux.estado = "Pendiente";
                pagos.Add(aux);
            }

            return pagos;
        }
        public List<PagosGridViewModel> getGridPagosEquipo()
        {
            var pagos = (from pag in dbApp.tblPagos
                         join pagDet in dbApp.tblDetallesPago on pag.IdTransaccion equals pagDet.IdTransaccion
                         join equ in dbApp.tblEquipos on pag.conceptoId equals equ.equId
                         where pag.concepto == "Equipo"
                         select new PagosGridViewModel()
                         {
                             userId = equ.equUserIdCreador,
                             userName = equ.equAdminCorreo,
                             conceptoNombre = equ.equNombreEquipo,
                             conceptoId = equ.equId,
                             total = "$ " + equ.equPrecioTorneo + " MXN",
                             estado = (equ.equPagado != false)? "Pagado":"Pendiente",
                             fechaPago = (equ.equPagado != false) ? pagDet.FechaCreacionUTC.ToString() : "---"
                         }
                         ).Distinct().ToList();
             var pendientes = (from equ in dbApp.tblEquipos
                              where equ.equPagado == false
                              select equ).ToList();

            foreach (var item in pendientes)
            {
                var aux = new PagosGridViewModel();
                aux.userId = item.equUserIdCreador;
                aux.userName = item.equAdminCorreo;
                aux.conceptoNombre = item.equNombreEquipo;
                aux.conceptoId = item.equId;
                aux.total = "$ " + item.equPrecioTorneo + " MXN";
                aux.estado = "Pendiente";
                pagos.Add(aux);
            }

            return pagos;
        }
       
        public List<PagosGridViewModel> getGridPagosJugador()
        {
            var pagos = (from pag in dbApp.tblPagos
                         join pagDet in dbApp.tblDetallesPago on pag.IdTransaccion equals pagDet.IdTransaccion
                         join jug in dbApp.tblJugadorEquipos on pag.conceptoId equals jug.equId
                         let jugNombre=dbApp.tblJugadores.Where(l=> l.jugUserId==jug.jugUserId).FirstOrDefault().jugNombre
                         where pag.concepto == "Jugador" && jug.jugPagado == true && jug.jugConfirmado == true
                         select new PagosGridViewModel()
                         {
                             userId = jug.jugUserId,
                             userName = jugNombre,
                             conceptoNombre = jug.tblEquipos.equNombreEquipo,
                             conceptoId = (int)jug.equId,
                             total = "$ " + jug.tblEquipos.equPrecioTorneo + " MXN",
                             estado = "Pagado" ,
                             fechaPago = pagDet.FechaCreacionUTC.ToString()
                         }
                         ).Distinct().ToList();
            var pendientes = (from jug in dbApp.tblJugadorEquipos
                              join jugador in dbApp.tblJugadores on jug.jugUserId equals jugador.jugUserId
                              where jug.jugPagado == false && jug.jugConfirmado == true
                              select new JugadorEquiposModel
                              {
                                  jugUserId = jug.jugUserId,
                                  jugCorreo = jugador.jugCorreo,
                                  jugNombre = jugador.jugNombre,
                                  jugConfirmado = jug.jugConfirmado,
                                  jugCodigoConfirmacion = jug.jugCodigoConfirmacion,
                                  jugEstatus = jug.jugEstatus,
                                  jugPrecioTorneo = jug.jugPrecioTorneo,
                                  equId=(int)jug.equId,
                                  tblEquipos = dbApp.tblEquipos.Where(l => l.equId == jug.equId).FirstOrDefault()
                              }).ToList();

            foreach (var item in pendientes)
            {
                var aux = new PagosGridViewModel();
                aux.userId = item.jugUserId;
                aux.userName = item.jugNombre;
                aux.conceptoNombre = (item.tblEquipos!=null)? item.tblEquipos.equNombreEquipo : "";
                aux.conceptoId = (item.equId!=null)? (int)item.equId:0;
                aux.total = (item.tblEquipos != null) ? "$ " + item.tblEquipos.equPrecioTorneo.ToString() + " MXN" : "";
                aux.estado = "Pendiente";
                pagos.Add(aux);
            }

            return pagos;
        }
        public List<PaymentsViewModel> getUltimosPagos()
         {
             return (from detPag in dbApp.tblDetallesPago
                     join pag in dbApp.tblPagos on detPag.IdPago equals pag.IdPago
                     join lig in dbApp.tblLigas on pag.conceptoId equals lig.ligId
                     join user in dbApp.tblUsersProfiles on pag.userId equals user.userIdOwner

                     select new PaymentsViewModel() {
                         ligNombreLiga = lig.ligNombreLiga,
                         payId=pag.IdPago,
                         payNombre=user.uprNombres+" "+user.uprApellidos,
                         payFecha= pag.pagoFechaCreacionUTC,
                         ligId=lig.ligId
                     }
                     ).ToList();
         }
         public List<DetallePagoViewModel> getDetallesPagos(int ligId) {
             var pagos = (from pag in dbApp.tblPagos
                          join lig in dbApp.tblLigas on pag.conceptoId equals lig.ligId
                          join detPag in dbApp.tblDetallesPago on pag.IdPago equals detPag.IdPago
                          join user in dbApp.tblUsersProfiles on pag.userId equals user.userIdOwner
                          where lig.ligId == ligId &&
                             pag.IdTransaccion == detPag.IdTransaccion &&
                             pag.concepto == "Liga"
                          select new DetallePagoViewModel()
                          {
                             IdPago = pag.IdPago,
                             conceptoPago = detPag.conceptoPago,
                             total = detPag.total,
                             IdTransaccion = pag.IdTransaccion,
                             referencia = detPag.referencia,
                             metodoPago = detPag.metodoPago,
                             status = detPag.status,
                             ipAddress = detPag.ipAddress,
                             concepto = lig.ligNombreLiga,
                             fechaPago = detPag.FechaCreacionUTC.ToString(),
                             userNombre = user.uprNombres + " " + user.uprApellidos
                          }).ToList();
             return pagos;
         }
        public List<DetallePagoViewModel> getDetallesTorneo(int torId)
        {
            var pagos = (from pag in dbApp.tblPagos
                          join tor in dbApp.tblTorneos on pag.conceptoId equals tor.torId
                          join tca in dbApp.tblTorneoCoAdministradores on tor.torId equals tca.torId into tor_tca
                          from tca in tor_tca.DefaultIfEmpty()
                          join detPag in dbApp.tblDetallesPago on pag.IdPago equals detPag.IdPago
                          join user in dbApp.tblUsersProfiles on pag.userId equals user.userIdOwner
                          where tor.torId  == torId &&
                            pag.IdTransaccion == detPag.IdTransaccion &&
                            pag.concepto == "Torneo"
                         select new DetallePagoViewModel()
                         {
                             IdPago = pag.IdPago,
                             conceptoPago = detPag.conceptoPago,
                             total = detPag.total,
                             IdTransaccion = pag.IdTransaccion,
                             referencia = detPag.referencia,
                             metodoPago = detPag.metodoPago,
                             status = detPag.status,
                             ipAddress = detPag.ipAddress,
                             concepto = tor.torNombreTorneo,
                             fechaPago = detPag.FechaCreacionUTC.ToString(),
                             userNombre = user.uprNombres + " " + user.uprApellidos
                         }).ToList();
            return pagos;
        }
        public List<DetallePagoViewModel> getDetallesPagosEquipos(int equId)
        {
            var pagos = (from pag in dbApp.tblPagos
                         join equi in dbApp.tblEquipos on pag.conceptoId equals equi.equId
                         join detPag in dbApp.tblDetallesPago on pag.IdPago equals detPag.IdPago
                         join user in dbApp.tblUsersProfiles on pag.userId equals user.userIdOwner
                         where equi.equId == equId &&
                            pag.IdTransaccion == detPag.IdTransaccion &&
                            pag.concepto == "Equipo"
                         select new DetallePagoViewModel()
                         {
                             IdPago = pag.IdPago,
                             conceptoPago = detPag.conceptoPago,
                             total = detPag.total,
                             IdTransaccion = pag.IdTransaccion,
                             referencia = detPag.referencia,
                             metodoPago = detPag.metodoPago,
                             status = detPag.status,
                             ipAddress = detPag.ipAddress,
                             conceptoNombre = equi.equNombreEquipo,
                             fechaPago = detPag.FechaCreacionUTC.ToString(),
                             userNombre = user.uprNombres + " " + user.uprApellidos
                         }).ToList();
            return pagos;
        }
        
        public List<DetallePagoViewModel> getDetallesPagosEquiposJug(int equId)
        {
            var pagos = (from pag in dbApp.tblPagos
                         join equi in dbApp.tblEquipos on pag.conceptoId equals equi.equId
                         join detPag in dbApp.tblDetallesPago on pag.IdPago equals detPag.IdPago
                         join user in dbApp.tblUsersProfiles on pag.userId equals user.userIdOwner
                         where equi.equId == equId &&
                            pag.IdTransaccion == detPag.IdTransaccion &&
                            pag.concepto == "Jugador"
                         select new DetallePagoViewModel()
                         {
                             IdPago = pag.IdPago,
                             conceptoPago = detPag.conceptoPago,
                             total = detPag.total,
                             IdTransaccion = pag.IdTransaccion,
                             referencia = detPag.referencia,
                             metodoPago = detPag.metodoPago,
                             status = detPag.status,
                             ipAddress = detPag.ipAddress,
                             conceptoNombre = equi.equNombreEquipo,
                             fechaPago = detPag.FechaCreacionUTC.ToString(),
                             userNombre = user.uprNombres + " " + user.uprApellidos
                         }).ToList();
            return pagos;
        }
        public int setDetallePago(DetallePagoViewModel model)
         {
             try
             {
                 // model.
                 schemaDetallesPago detPago = new schemaDetallesPago();
                 detPago.IdPago = model.IdPago;
                 detPago.IdTransaccion = model.IdTransaccion;
                 detPago.ipAddress = model.ipAddress;
                 detPago.metodoPago = model.metodoPago;
                 detPago.referencia = model.referencia;
                 detPago.status = model.status;
                 detPago.total = model.total;
                detPago.conceptoPago = model.conceptoPago;
                 dbApp.tblDetallesPago.Add(detPago);
                 dbApp.SaveChanges();

                 return detPago.Id;
             }
             catch (Exception ex)
             {
                 
                Global_Functions.saveErrors(ex.ToString(), false);
                return 0;
                throw;
             }

         }
         public int setPago(string userId,int ConceptoId, string transaccion,string Concepto)
         {
             try
             {
                 schemaPagos model = new schemaPagos();
                 model.userId=userId;
                 model.concepto = Concepto;
                 model.conceptoId= ConceptoId;
                 model.IdTransaccion = transaccion; 
                 dbApp.tblPagos.Add(model);
                 dbApp.SaveChanges();                
                 return model.IdPago;
             }
             catch (Exception ex)
             {
                 
                 Global_Functions.saveErrors(ex.ToString(), false);
                 return 0;
                 throw;
             }

         }
        public bool setLigaPago(int ligId,string status)
         {
            try
            {
                var liga = getLigaById(ligId);

                 if (liga!=null)
                 {
                     liga.statusPago = status;
                     dbApp.SaveChanges();
                     return true;
                 }
                 return false;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
             
         }

        public bool setTorneoPago(int torId, string status)
        {
            try
            {
                var torneo = getTorneoById(torId);

                if (torneo != null)
                {
                    torneo.torPagado = (status == "paid") ? true:false ;

                    dbApp.SaveChanges();
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
        public bool setEquipoPago(int equId,string userId ,bool status)
        {
            try
            {
                var equipo = getEquipoById(equId);

                if (equipo != null)
                {
                    if (userId!="")
                    {
                        equipo.equUserIdCreador = userId;
                        equipo.equEstatus = true;
                    }                    
                    equipo.equPagado = status;
                    
                    dbApp.SaveChanges();
                    return true;
                }else
                    return false;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }

        public String generator_Pass()
        {
            int lengthOfPassword = 10;
            string passCode = DateTime.Now.Ticks.ToString();
            string pass = BitConverter.ToString(new System.Security.Cryptography.SHA512CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(passCode))).Replace("-", String.Empty);
            var password = pass.Substring(0, lengthOfPassword);
            return password;
        }
         
        public bool setReportBug(schemaReporteBugs report)
        {
            try
            {
                var model = new schemaReporteBugs();
                model.correoUsuario = report.correoUsuario;
                model.reporte = report.reporte;
                model.ipAddress = report.ipAddress;
                model.browser = report.browser;
                dbApp.tblReporteBugs.Add(report);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }
        public bool setLoginHistory(schemaLoginHistory login)
        {
            try
            {
                var model = new schemaLoginHistory();
                model.correoUsuario = login.correoUsuario;
                model.exception = login.exception;
                model.ipAddress = login.ipAddress;

                dbApp.tblLoginHistory.Add(model);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }

        public bool setNewSubPerfil(string adminId, string userId)
        {
            try
            {
                var model = new schemaUsersMisCuentas();
                model.userIdAdmin = adminId;
                model.userId = userId;
                model.umcCuentaAdministrada = true;
                model.umcCuentasFusionadas = false;
                model.activo = true;
                dbApp.tblUsersMisCuentas.Add(model);
                dbApp.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }
        public bool set_deleteMisCuentas(string userId, string userAdmin)
        {
            try
            {
                var user_count = dbApp.tblUsersMisCuentas.Where(l => l.userId.ToUpper() == userId.ToUpper() && l.userIdAdmin == userAdmin ).First();
                //user.activo = false;
                if (user_count != null)
                {
                    dbApp.tblUsersMisCuentas.Remove(user_count);
                }                
                dbApp.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                Global_Functions.saveErrors(ex.ToString(), false);
                return false;
                throw;
            }
        }
        public List<ViewModelAvisos> getAvisos(string userId,string rolId)
        {
            //var rol = getRoles().Where(l => l.rolId.ToUpper() == rolId.ToUpper()).FirstOrDefault();
            var usr = getUserById(userId);
            var roles = getUserRoles(usr);
            var aviso = new List<ViewModelAvisos>();
            if (roles != null && roles.Count > 0)
            {
                foreach (var item in roles)
                {
                    var new_aviso = new ViewModelAvisos();
                    if (item.rolName == constClass.rolPlayer)
                    {
                        aviso.AddRange(getAvisoPlayer(userId));
                    
                        var league = getLeagueGridStatus().Where(l => l.ligCreadorId.ToUpper() == userId.ToUpper()).ToList();
                        if (league != null && league.Count > 0)
                        {
                            foreach (var lig in league)
                            {
                                new_aviso.aviso = "La Liga " + lig.ligNombreLiga + " esta en " + lig.viewStatus;
                                new_aviso.fecha_Registro = lig.ligFechaRegistro;
                                aviso.Add(new_aviso);
                            }

                        }
                    }                    
                }                
            }
         
            return aviso;
        }
        public List<ViewModelAvisos> getAvisoPlayer(string userId)
        {
            var aviso = (from equ in dbApp.tblEquipos
                         join jug in dbApp.tblJugadorEquipos on equ.equId equals jug.equId
                         where jug.jugUserId == userId && jug.jugPagado == false && jug.jugConfirmado == false
                         select new ViewModelAvisos()
                         {
                             aviso = "Invitacion al Equipo " + equ.equNombreEquipo.ToUpper() + " revise su correo electronico",
                             fecha_Registro = jug.jugFechaCreacionUTC
                         }).ToList(); 
            return aviso;
        }

        public List<ViewModelAvisos> getAvisoReferee(string userId)
        {
            var aviso = (from lig in dbApp.tblLigas
                         join arbL in dbApp.tblArbitrosLigas on lig.ligId equals arbL.ligId
                         join arb in dbApp.tblArbitros on arbL.arbId equals arb.arbId
                         where arb.arbUserId == userId && arbL.arbConfirmado == false
                         select new ViewModelAvisos()
                         {
                             aviso = "Invitacion ala Liga " + lig.ligNombreLiga.ToUpper() + " revise su correo electronico",
                             fecha_Registro = arb.arbFechaCreacionUTC
                         }).ToList();
            return aviso;
        }

        public List<schemaUsersMisCuentas> getMisCuentas(string userAdminId)
        {
            var model = dbApp.tblUsersMisCuentas.Where(l => l.userIdAdmin.Equals(userAdminId)).ToList();

            return model;
        }
        public List<schemaCountry> getCountry()
        {
            return dbApp.tblCountry.ToList();
        }

        public bool set_eliminarEstadistica(string idJugador, int idEquipo, int idPartido)
        {
            try
            {
                var estadistica = dbApp.tblJuegosFutbolEstadisticasJugador.Where(s => s.parId == idPartido && s.equId == idEquipo && s.UserIdJugador == idJugador).FirstOrDefault();
                if (estadistica != null)
                {
                    dbApp.tblJuegosFutbolEstadisticasJugador.Remove(estadistica);
                    dbApp.SaveChanges();
                    return true;
                }
                return false;
            }
            catch(Exception)
            {
                return false;
            }
        }
        /* public List<schemaDatosTarjeta> getDatosTarjeta(ApplicationUser userId)
         {
             return (from dtar in dbApp.tblDatosTarjeta
                     where dtar.userId == userId.Id
                     select dtar
                     ).ToList();
         }*/
    }
}