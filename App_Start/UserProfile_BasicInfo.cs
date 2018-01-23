using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace sw_EnligateWeb.App_Start
{
    public class UserProfile_BasicInfo : ActionFilterAttribute
    {

        /// <summary>
        /// Se llama despues de cada Acción.
        /// En esta funcion se obtienen los datos que se necesitan para realizar las validaciónes
        /// de los usuarios (datos de su perfil y sus roles).
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                DatabaseFunctions db = new DatabaseFunctions();

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var usr = db.getUserByUserName(HttpContext.Current.User.Identity.Name);
                    if (usr != null)
                    {
                        var prof = db.getUserMainProfile(usr.Id);
                        if (prof != null)
                            filterContext.Controller.ViewBag.afa_usrName = (prof.uprNombres.Trim() == "-") ? ((prof.uprApellidos.Trim() == "-") ? usr.UserName : prof.uprApellidos.Trim()) : prof.uprNombres.Trim();
                        else
                            filterContext.Controller.ViewBag.afa_usrName = usr.UserName;
                        filterContext.Controller.ViewBag.afa_profileComplete = true;

                        if (prof.cp == 0 || prof.uprFechaNacimiento == null || prof.uprCiudad == null || prof.uprEstado == null || prof.uprPais == null || prof.uprGenero == null )
                            filterContext.Controller.ViewBag.afa_profileComplete = false;
                               
                        filterContext.Controller.ViewBag.afa_usrEmailConfirmed = usr.EmailConfirmed;
                        filterContext.Controller.ViewBag.afa_RedirectToProfile = false;
                        if (!usr.EmailConfirmed)
                        {
                            string controller = filterContext.RouteData.Values["controller"].ToString();
                            string action = filterContext.RouteData.Values["action"].ToString();
                            if (controller == "Admin" && action != "Index" && action != "Profile")
                                filterContext.Controller.ViewBag.afa_RedirectToProfile = true;
                        }

                        var usrRoles = db.set_getUserCurrentRole(usr.UserName);
                        filterContext.Controller.ViewBag.afa_UserRolesNameCurrent = (string)usrRoles["currentUsrRoleName"];
                        filterContext.Controller.ViewBag.afa_UserRoles = new SelectList((List<SelectListItem>)usrRoles["rolesUser"], "Value", "Text", (string)usrRoles["currentUsrRoleId"]);

                        filterContext.Controller.ViewBag.afa_MenuRole = db.getMenusByRoleId((string)usrRoles["currentUsrRoleId"]);
                    }
                }

                filterContext.Controller.ViewBag.afa_DeportesActive = db.getDeportes_Active();
                filterContext.Controller.ViewBag.afa_LigaCategorias = db.getLigaCategorias_Active();
            }
            base.OnResultExecuting(filterContext);
        }

        
    }

}