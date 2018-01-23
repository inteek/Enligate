using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models;
using sw_EnligateWeb.Models.HelperClasses;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;

namespace sw_EnligateWeb.Controllers
{
    public class AccountController : Controller
    {
        DatabaseFunctions db = new DatabaseFunctions();

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        /// Cierra la sesión y regresa a la pagina de inicio
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        #region Login

        /// <summary>
        /// Accion que realiza el inicio de sesión desde la forma de login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _LoginForm(LoginViewModel model, string returnUrl)
        {
            model.scriptJS = "";
            if (!ModelState.IsValid)
            {
                return PartialView("Account/_LoginForm", model);
            }

            ApplicationUser usr = db.getUserByUserEmail(model.usuEmail);
            if (usr == null)
            {
                ModelState.AddModelError(constClass.error, "El usuario o la contraseña son incorrectas.");
                return PartialView("Account/_LoginForm", model);
            }

            if (!usr.usuEstatus)
            {
                ModelState.AddModelError(constClass.error, "Tu usuario esta deshabilitado, ponte en contacto con el administrador");
                return PartialView("Account/_LoginForm", model);
            }
            if (!usr.EmailConfirmed)
            {
                ModelState.AddModelError(constClass.info, "Tu cuenta no ha sido activada, por favor, revise su correo electronico");
                return PartialView("Account/_LoginForm", model);
            }
            //if (!usr.EmailConfirmed)
            //{
            //    if (sendConfirmEmailAccountCodeEmail(usr))
            //    {
            //        ModelState.Clear();
            //        ModelState.AddModelError(constClass.info, "Debes confirmar tu cuenta de correo. Hemos enviado un correo de confirmación a tu cuenta " + usr.Email);
            //        return PartialView("Account/_LoginForm", model);

            //    }
            //    string err = "El codigo de confirmación no se envió. Intenta nuevamente";
            //    Global_Functions.saveErrors(err, false);
            //    AddError(err);
            //    return PartialView("Account/_LoginForm", model);
            //}

            var result = Login(usr.Id, model.usuEmail, model.usuPassword, model.RememberMe);
            switch (result)
            {
                case SignInStatus.Success:
                    ModelState.AddModelError(constClass.success, "Iniciando sesión");
                    if (returnUrl != "")
                        model.scriptJS = "document.location.replace('" + returnUrl + "');";
                    else
                        model.scriptJS = "document.location.replace('" + Url.Action("Index", "Admin") + "');";
                    break;
                case SignInStatus.LockedOut:
                    ModelState.AddModelError(constClass.error, "Por seguridad la cuenta se bloqueo por intentos fallidos, intenta nuevamente en 5 min");
                    break;
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError(constClass.error, "El usuario o la contraseña son incorrectas.");
                    break;
            }

            if (returnUrl != null && returnUrl.Trim() != "")
                ViewBag.ReturnUrl = returnUrl;

            //model.scriptJS = HttpUtility.HtmlEncode(model.scriptJS);
            return PartialView("Account/_LoginForm", model);
        }

        /// <summary>
        /// Funcion que realiza el inicio de sesión
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="remember"></param>
        /// <returns></returns>
        protected SignInStatus Login(string userId, string email, string password, bool remember)
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            //AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = remember }); //, await user.GenerateUserIdentityAsync(UserManager));

            var loginHistory = new schemaLoginHistory();
            loginHistory.correoUsuario = email;
            loginHistory.ipAddress = Request.UserHostAddress;
            
            SignInStatus resultLogin = SignInManager.PasswordSignIn(email, password, remember, true);
            if (resultLogin == SignInStatus.Success)
            {
                loginHistory.exception = resultLogin.ToString();
                UserManager.ResetAccessFailedCount(userId);
                //Registrar en el log (AspNetUserLogins)
            }else
                loginHistory.exception = resultLogin.ToString();
            db.setLoginHistory(loginHistory);
            Global_Functions.saveLoginHistory(email+" "+loginHistory.ipAddress+" "+ resultLogin);
            return resultLogin;
        }

        /// <summary>
        /// Funcion que valida si hay que redirigir  a alguna pantalla
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        protected object hasReturnUrl(string returnUrl)
        {
            if (returnUrl == String.Empty || returnUrl == "" || returnUrl == null)
                 return null;
            return new { returnUrl = returnUrl };
        }

        #endregion

        #region Register

        /// <summary>
        /// Acción que realiza el registro de un nuevo usuario desde la forma de login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _RegisterForm(RegisterViewModel model, string returnUrl)
        {
            model.scriptJS = "";
            
            if (ModelState.IsValid)
            {
                ApplicationUser usr = db.getUserByUserEmail(model.usuEmail);
                if (usr != null)
                {
                    ModelState.AddModelError(constClass.info, "El correo electrónico ya está registrado. Inicia sesión o recupera la contraseña.");
                    return PartialView("Account/_RegisterForm", model);
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.usuEmail,
                    Email = model.usuEmail,
                    PhoneNumber = "",
                    EmailConfirmed = false,
                    usuEstatus = true,
                    created_at = db.DateTimeMX()
                };

                var result = UserManager.Create(user, model.usuPassword);
                if (result != IdentityResult.Success)
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                    Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
                }
                else
                {
                    
                    var arb = db.getArbitroByEmail(model.usuEmail);
                    var prof = new schemaUsersProfiles();
                    prof.uprNombres = "-";
                    prof.uprApellidos = "-";
                    prof.uprTelefono = "";
                    db.setUserProfileMain_UpdateInsert(user, prof);
                    if (arb.Count > 0)
                    {
                        result = UserManager.AddToRole(user.Id, constClass.rolReferee);
                        db.setArbitroUserId(user.Id,user.Email);
                        var rol = db.getRoleByName(constClass.rolReferee);
                        db.setCurrentUserRole(user.Id, rol.Id);
                    }
                    else{
                        result = UserManager.AddToRole(user.Id, constClass.rolPlayer);
                        var rol = db.getRoleByName(constClass.rolPlayer);
                        db.setCurrentUserRole(user.Id,rol.Id);
                    }

                    if (result != IdentityResult.Success)
                    {
                        UserManager.Delete(user);
                        ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                        Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
                    }
                    else
                    {
                        //user = db.getUserById(user.Id);
                        string mailSended = "";
                        if (sendConfirmEmailAccountCodeEmail(user,returnUrl))
                        {
                            string rand = Engine.Global_Functions.getRandomString(10);

                            ModelState.Clear();
                            mailSended = "Hemos enviado un correo de confirmación a tu cuenta " + user.Email;
                            ModelState.AddModelError(constClass.success, "Usuario registrado, " + mailSended);

                            model.scriptJS = @"function jsRedirect_RegisterForm" + rand + @"(){
                                                    document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                                 @"}
                                                   setTimeout(jsRedirect_RegisterForm" + rand + ",2500);";
                            returnUrl = "";
                        }

                       // var res = Login(user.Id, model.usuEmail, model.usuPassword, false);
                       /*
                        switch (res)
                        {
                            case SignInStatus.Success:
                                ModelState.AddModelError(constClass.success, "Usuario registrado, iniciando sesión. " + mailSended);
                                string rand = Engine.Global_Functions.getRandomString(10);
                                ModelState.AddModelError(constClass.success, "Iniciando sesión");
                                if (returnUrl != null)
                                    model.scriptJS = @"function jsRedirect_RegisterForm" + rand + @"(){
                                                    document.location.replace('" + returnUrl + "');" +
                                                 @"}
                                                   setTimeout(jsRedirect_RegisterForm" + rand + ",2500);";
                                else
                                    model.scriptJS = @"function jsRedirect_RegisterForm" + rand + @"(){
                                                        document.location.replace('" + Url.Action("Index", "Admin") + "');" +
                                                     @"}
                                                       setTimeout(jsRedirect_RegisterForm" + rand + ",2500);";
                                break;
                            case SignInStatus.LockedOut:
                                ModelState.AddModelError(constClass.error, "Por seguridad la cuenta se bloqueo por intentos fallidos, intenta nuevamente en 5 min");
                                break;
                            //case SignInStatus.RequiresVerification:
                            //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                            case SignInStatus.Failure:
                            default:
                                ModelState.AddModelError(constClass.error, "El usuario o la contraseña son incorrectas.");
                                break;
                        }
                        */
                    }
                }
            }
            if (returnUrl != null && returnUrl.Trim() != "")
                ViewBag.ReturnUrl = returnUrl;

            return PartialView("Account/_RegisterForm", model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public bool _RegisterNewUser(RegisterViewModel model, string returnUrl)
        {
            model.scriptJS = "";

            if (ModelState.IsValid)
            {
                ApplicationUser usr = db.getUserByUserEmail(model.usuEmail);
                if (usr != null)
                {
                    ModelState.AddModelError(constClass.info, "El correo electrónico ya está registrado. Inicia sesión o recupera la contraseña.");
                    // return PartialView("Account/_RegisterForm", model);
                    return false;
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.usuEmail,
                    Email = model.usuEmail,
                    PhoneNumber = "",
                    EmailConfirmed = false,
                    usuEstatus = true
                };

                var result = UserManager.Create(user, model.usuPassword);
                if (result != IdentityResult.Success)
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                    Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
                    return false;
                }
                else
                {

                    var prof = new schemaUsersProfiles();
                    prof.uprNombres = "-";
                    prof.uprApellidos = "-";
                    prof.uprTelefono = "";
                    db.setUserProfileMain_UpdateInsert(user, prof);

                    result = UserManager.AddToRole(user.Id, constClass.rolPlayer);
                    var rol = db.getRoleByName(constClass.rolPlayer);
                    db.setCurrentUserRole(user.Id, rol.Id);
                

                    if (result != IdentityResult.Success)
                    {
                        UserManager.Delete(user);
                        ModelState.AddModelError(constClass.error, "Hubo un error al crear tu usuario, intentalo nuevamente.");
                        Global_Functions.saveErrors(String.Join(". ", result.Errors), false);
                        return false;
                    }
                    else
                    {
                        //user = db.getUserById(user.Id);
                        string mailSended = "";
                        if (sendConfirmEmailAccountCodeEmail(user, returnUrl))
                        {
                            ModelState.Clear();
                            mailSended = "Hemos enviado un correo de confirmación a tu cuenta " + user.Email;
                        }

                        var res = Login(user.Id, model.usuEmail, model.usuPassword, false);
                        switch (res)
                        {
                            case SignInStatus.Success:
                                ModelState.AddModelError(constClass.success, "Usuario registrado, iniciando sesión. " + mailSended);
                                string rand = Engine.Global_Functions.getRandomString(10);
                                ModelState.AddModelError(constClass.success, "Iniciando sesión");
                                return true;
                                if (returnUrl != null) 
                                    model.scriptJS = @"function jsRedirect_RegisterForm" + rand + @"(){
                                                    document.location.replace('" + returnUrl + "');" +
                                                 @"}
                                                   setTimeout(jsRedirect_RegisterForm" + rand + ",2500);";                                
                                else
                                    model.scriptJS = @"function jsRedirect_RegisterForm" + rand + @"(){
                                                        document.location.replace('" + Url.Action("Index", "Admin") + "');" +
                                                     @"}
                                                       setTimeout(jsRedirect_RegisterForm" + rand + ",2500);";
                                

                                break;
                            case SignInStatus.LockedOut:
                                ModelState.AddModelError(constClass.error, "Por seguridad la cuenta se bloqueo por intentos fallidos, intenta nuevamente en 5 min");
                                break;
                            //case SignInStatus.RequiresVerification:
                            //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                            case SignInStatus.Failure:
                            default:

                                ModelState.AddModelError(constClass.error, "El usuario o la contraseña son incorrectas.");
                                return false;
                                break;
                        }
                    }
                }
            }
            if (returnUrl != null && returnUrl.Trim() != "")
                ViewBag.ReturnUrl = returnUrl;
            return false;
            //return PartialView("Account/_RegisterForm", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public bool _RegisterNew(RegisterViewModel model, bool confirm=false)
        {
            var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
            ApplicationUserManager _userManager = new ApplicationUserManager(store);
            var manger = _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.usuEmail,
                Email = model.usuEmail,
                PhoneNumber = "",
                EmailConfirmed = confirm,
                usuEstatus = true
            };
            var usmanger = manger.Create(user, model.usuPassword);
            if (usmanger.Succeeded)
            {
                var profile = db.getUserMainProfile(user.Id);
                if (profile == null)
                {
                    var prof = new schemaUsersProfiles();
                    prof.uprNombres = (user.UserName != null) ? user.UserName : "-";
                    prof.uprApellidos = "-";
                    prof.uprTelefono = "";
                    var profileStatus = db.setUserProfileMain_UpdateInsert(user, prof);
                    if (profileStatus)
                    {
                        return true;
                    }
                }
                return true;
            }
            return true;
        }

        private Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            throw new NotImplementedException();
        }

        public bool _RegisterNewAccount(RegisterViewModel model, string password)
        {
            var user = new ApplicationUser
            {
                UserName = model.usuEmail,
                Email = model.usuEmail,
                PhoneNumber = "",
                EmailConfirmed = true,
                usuEstatus = true
            };

            using (var db = new ApplicationDbContext())
            {
                var store = new UserStore<ApplicationUser>(db);
                var manager = new UserManager<ApplicationUser, string>(store);

                var result = manager.Create(user, model.usuPassword);
                if (!result.Succeeded)
                    throw new ApplicationException("Unable to create a user.");

                result = manager.AddToRole(user.Id,constClass.rolPlayer);
                if (!result.Succeeded)
                {                    
                    throw new ApplicationException("Unable to add user to a role.");
                }
            }
            return true;
        }
        /// <summary>
        /// Funcion que envia el correo para confirmar el correo electronico del usuario
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool sendConfirmEmailAccountCodeEmail(ApplicationUser user, string returnUrl)
        {
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            string code = Global_Functions.getSha1(0,Global_Functions.generateCode());
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { email = user.UserName, code = code , ReturnUrl = returnUrl }, protocol: Request.Url.Scheme);
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            if (db.setUpdateEmailValidation(user, code))
            {
                var mainPerfil = db.getMainPerfilById(user.Id);
                var correo = "";
                if (mainPerfil.Count != 0)
                {
                    foreach (var item in mainPerfil)
                    {
                        correo = item.tblUserAdmin.Email;
                    }
                }
                else
                {
                    correo = user.Email;
                }

                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    string body = Global_Functions.getBodyHTML("~/Emails/confirmEmailAccount.html");
                    body = body.Replace("<%= UrlValidationCode %>", callbackUrl);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(correo, siteConfig.scoSenderDisplayEmailName, "Confirmar cuenta de correo", body,
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

        public ActionResult SubPerfilConfirmar(string email_admin, string email_sub, string name)
        {
            var redirectHome = false;
            var model = new RegisterViewModel();
            var admin = db.getUserByUserEmail(email_admin);
            var sub = db.getUserByUserEmail(email_sub);
            ViewBag.UsuarioNombre = sub.UserName;
            if (db.setNewSubPerfil(admin.Id, sub.Id))
            {
                var usuario = UserManager.FindById(sub.Id);
                SignInManager.SignIn(usuario, true, false);
                redirectHome = true;
            }
            string rand = Engine.Global_Functions.getRandomString(10);

            if (!redirectHome)
            {
                ViewBag.jsScript = @"function jsRedirect_ResetPasswordCode" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                   @"}
                                   setTimeout(jsRedirect_ResetPasswordCode" + rand + ",2500);";
            }else
            {
                ViewBag.jsScript = @"function jsRedirect_ResetPasswordCode" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Admin") + "');" +
                               @"}
                                   setTimeout(jsRedirect_ResetPasswordCode" + rand + ",2500);";
            }

            return View();
        }
        public bool enviarEmailParticipanteAviso(string admin,string correo, string password)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                var body = Global_Functions.getBodyHTML("~/Emails/NuevoSubPerfil.html");
                var emailTo = correo;
                
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
      //  public ActionResult SubPerfilRechazar(string email_admin, string email_sub)
       // {

//        }
        #endregion

        #region ConfirmEmail

        /// <summary>
        /// Accion que confirma la cuenta de correo, se acciona desde el correo de confirmación
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string email, string code, string ReturnUrl)
        {
            ViewBag.UsuarioNombre = "";
            bool redirectHome = false;
            bool sign = false;
            if (email == null || code == null)
            {
                ModelState.AddModelError(constClass.error, "Debes de especificar el usuario y el código. Redireccionando . . .");
                redirectHome = true;
            }
            else
            {
                ApplicationUser usr = db.getUserByUserName(email);
                if (usr == null)
                {
                    ModelState.AddModelError(constClass.error, "Usuario no encontrado. Redireccionando . . .");
                    redirectHome = true;
                }
                else if (usr.usuEmailValidationCode == null || usr.usuEmailValidationCode.Trim().ToLower() != code.Trim().ToLower())
                {
                    ModelState.AddModelError(constClass.error, "El codigo es incorrecto, favor de verificarlo. Redireccionando . . .");
                    redirectHome = true;
                }
                else if (usr.usuEmailValidationCodeEndDateUtc == null || usr.usuEmailValidationCodeEndDateUtc < db.DateTimeMX())
                {
                    ModelState.AddModelError(constClass.error, "El codigo ya vencio. Redireccionando . . .");
                    redirectHome = true;
                }
                else
                {
                    db.setClearEmailValidation(usr);

                    var prof = db.getUserMainProfile(usr.Id);
                    ViewBag.UsuarioNombre = (prof != null && prof.uprNombres.Trim() == "-") ? usr.UserName : prof.uprNombres.Trim();
                    SignInManager.SignIn(usr, false, false);
                    sign = true;
                }
            }
            string rand = Engine.Global_Functions.getRandomString(10);
            if (redirectHome)
            {
                    ViewBag.jsScript = @"function jsRedirect_ConfirmEmail" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                   @"}
                                   setTimeout(jsRedirect_ConfirmEmail" + rand + ",2500);";
                                
            }
            if (sign && ReturnUrl != "")
            {
                ViewBag.jsScript = @"function jsRedirect_ConfirmEmail" + rand + @"(){
                                        document.location.replace('" + ReturnUrl + "');" +
                               @"}
                                   setTimeout(jsRedirect_ConfirmEmail" + rand + ",2500);";
            }
            return View();

            //if (code != usr.usuEmailValidationCode && usr.EmailConfirmed == false)
            //{
            //    ModelState.Clear();
            //    if (sendConfirmEmailAccountCodeEmail(usr))
            //        ModelState.AddModelError(constClass.info, "El código de validación no es correcto. Se reenvió un nuevo código de validación a tu correo.");
            //    else
            //        ModelState.AddModelError(constClass.error, "El código de validación no es correcto. Ocurrió un error al enviarte un nuevo código de validación a tu correo. Intentalo nuevamente.");
            //    TempData["ViewData"] = ViewData;
            //    return RedirectToAction("Login");
            //}
        }

        /// <summary>
        /// Accion que reenvia el correo de confirmación
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ConfirmEmailForward()
        {
            string Id = User.Identity.GetUserId();
            ApplicationUser usr = db.getUserById(Id);
            if (usr == null)
            {
                ModelState.AddModelError(constClass.error, "Usuario no encontrado.");
            }
            else
            {
                if (sendConfirmEmailAccountCodeEmail(usr,""))
                {
                    ModelState.Clear();
                    ModelState.AddModelError(constClass.success, "Hemos reenviado un correo de confirmación a tu cuenta " + usr.Email);
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error al reenviar el correo de confirmación, intentalo nuevamente.");
                }
            }

            return PartialView("Account/_ConfirmEmailForward");
        }
        
       
        #endregion

        #region LoginExternal

        /// <summary>
        /// Redirecciona a la página del proveedor de credenciales. (Facebook,etc)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _LoginFormSocialNetworks(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("LoginExternalConfirmAsync", "Account", hasReturnUrl(returnUrl)));
        }
        
        /// <summary>
        /// Recibe el Callback con la información del proveedor de credenciales.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> LoginExternalConfirmAsync(string returnUrl = null)
        {
            ExternalLoginInfo loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
                return RedirectToAction("Index", "Home", hasReturnUrl(returnUrl));
            //Tries to log in and if it has success redirects to admin page.
            string logInUrl = await externalLoginSignInSync(loginInfo, returnUrl);
            if (logInUrl != null)
                return Redirect(logInUrl);
            
            //Is not registered in the database register claims in the database and try to loggin again
            var claimName = loginInfo.ExternalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var claimEmail = loginInfo.ExternalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            LoginExternalConfirmViewModel model = new LoginExternalConfirmViewModel();
            splitClaimName(model, claimName);
            model.usuEmail = claimEmail != null ? claimEmail.Value : String.Empty;
            model.usuEmailConfirmar = model.usuEmail;
            model.LoginProvider = loginInfo.Login.LoginProvider;

            if (await externalLoginCreateUser_Provider(model, loginInfo))
            {
                logInUrl = await externalLoginSignInSync(loginInfo, returnUrl);
                if (logInUrl != null)
                    return Redirect(logInUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Inicia la sesión del usuario cuando ya esta logeado.
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        protected async Task<string> externalLoginSignInSync(ExternalLoginInfo loginInfo, string returnUrl)
        {
            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                case SignInStatus.LockedOut:
                    string userId = db.getUserIdByProviderKey(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey);
                    var user = db.getUserById(userId);
                    if (user != null)
                    {
                        if (returnUrl == null)
                            return Url.Action("Index", "Admin", null);
                        return (returnUrl);
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// Divide el nombre del usuario que se obtiene del proveedor externo.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="claimName"></param>
        protected void splitClaimName(LoginExternalConfirmViewModel model, Claim claimName)
        {
            string names = claimName != null ? claimName.Value : string.Empty;
            if (names != string.Empty)
            {
                var split = names.Split(new char[] { ' ' });
                int counter = 0;
                foreach (string name in split)
                {
                    if (counter < 1)
                        model.usuNombre += " " + name;
                    else
                        model.usuApellido += " " + name;
                    counter++;
                }
            }
        }

        /// <summary>
        /// Crea el usuario si se logea si este no existe en la bd y e inicio sesion desde un proveedor externo
        /// </summary>
        /// <param name="model"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        protected async Task<bool> externalLoginCreateUser_Provider(LoginExternalConfirmViewModel model, ExternalLoginInfo loginInfo)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.usuEmail,
                Email = model.usuEmail,
                EmailConfirmed = true,
                usuEstatus = true
            };

            var prof = new schemaUsersProfiles();
            prof.uprNombres = model.usuNombre;
            prof.uprApellidos = model.usuApellido;

            ApplicationUser userChecked = db.getUserByUserEmail(model.usuEmail);
            if (userChecked == null)
            {
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    
                    db.setUserProfileMain_UpdateInsert(user, prof);

                    result = UserManager.AddToRole(user.Id, constClass.rolPlayer);
                    var rol = db.getRoleByName(constClass.rolPlayer);
                    db.setCurrentUserRole(user.Id, rol.Id);

                    result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                    if (result.Succeeded)
                    {
                        return true;
                        //result = await UserManager.AddToRoleAsync(user.Id, constClass.rolPlayer);
                        //if (result.Succeeded)
                        //{
                        //return true;
                        //}
                        //else
                        //{
                         //   Global_Functions.saveErrors(result.Errors.ToString(), false);
                         //   AddErrors(result);
                        //}
                    }
                    else
                    {
                        Global_Functions.saveErrors(result.Errors.ToString(), false);
                        AddErrors(result);
                    }
                }
                else
                {
                    Global_Functions.saveErrors(result.Errors.ToString(), false);
                    AddErrors(result);
                }
            }
            else
            {
                user.Id = userChecked.Id;
                //Inserta en la base de datos para que ese usuario pueda ingresar por ese proveedor con la misma cuenta de correo.
                var result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                if (result.Succeeded)
                {
                    db.setUpdateUserProfile_ConfirmEmail(user, prof);
                    return true;
                }
                else
                {
                    Global_Functions.saveErrors(result.Errors.ToString(), false);
                    AddErrors(result);
                }
            }

            return false;
        }

        #endregion

        #region PasswordRecovery

        /// <summary>
        /// Acción que valida los datos y envia el correo de recuperación de contraseña
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _PasswordRecoveryForm(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usr = db.getUserByUserEmail(model.usuEmail);
                if (usr == null)
                {
                    ModelState.AddModelError(constClass.info,"Usuario no encontrado");
                }
                else if (!usr.usuEstatus)
                {
                    ModelState.AddModelError(constClass.info, "Tu usuario esta deshabilitado, ponte en contacto con el administrador");
                }
                else if (!sendPasswordRecoveryEmail(usr))
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error al enviar el correo de recuperación. Intentalo nuevamente");
                }
                else
                {
                    ModelState.Clear();
                    model = new ForgotPasswordViewModel();
                    model.scriptJS = "$('#modalPassRecovery').modal('hide');";
                    ModelState.AddModelError(constClass.success, "Se ha enviado a tu correo el codigo de validación para la recuperación de la contraseña.");
                }
            }

            return PartialView("Account/_PasswordRecoveryForm", model);
        }

        /// <summary>
        /// Acción que valida el codigo de recuperación de contraseña y correo para
        /// mostrar la forma de recuperación de contraseña
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordCode(string email, string code)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            ViewBag.UsuarioNombre = "";

            bool redirectHome = false;

            if (email == null || code == null)
            {
                ModelState.AddModelError(constClass.error, "Debes de especificar el usuario y el código. Redireccionando . . .");
                redirectHome = true;
            }
            else
            {
                ApplicationUser usr = db.getUserByUserEmail(email);
                if (usr == null)
                {
                    ModelState.AddModelError(constClass.error, "Debes de especificar el usuario y el código. Redireccionando . . .");
                    redirectHome = true;
                }
                else if (usr.usuPasswordRecoveryCode == null || usr.usuPasswordRecoveryCode.Trim() != code.Trim())
                {
                    ModelState.AddModelError(constClass.error, "El codigo es incorrecto, favor de verificarlo. Redireccionando . . .");
                    redirectHome = true;
                }
                else if (usr.usuPasswordRecoveryCodeEndDateUtc == null || usr.usuPasswordRecoveryCodeEndDateUtc < db.DateTimeMX())
                {
                    ModelState.AddModelError(constClass.error, "El codigo ya vencio. Redireccionando . . .");
                    redirectHome = true;
                }
                else
                {
                    model.Code = code;
                    model.userId = usr.Id;
                    model.usuEmail = usr.Email;

                    var prof = db.getUserMainProfile(usr.Id);
                    ViewBag.UsuarioNombre = (prof != null && prof.uprNombres.Trim() == "-") ? usr.UserName : prof.uprNombres.Trim();
                }
            }

            if (redirectHome)
            {
                string rand = Engine.Global_Functions.getRandomString(10);
                ViewBag.jsScript = @"function jsRedirect_ResetPasswordCode" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                   @"}
                                   setTimeout(jsRedirect_ResetPasswordCode" + rand + ",2500);";
            }

            return View(model);
        }

        /// <summary>
        /// Acción que realiza el cambio de contraseña
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _ResetPasswordCodeForm(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usr = db.getUserByUserEmail(model.usuEmail);
                if (usr == null)
                {
                    ModelState.AddModelError(constClass.error, "Usuario no encontrado. Redireccionando . . .");
                }
                else if (usr.usuPasswordRecoveryCode == null || usr.usuPasswordRecoveryCode == "")
                {
                    ModelState.AddModelError(constClass.error, "No hay código que validar. Redireccionando . . .");
                }
                else if (!usr.usuPasswordRecoveryCode.Trim().ToLower().Equals(model.Code.Trim().ToLower()))
                {
                    ModelState.AddModelError(constClass.error, "El código de seguridad no coincide, verificalo en tu correo. Redireccionando . . .");
                }
                else if ((DateTime)usr.usuPasswordRecoveryCodeEndDateUtc < db.DateTimeMX())
                {
                    ModelState.AddModelError(constClass.error, "El código ya venció, solicita al administrador que te envie uno nuevo. Redireccionando . . .");
                }
                else
                {
                    //Confirma correo y cambia contraseña
                    string passToken = UserManager.GeneratePasswordResetToken(usr.Id);
                    IdentityResult result = UserManager.ResetPassword(usr.Id, passToken, model.usuPassword);
                    if (result.Succeeded)
                    {
                        db.setClearPasswordRecoveryCode(usr);

                        ModelState.Clear();
                        ModelState.AddModelError(constClass.success, "Tu contraseña ha sido cambiada. Redireccionando . . .");

                        //Realiza el login automatico.
                       //var status = Login(model.userId, model.usuEmail, model.usuPassword, false);

                        var usuario = UserManager.FindById(model.userId);
                        SignInManager.SignIn(usuario, true, false);
                        model = new ResetPasswordViewModel();
                        string rand = Engine.Global_Functions.getRandomString(10);
                        model.scriptJS = @"function jsRedirect_ResetPasswordCodeForm" + rand + @"(){
                                            document.location.replace('" + Url.Action("Index", "Admin") + "');" +
                                         @"}
                                         setTimeout(jsRedirect_ResetPasswordCodeForm" + rand + ",2500);";
                    }
                    else
                        ModelState.AddModelError(constClass.error, "Hubo un error al cambiar la contraseña, intenta nuevamente. " + string.Join("", result.Errors));
                }
            }

            return PartialView("Account/_ResetPasswordCodeForm", model);
        }

        /// <summary>
        /// Funcion que envia el correo de recuperación de contraseña
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

        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        private object aspNetUsers;

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
                // this line fixed the problem with returing null
                context.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;

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
    }
}