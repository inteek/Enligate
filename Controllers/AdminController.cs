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

using NUnit;
using System.Web.Script.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;
namespace sw_EnligateWeb.Controllers
{
    public class AdminController : MyBaseController
    {
        DatabaseFunctions db = new DatabaseFunctions();

        static ChatModel chatModel;

        /// <summary>
        /// When the method is called with no arguments, just return the view
        /// When argument logOn is true, a user logged on
        /// When argument logOff is true, a user closed their browser or navigated away (log off)
        /// When argument chatMessage is specified, the user typed something in the chat
        /// </summary>
        public ActionResult ChatEnligate(string user, bool? logOn, string logOff, string chatMessage)
        {
            try
            {
                if (chatModel == null) chatModel = new ChatModel();

                //trim chat history if needed
                if (chatModel.ChatHistory.Count > 100)
                    chatModel.ChatHistory.RemoveRange(0, 90);

                if (!Request.IsAjaxRequest())
                {
                    //first time loading
                    return View(chatModel);
                }
                else if (logOn != null && (bool)logOn)
                {
                    //check if nickname already exists
                    if (chatModel.Users.FirstOrDefault(u => u.NickName == user) != null)
                    {
                        throw new Exception("This nickname already exists");
                    }
                    else if (chatModel.Users.Count > 10)
                    {
                        throw new Exception("The room is full!");
                    }
                    else
                    {
                        #region create new user and add to lobby
                        chatModel.Users.Add(new ChatModel.ChatUser()
                        {
                            NickName = user,
                            LoggedOnTime = db.DateTimeMX(),
                            LastPing = db.DateTimeMX()
                        });

                        //inform lobby of new user
                        chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
                        {
                            Message = "User '" + user + "' logged on.",
                            When = db.DateTimeMX()
                        });
                        #endregion

                    }

                    return PartialView("Lobby", chatModel);
                }
                else if (logOff != null && logOff != "")
                {
                    LogOffUser(chatModel.Users.FirstOrDefault(u => u.NickName == user));
                    return PartialView("Lobby", chatModel);
                }
                else
                {

                    ChatModel.ChatUser currentUser = chatModel.Users.FirstOrDefault(u => u.NickName == user);

                    //remember each user's last ping time
                    currentUser.LastPing = db.DateTimeMX();

                    #region remove inactive users
                    List<ChatModel.ChatUser> removeThese = new List<ChatModel.ChatUser>();
                    foreach (Models.ChatModel.ChatUser usr in chatModel.Users)
                    {
                        TimeSpan span = db.DateTimeMX() - usr.LastPing;
                        if (span.TotalSeconds > 15)
                            removeThese.Add(usr);
                    }
                    foreach (ChatModel.ChatUser usr in removeThese)
                    {
                        LogOffUser(usr);
                    }
                    #endregion

                    #region if there is a new message, append it to the chat
                    if (!string.IsNullOrEmpty(chatMessage))
                    {
                        chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
                        {
                            ByUser = currentUser,
                            Message = chatMessage,
                            When = db.DateTimeMX()
                        });
                    }
                    #endregion

                    return PartialView("ChatHistory", chatModel);
                }
            }
            catch (Exception ex)
            {
                //return error to AJAX function
                Response.StatusCode = 500;
                return Content(ex.Message);
            }
        }


        /// <summary>
        /// Remove this user from the lobby and inform others that he logged off
        /// </summary>
        /// <param name="user"></param>
        public void LogOffUser(ChatModel.ChatUser user)
        {
            chatModel.Users.Remove(user);
            chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
            {
                Message = "User '" + user.NickName + "' logged off.",
                When = db.DateTimeMX()
            });
        }

        public void crearMenus()
        {
            string[] menuArr = new string[]{     
                //--Jugador
                "Dashboard", "Dashboard", "Admin","0", constClass.rolPlayer,
                "Calendario", "Calendario", "Admin","1", constClass.rolPlayer,
                "Historial", "Historial", "Admin","2", constClass.rolPlayer,

                //--Admin de Enligate - Dueño de enligate
                "Dashboard", "Index", "Administrator","3", constClass.rolAdmin,
                "Ligas Inscritas", "Leagues", "Administrator","4", constClass.rolAdmin,
                //"Publicidad", "Advertising", "Administrator","5", constClass.rolAdmin,
                "Solicitudes", "Requests", "Administrator","6", constClass.rolAdmin,
                "Pagos", "Fees", "Administrator","7", constClass.rolAdmin,

                //--Administrador de Ligas
                "Dashboard", "Index", "AdminLigas","8", constClass.rolOwners,
                "Ligas", "MainLeague", "AdminLigas","9", constClass.rolOwners,
                "Torneos", "Torneos", "AdminLigas","10", constClass.rolOwners,
                "Calendario", "Calendario", "AdminLigas","11", constClass.rolOwners,
                "Equipos", "EquipoNuevo", "AdminLigas","12", constClass.rolOwners,
                "Pagos", "Pagos", "AdminLigas","13", constClass.rolOwners,

                //--Administrador de Torneos
                "Dashboard", "Index", "AdminTorneos","14", constClass.rolAdminTorneos,
                "Torneos", "Torneos", "AdminTorneos","15", constClass.rolAdminTorneos,
                "Calendario", "Calendario", "AdminTorneos","16", constClass.rolAdminTorneos,
                "Arbitros", "Arbitros", "AdminTorneos","17", constClass.rolAdminTorneos,
                "Pagos", "Pagos", "AdminTorneos","18", constClass.rolAdminTorneos,

                //--Administrador de Equipos
                "Dashboard", "Dashboard", "AdminEquipos","19", constClass.rolCoach,
                "Calendario", "Calendario", "AdminEquipos","20", constClass.rolCoach,
                "Historial", "Historial", "AdminEquipos","21", constClass.rolCoach,

                //--Referee
                "Dashboard", "Index", "Referee","22", constClass.rolReferee,
                "Calendario", "Calendario", "Referee","23", constClass.rolReferee,
                "Historial", "Historial", "Referee","24", constClass.rolPlayer,
                //"Historial", "Historial", "Admin","24", constClass.rolReferee,
                //"Historial", "Records", "Admin","22", constClass.rolReferee,
                //--Coach
                //"Noticias", "Profile", "Admin","14", constClass.rolCoach,
                //"Calendario", "Calendar", "Admin","15", constClass.rolCoach,
                //"Historial", "Records", "Admin","16", constClass.rolCoach
            };

            string[] submenuArr = new string[]{     
                //--Player
                "Dashboard", "Dashboard", "Admin","0",
                "Mi Perfil", "Perfil", "Admin","0",
                "Otros Perfiles", "MisPerfiles", "Admin","0",
                "Mis Torneos", "Torneo", "Admin","0",
                "Mis Equipos", "MisEquipos", "Admin","0",
                "Mis Pagos", "MisPagos", "Admin","0",

                //--Admin de Enligate
                "Dashboard", "Index", "Administrator","3",
                "Mi Perfil", "Perfil", "Administrator","3",
                "Tipo de Torneo", "TipoTorneo", "Administrator","3",
                "Tarifas", "Fees", "Administrator","7",
                "Historial", "FeesPayments", "Administrator","7",
                "Publicidad", "FeesAdvertising", "Administrator","7",

                //--Administrador de Ligas
                "Dashboard", "Index", "AdminLigas","8",
                "Mi Perfil", "Perfil", "AdminLigas","8",
                "Crear Nuevo Torneo", "TorneoNuevo", "AdminLigas","8",
                "Crear Nuevo Equipo", "EquipoNuevo", "AdminLigas","8",

                "Liga Principal", "MainLeague", "AdminLigas","9",
                "Crear Nueva Liga", "AddLeague", "AdminLigas","9",
                "Ligas Activas", "Leagues", "AdminLigas","9",
                "Arbitros","Arbitros","AdminLigas","9",
                "Reputación", "Status", "AdminLigas","9",

                "Torneos", "Torneos", "AdminLigas","10",
                "Crear Nuevo Torneo", "TorneoNuevo", "AdminLigas","10",
                //"Equipos", "Equipos", "AdminLigas","10",                
                "Categorias", "TorneoCategorias", "AdminLigas","10",
                "Canchas", "TorneoCanchas", "AdminLigas","10",
                //"Crear Nuevo Equipo", "EquipoNuevo", "AdminLigas","12",
                "Partidos", "Partidos", "AdminLigas","10",
                "Equipos", "EquipoNuevo", "AdminLigas","12",
                
                //--Administrador de Torneos
                "Dashboard", "Index", "AdminTorneos","14",
                "Mi Perfil", "Perfil", "AdminTorneos","14",
                "Torneos", "Torneos", "AdminTorneos","15",
                //"Equipos", "Equipos", "AdminLigas","15",
                //"Crear Nuevo Torneo", "TorneoNuevo", "AdminTorneos","15",
                "Crear Nuevo Equipo", "EquipoNuevo", "AdminTorneos","15",
                //"Categorias", "TorneoCategorias", "AdminTorneos","15",
                //"Canchas", "TorneoCanchas", "AdminTorneos","15",

                //--Administrador de Equipos
                "Dashboard", "Dashboard", "AdminEquipos","19",
                "Mi Perfil", "Perfil", "AdminEquipos","19",
                "Otros Perfiles", "MisPerfiles", "AdminEquipos","19",
                "Mis Equipos", "MisEquipos", "AdminEquipos","19",
                "Mis Pagos", "MisPagos", "AdminEquipos","19",

                //--Referee
                "Dashboard", "Index", "Referee","22",
                "Mi Perfil", "Perfil", "Referee","22",
                "Ligas", "Ligas", "Referee","22",
                "Torneos", "Torneos", "Referee","22",
                //"Otros Perfiles", "MyProfiles", "Admin","11",

                //--Coach
                //"Mi Perfil", "Profile", "Admin","14",
                //"Otros Perfiles", "MyProfiles", "Admin","14",
            };

            List<schemaMenus> menuList = new List<schemaMenus>();
            List<schemaSubMenus> submenuList = new List<schemaSubMenus>();

            IdentityRole item;
            schemaMenus men;
            schemaSubMenus subMenu;
            
            for (int m = 0; m < menuArr.Count(); m += 5)
            {
                item = db.getRoleByName(menuArr[m + 4]);

                men = new schemaMenus();
                men.rolId = item.Id;
                men.menNombre = menuArr[m];
                men.menAction = menuArr[m + 1];
                men.menController = menuArr[m + 2];
                men.menId = Global_Functions.replaceSpecialChars(menuArr[m + 4].Replace(" ", "").Trim() + "_" + men.menNombre.Replace(" ", "").Trim());
                men.menOrden = int.Parse(menuArr[m + 3]);
                menuList.Add(men);

                int smenCounter = 0;
                for (int s = 0; s < submenuArr.Count(); s += 4)
                {
                    if(submenuArr[s + 3] == menuArr[m + 3])
                    {
                        subMenu = new schemaSubMenus();
                        subMenu.menId =  men.menId;
                        subMenu.smeNombre = submenuArr[s];
                        subMenu.smeAction = submenuArr[s + 1];
                        subMenu.smeController = submenuArr[s + 2];
                        subMenu.smeId = Global_Functions.replaceSpecialChars(men.menId + "_" + subMenu.smeNombre.Replace(" ", "").Trim());
                        subMenu.smeOrden = smenCounter;
                        smenCounter++;
                        submenuList.Add(subMenu);
                    }
                }
            }           

            db.addMenus(menuList, submenuList);

            var roles = db.getRoleByName(constClass.rolAdminTorneos);
            if(roles == null)
            {
                var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
                role.Create(new IdentityRole(constClass.rolAdminTorneos));
            }
                
        }

        public void crearTarifas()
        {
        //    var items = new List<schemaTarifasCfpptpMetodosPago>();
        //    var conceptoLiga = new schemaTarifasConceptos();
        //    var conceptoTorneo = new schemaTarifasConceptos();
        //    var formaPagoUnico = new schemaTarifasFormasPago();
        //    var formaPagoRenta = new schemaTarifasFormasPago();
        //    var formaPagoComision = new schemaTarifasFormasPago();
        //    var periodicidadUnico = new schemaTarifasPeriodicidades();
        //    var periodicidadAnual = new schemaTarifasPeriodicidades();
        //    var periodicidadMensual = new schemaTarifasPeriodicidades();
        //    var periodicidadSemanal = new schemaTarifasPeriodicidades();           
        //    var tipoPagoTotal = new schemaTarifasTiposPago();
        //    var tipoPagoAnticipo = new schemaTarifasTiposPago();
        //    var metodoPagoOxxo = new schemaTarifasMetodosPago();
        //    var metodoPagoTarjeta = new schemaTarifasMetodosPago();
        //    var metodoPagoTransferencia = new schemaTarifasMetodosPago();

        //    var conceptoFormaPagoTorneoUnico = new schemaTarifasConceptosFormasPago();
        //    var conceptoFormaPagoLigaRenta = new schemaTarifasConceptosFormasPago();
        //    var conceptoFormaPagoTorneoComision = new schemaTarifasConceptosFormasPago();

        //    var cfpPeriodicidadTorneoUnicoUnico = new schemaTarifasCfpPeriodicidades();
        //    var cfpPeriodicidadLigaRentaAnual = new schemaTarifasCfpPeriodicidades();
        //    var cfpPeriodicidadLigaRentaMensual = new schemaTarifasCfpPeriodicidades();
        //    var cfpPeriodicidadTorneoComisionSemanal = new schemaTarifasCfpPeriodicidades();

        //    var cfppTipoPagoTorneoUnicoUnicoTotal = new schemaTarifasCfppTiposPago();
        //    var cfppTipoPagoLigaRentaAnualTotal = new schemaTarifasCfppTiposPago();
        //    var cfppTipoPagoLigaRentaMensualTotal = new schemaTarifasCfppTiposPago();
        //    var cfppTipoPagoTorneoComisionSemanalTotal = new schemaTarifasCfppTiposPago();
        //    var cfppTipoPagoTorneoComisionSemanalAnticipo = new schemaTarifasCfppTiposPago();

        //    var cfpptpMetodosPago1 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago2 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago3 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago4 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago5 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago6 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago7 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago8 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago9 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago10 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago11 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago12 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago13 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago14 = new schemaTarifasCfpptpMetodosPago();
        //    var cfpptpMetodosPago15 = new schemaTarifasCfpptpMetodosPago();

        //    conceptoLiga.tcoIdConcepto = "LIGA";
        //    conceptoTorneo.tcoIdConcepto = "TORNEO";
        //    db.tempSetTarifasConceptos(conceptoLiga);
        //    db.tempSetTarifasConceptos(conceptoTorneo);
        //    formaPagoUnico.tfpIdFormaPago = "PAGO_UNICO";
        //    formaPagoRenta.tfpIdFormaPago = "RENTA";
        //    formaPagoComision.tfpIdFormaPago = "COMISION";
        //    db.tempSetTarifasFormasPago(formaPagoUnico);
        //    db.tempSetTarifasFormasPago(formaPagoRenta);
        //    db.tempSetTarifasFormasPago(formaPagoComision);
        //    conceptoFormaPagoTorneoUnico.tblTarifasConcepto = conceptoTorneo;
        //    conceptoFormaPagoTorneoUnico.tblTarifasFormaPago = formaPagoUnico;
        //    conceptoFormaPagoLigaRenta.tblTarifasConcepto = conceptoLiga;
        //    conceptoFormaPagoLigaRenta.tblTarifasFormaPago = formaPagoRenta;
        //    conceptoFormaPagoTorneoComision.tblTarifasConcepto = conceptoTorneo;
        //    conceptoFormaPagoTorneoComision.tblTarifasFormaPago = formaPagoComision;
        //    db.tempSetTarifasConceptosFormasPagoList(conceptoFormaPagoTorneoUnico);
        //    db.tempSetTarifasConceptosFormasPagoList(conceptoFormaPagoLigaRenta);
        //    db.tempSetTarifasConceptosFormasPagoList(conceptoFormaPagoTorneoComision);

        //    periodicidadUnico.tpeIdPeriodicidad = "UNICO";
        //    periodicidadAnual.tpeIdPeriodicidad = "ANUAL";
        //    periodicidadMensual.tpeIdPeriodicidad = "MENSUAL";
        //    periodicidadSemanal.tpeIdPeriodicidad = "SEMANAL";
        //    db.tempSetTarifasPeriodicidades(periodicidadUnico);
        //    db.tempSetTarifasPeriodicidades(periodicidadAnual);
        //    db.tempSetTarifasPeriodicidades(periodicidadMensual);
        //    db.tempSetTarifasPeriodicidades(periodicidadSemanal);
        //    cfpPeriodicidadTorneoUnicoUnico.tblTarifasConceptoFormaPago = conceptoFormaPagoTorneoUnico;
        //    cfpPeriodicidadTorneoUnicoUnico.tblTarifasPeriodicidad = periodicidadUnico;
        //    cfpPeriodicidadLigaRentaAnual.tblTarifasConceptoFormaPago = conceptoFormaPagoLigaRenta;
        //    cfpPeriodicidadLigaRentaAnual.tblTarifasPeriodicidad = periodicidadAnual;
        //    cfpPeriodicidadLigaRentaMensual.tblTarifasConceptoFormaPago = conceptoFormaPagoLigaRenta;
        //    cfpPeriodicidadLigaRentaMensual.tblTarifasPeriodicidad = periodicidadMensual;
        //    cfpPeriodicidadTorneoComisionSemanal.tblTarifasConceptoFormaPago = conceptoFormaPagoTorneoComision;
        //    cfpPeriodicidadTorneoComisionSemanal.tblTarifasPeriodicidad = periodicidadSemanal;
        //    db.tempSetTarifasCfpPeriodicidadesList(cfpPeriodicidadTorneoUnicoUnico);
        //    db.tempSetTarifasCfpPeriodicidadesList(cfpPeriodicidadLigaRentaAnual);
        //    db.tempSetTarifasCfpPeriodicidadesList(cfpPeriodicidadLigaRentaMensual);
        //    db.tempSetTarifasCfpPeriodicidadesList(cfpPeriodicidadTorneoComisionSemanal);

        //    tipoPagoTotal.ttpIdTipoPago = "TOTAL";
        //    tipoPagoAnticipo.ttpIdTipoPago = "ANTICIPO";
        //    db.tempSetTarifasTiposPago(tipoPagoTotal);
        //    db.tempSetTarifasTiposPago(tipoPagoAnticipo);
        //    cfppTipoPagoTorneoUnicoUnicoTotal.tblTarifasCfpPeriodicidad = cfpPeriodicidadTorneoUnicoUnico;
        //    cfppTipoPagoTorneoUnicoUnicoTotal.tblTarifasTipoPago = tipoPagoTotal;
        //    cfppTipoPagoLigaRentaAnualTotal.tblTarifasCfpPeriodicidad = cfpPeriodicidadLigaRentaAnual;
        //    cfppTipoPagoLigaRentaAnualTotal.tblTarifasTipoPago = tipoPagoTotal;
        //    cfppTipoPagoLigaRentaMensualTotal.tblTarifasCfpPeriodicidad = cfpPeriodicidadLigaRentaMensual;
        //    cfppTipoPagoLigaRentaMensualTotal.tblTarifasTipoPago = tipoPagoTotal;
        //    cfppTipoPagoTorneoComisionSemanalTotal.tblTarifasCfpPeriodicidad = cfpPeriodicidadTorneoComisionSemanal;
        //    cfppTipoPagoTorneoComisionSemanalTotal.tblTarifasTipoPago = tipoPagoTotal;
        //    cfppTipoPagoTorneoComisionSemanalAnticipo.tblTarifasCfpPeriodicidad = cfpPeriodicidadTorneoComisionSemanal;
        //    cfppTipoPagoTorneoComisionSemanalAnticipo.tblTarifasTipoPago = tipoPagoAnticipo;
        //    db.tempSetTarifasCfppTiposPagoList(cfppTipoPagoTorneoUnicoUnicoTotal);
        //    db.tempSetTarifasCfppTiposPagoList(cfppTipoPagoLigaRentaAnualTotal);
        //    db.tempSetTarifasCfppTiposPagoList(cfppTipoPagoLigaRentaMensualTotal);
        //    db.tempSetTarifasCfppTiposPagoList(cfppTipoPagoTorneoComisionSemanalTotal);
        //    db.tempSetTarifasCfppTiposPagoList(cfppTipoPagoTorneoComisionSemanalAnticipo);

        //    metodoPagoOxxo.tmpIdMetodoPago = "OXXO";
        //    metodoPagoTarjeta.tmpIdMetodoPago = "TARJETA";
        //    metodoPagoTransferencia.tmpIdMetodoPago = "TRANSFERENCIA";
        //    db.tempSetTarifasMetodosPago(metodoPagoOxxo);
        //    db.tempSetTarifasMetodosPago(metodoPagoTarjeta);
        //    db.tempSetTarifasMetodosPago(metodoPagoTransferencia);
        //    cfpptpMetodosPago1.tblTarifasCfppTipoPago = cfppTipoPagoTorneoUnicoUnicoTotal;
        //    cfpptpMetodosPago1.tblTarifasMetodoPago = metodoPagoOxxo;
        //    cfpptpMetodosPago2.tblTarifasCfppTipoPago = cfppTipoPagoTorneoUnicoUnicoTotal;
        //    cfpptpMetodosPago2.tblTarifasMetodoPago = metodoPagoTarjeta;
        //    cfpptpMetodosPago3.tblTarifasCfppTipoPago = cfppTipoPagoTorneoUnicoUnicoTotal;
        //    cfpptpMetodosPago3.tblTarifasMetodoPago = metodoPagoTransferencia;
        //    cfpptpMetodosPago4.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaAnualTotal;
        //    cfpptpMetodosPago4.tblTarifasMetodoPago = metodoPagoOxxo;
        //    cfpptpMetodosPago5.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaAnualTotal;
        //    cfpptpMetodosPago5.tblTarifasMetodoPago = metodoPagoTarjeta;
        //    cfpptpMetodosPago6.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaAnualTotal;
        //    cfpptpMetodosPago6.tblTarifasMetodoPago = metodoPagoTransferencia;
        //    cfpptpMetodosPago7.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaMensualTotal;
        //    cfpptpMetodosPago7.tblTarifasMetodoPago = metodoPagoOxxo;
        //    cfpptpMetodosPago8.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaMensualTotal;
        //    cfpptpMetodosPago8.tblTarifasMetodoPago = metodoPagoTarjeta;
        //    cfpptpMetodosPago9.tblTarifasCfppTipoPago = cfppTipoPagoLigaRentaMensualTotal;
        //    cfpptpMetodosPago9.tblTarifasMetodoPago = metodoPagoTransferencia;
        //    cfpptpMetodosPago10.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalTotal;
        //    cfpptpMetodosPago10.tblTarifasMetodoPago = metodoPagoOxxo;
        //    cfpptpMetodosPago11.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalTotal;
        //    cfpptpMetodosPago11.tblTarifasMetodoPago = metodoPagoTarjeta;
        //    cfpptpMetodosPago12.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalTotal;
        //    cfpptpMetodosPago12.tblTarifasMetodoPago = metodoPagoTransferencia;
        //    cfpptpMetodosPago13.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalAnticipo;
        //    cfpptpMetodosPago13.tblTarifasMetodoPago = metodoPagoOxxo;
        //    cfpptpMetodosPago14.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalAnticipo;
        //    cfpptpMetodosPago14.tblTarifasMetodoPago = metodoPagoTarjeta;
        //    cfpptpMetodosPago15.tblTarifasCfppTipoPago = cfppTipoPagoTorneoComisionSemanalAnticipo;
        //    cfpptpMetodosPago15.tblTarifasMetodoPago = metodoPagoTransferencia;

        //    items.Add(cfpptpMetodosPago1);
        //    items.Add(cfpptpMetodosPago2);
        //    items.Add(cfpptpMetodosPago3);
        //    items.Add(cfpptpMetodosPago4);
        //    items.Add(cfpptpMetodosPago5);
        //    items.Add(cfpptpMetodosPago6);
        //    items.Add(cfpptpMetodosPago7);
        //    items.Add(cfpptpMetodosPago8);
        //    items.Add(cfpptpMetodosPago9);
        //    items.Add(cfpptpMetodosPago10);
        //    items.Add(cfpptpMetodosPago11);
        //    items.Add(cfpptpMetodosPago12);
        //    items.Add(cfpptpMetodosPago13);
        //    items.Add(cfpptpMetodosPago14);
        //    items.Add(cfpptpMetodosPago15);

        //    db.tempSetTarifasMetodosPagoList(items);
        }

        #region Constructores

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminController()
        {
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        /// Realiza el cambio de rol del usuario autentificado y redirecciona a la página del perfil
        /// </summary>
        /// <param name="ddlCurrentRole"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeCurrentRole(string ddlCurrentRole)
        {
            //var usr = db.getUserById();
            db.setCurrentUserRole(User.Identity.GetUserId(), ddlCurrentRole);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Realiza la carga de la pagina Index del Dashboard, redirecciona al perfile
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //Temporal, es para llenar la base de datos
            //crearMenus();
            //crearTarifas();

            //Redirecciona dependiendo del rol actual.
            var userRolesDic = db.set_getUserCurrentRole(User.Identity.Name);
            switch ((string)userRolesDic["currentUsrRoleName"])
            {
                case constClass.rolAdmin:
                    return RedirectToAction("Index","Administrator");
                case constClass.rolOwners:
                    return RedirectToAction("Index", "AdminLigas");
                case constClass.rolAdminTorneos:
                    return RedirectToAction("Index", "AdminTorneos");
                case constClass.rolCoach:
                    return RedirectToAction("Index", "AdminEquipos");
                case constClass.rolReferee:
                    return RedirectToAction("Index", "Referee");
                case constClass.rolPlayer:
                    return RedirectToAction("Dashboard");
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Regresa la pantalla de Index para mostrar el dashboar del rol jugador.
        /// </summary>
        /// <returns></returns>
        public ActionResult Dashboard()
        {
            var usr = User.Identity.GetUserId();
            var user = db.getUserById(usr);
            var model = db.getAvisos(user.Id, user.usuRolActual);
            ViewBag.numAvisos = model.Count();
            return View("Index");
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

            model._profile.dllContry.Add(new SelectListItem { Text = "SELECCIONE PAIS", Value = "0" });

            model._profile.dllContry.AddRange(db.getCountry()
                     .Select(l => new SelectListItem { Text = l.name.ToUpper(), Value = l.iso.ToUpper() })
                     .ToList());

            model._profile.dllState.Add(new SelectListItem { Text = "SELECCIONE ESTADO", Value = "0" });

            model._profile.dllCity.Add(new SelectListItem { Text = "SELECCIONE CIUDAD", Value = "0" });

            model._profile.usuPais = (prof.uprPais != null) ? prof.uprPais.Trim() : "";
            model._profile.usuEstado = (prof.uprEstado != null) ? prof.uprEstado.Trim() : "";
            model._profile.usuCiudad = (prof.uprCiudad != null) ? prof.uprCiudad.Trim() : "";

            model._profile.codeIdPais = (prof.codeIdPais != null) ? prof.codeIdPais.Trim() : "";
            model._profile.codeIdEstado = (prof.codeIdEstado != null) ? prof.codeIdEstado.Trim() : "";
            model._profile.codeIdCiudad = (prof.codeIdCiudad != null) ? prof.codeIdCiudad.Trim() : "";

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
        public ActionResult _CompleteProfile()
        {
            ProfileViewModel model = getPerfilUsuario(User.Identity.Name);

            return PartialView("Admin/_CompleteProfile", model);
        }
        [HttpPost]
        public JsonResult _GetPerfilName()
        {            
            var users = db.getMisCuentas(User.Identity.GetUserId()).Where(l=> l.activo.Equals(true));
            List<string> list = new List<string>();
            int i = 1;
            foreach (var item in users)
            {
                var name = db.getUserById(item.userId).UserName;
                ProfileViewModel model = getPerfilUsuario(name);
                model._profile.profileNumber = "profile_"+i;
                model._profile.user_id = item.userId;
                //  list.Add(RenderPartialViewToString("Admin/_Perfil", model._profile));           
                list.Add(RenderPartialViewToString("Admin/_PerfilOtros", model._profile));
                i++;
            }

            var json = list.ToArray();
            return Json(json);
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
            
            var profile = new schemaUsersProfiles();
            var nombreCompleto = Global_Functions.getName_LastName(model.usuNombreCompleto);
            profile.uprNombres = nombreCompleto["name"];
            profile.uprApellidos = nombreCompleto["lastname"];
            profile.uprGenero = (model.usuGenero != null) ? model.usuGenero : null;
            profile.uprFechaNacimiento = Global_Functions.stringToDate(model.usuFechaNacimiento);

            profile.uprPais = (model.usuPais != null) ? model.usuPais.Trim() : null;
            profile.uprEstado = (model.usuEstado != null) ? model.usuEstado.Trim() : null;
            profile.uprCiudad = (model.usuCiudad != null) ? model.usuCiudad.Trim() : null;

            profile.codeIdPais = (model.codeIdPais != null) ? model.codeIdPais.Trim() : null;
            profile.codeIdEstado = (model.codeIdEstado != null) ? model.codeIdEstado.Trim() : null;
            profile.codeIdCiudad = (model.codeIdCiudad != null) ? model.codeIdCiudad.Trim() : null;

            profile.cp = (model.usuCP > 0) ? model.usuCP : 0;
            profile.uprTelefono = (model.usuTelefono != null) ? model.usuTelefono.Trim() : null;

            //usr.Email = (model.usuCorreo != null) ? model.usuCorreo.Trim() : null;

            if (db.setUserProfileMain_UpdateInsert(usr, profile))
            {
                //  ModelState.AddModelError(constClass.success, "Guardado.");
            }
            else
            {
                ModelState.AddModelError(constClass.error, "Hubo un error guardando la información.");
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
       // [ValidateAntiForgeryToken]
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

        public JsonResult delete_profile(string userId)
        {
            try
            {
                var userAdmin = User.Identity.GetUserId();
                if (db.set_deleteMisCuentas(userId, userAdmin))
                {
                    return Json("La cuenta se dio de baja");
                }
                else
                {
                    return Json("ocurrio un error al dar de baja la cuenta");
                }
            
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
                throw;
            }            
        }

        public JsonResult ValidateEmailExistSubProfile(string email)
        {
            var userAdmin = User.Identity.GetUserId();
            var userSub = db.getUserByUserEmail(email);
            var counts = db.getMisCuentas(userAdmin);
            var msg = "failed"; var status = "Disponible";
            if (userSub!=null)
            {
                if (counts != null && counts.Count > 0)
                {
                    var countSub = counts.Where(l => l.userId == userSub.Id);
                    if (countSub.Count() > 0)
                    {
                        msg = "Ya estas seguiendo esta cuenta";
                        status = "success";
                    }
                    else
                    {
                        msg = "Esta cuenta esta disponible";
                        status = "failed";
                    }
                }
            }else
            {
                msg = "Esta cuenta esta disponible";
                status = "failed";
            }
            
            var data = new { status = status, msg = msg};
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// Regresa el submenu del menú actual
        /// </summary>
        /// <param name="menuId">Id del menú actual</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _GetSubmenuByCurrentMenu(string menuId)
        {
            var model = db.getSubMenusByMenId(menuId);
            return PartialView("_SubmenuView", model);
        }

        #region Torneo/Equipo Confirmar Participacion
        public ActionResult Torneo()
        {
            // var userId = User.Identity.GetUserId();
            // var liga = db.getLigaByJugador(userId);
            // ViewBag.ligId = liga.ligId;
            // ViewBag.ligKey = liga.ligUserIdCreador;
            ViewBag.haveTorneos = db.getTorneoByPlayer(User.Identity.GetUserId()).Count;
           return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult TorneoEstadisticas(int Id)
        {
            return PartialView("Torneos/_TorneoEstadisticasPartial", Id);
            //return View();
        }
        #region Acciones

        public ActionResult changeRole(int? id, string rol)
        {
            var userId = User.Identity.GetUserId();
            var usuario = db.getUserById(userId);
            if (id!=null && rol != "")
            {
                var rolEquipo  = db.getRoles().Where(l => l.rolName == constClass.rolCoach).First();
                if (usuario.usuRolActual != rolEquipo.rolId)
                {
                    var userRol = db.getUserRoles(usuario).Where(l => l.rolName == constClass.rolPlayer);
                    if (userRol.Count() <= 0)
                    {
                        UserManager.AddToRole(userId, constClass.rolPlayer);
                    }
                    else
                    {
                        db.setCurrentUserRole(userId, rolEquipo.rolId);
                    }
                }
                return RedirectToAction("MisEquipos","AdminEquipos");
            }
            var rolIdJug = db.getRoles().Where(l => l.rolName == constClass.rolPlayer).First();
            if (usuario.usuRolActual != rolIdJug.rolId)
            {
                var userRol = db.getUserRoles(usuario).Where(l => l.rolName == constClass.rolPlayer);
                if (userRol.Count() <= 0)
                {
                    UserManager.AddToRole(userId, constClass.rolPlayer);
                }else
                {
                    db.setCurrentUserRole(userId, rolIdJug.rolId);
                }
            }

            return RedirectToAction("MisEquipos");
        }
        public ActionResult _newPlayerRegister(string email,int torId,int equId)
        {
            var usr = db.getUserByUserEmail(email);
            var jugador = db.getJugadorByIds(email, torId, equId);
            var usuPassword = "";var usuarioId = "";
            if (usr == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = "",
                    EmailConfirmed = false,
                    usuEstatus = true
                };
                usuPassword = db.generator_Pass();
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
                    usuarioId = user.Id;
                }
                //db.setClearEmailValidation(user);

                
                //var res = Login(user.Id, user.Email, usuPassword, false);

                enviarEmailParticipanteAviso(jugador, user.Email, jugador.tblTorneos.torNombreTorneo, jugador.tblEquipos.equNombreEquipo, usuPassword);
            }
            else
            {
                usuarioId = usr.Id;
            }

            if(db.setJugadoresEquipo_ConfirmarParticipacion(email, torId, equId, usuarioId,""))
            {
                string usuarioNombre = null;
                var user = db.getUserById(usuarioId);
                if (user == null)
                    usuarioNombre = email;
                else
                {
                    var prof = db.getUserMainProfile(user.Id);
                    if (prof == null)
                        prof = new schemaUsersProfiles();
                    usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                    if (usuarioNombre == "")
                        usuarioNombre = user.Email;

                }

                string nombreEquipo = null;

                ViewBag.UsuarioNombre = usuarioNombre;
                ViewBag.esEquipo = false;
                if (equId > 0)
                {
                    ViewBag.esEquipo = true;
                    nombreEquipo = jugador.tblEquipos.equNombreEquipo;
                    ViewBag.EquipoNombre = nombreEquipo;
                }
                ViewBag.TorneoNombre = jugador.tblTorneos.torNombreTorneo;
                //var res = Login(user.Id, user.Email, user.PasswordHash, false);

                var usuario = UserManager.FindById(user.Id);

                enviarEmailParticipanteAvisoConfirmacion(jugador, usuarioNombre, jugador.tblTorneos.torNombreTorneo, nombreEquipo);

                if (usuario != null)
                {
                    //SignInManager.SignInAsync(usuario, true, true);

                    var rolActual = usuario.usuRolActual;
                    var rolPlayer = db.getRoleByName(constClass.rolPlayer);
                    if (rolPlayer.Id.ToUpper() != rolActual.ToUpper())
                    {
                        var rolUser = db.getUserRoles(usuario).Where(l => l.rolName.ToUpper() == constClass.rolPlayer.ToUpper());
                        if (rolUser == null)
                            UserManager.AddToRole(usuario.Id, constClass.rolPlayer);
                        db.setCurrentUserRole(usuario.Id, rolPlayer.Id);
                    }
                }
                SignInManager.SignIn(usuario, true, false);
                ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                return Redirect("Index");
            }
            return Json("wrong");
        }

        /// <summary>
        /// Confirma la participacion en el torneo y/o equipo.
        /// Tiene que estar registrado para confirmar su participación.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>

        [AllowAnonymous]
        public ActionResult ParticipanteConfirmar(string email, int torId, int equId, string code)
        {
            //Valida que la persona que está confirmando la cuenta exista y tenga la sesión iniciada.
            bool cerrarSesion = false;
            var usuPassword = "";
            var usr = db.getUserByUserEmail(email);
            if(usr == null)
                cerrarSesion = true;
            else
                if(usr.Id != User.Identity.GetUserId())
                    cerrarSesion = true;

            if(cerrarSesion)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }

            //Si el usuario inicio sesión con su cuenta, se procede a validar la información.
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            var jugador = db.getJugadorByIds(email, torId, equId);
            var usuarioId = "";
            if (jugador != null)
            {
                if (jugador.jugCodigoConfirmacion == code.Trim())
                {
                    if (usr == null)
                    {
                        ApplicationUser user = new ApplicationUser
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

                        var newUser = new AccountController()._RegisterNew(modelRegister,true);
                        if (newUser)
                        {
                            ApplicationUser userId = db.getUserByUserEmail(email);
                            user.Id = userId.Id;
                            if (userId.usuRolActual==null)
                            {
                                var rolId = db.getRoleByName(constClass.rolPlayer).Id;                                
                                UserManager.AddToRole(userId.Id, constClass.rolPlayer);
                                db.setCurrentUserRole(userId.Id, rolId);
                            }
                        }else
                        {
                            usuarioId = null;
                        }
                        //  var userId = RegisterPlayer(user, usuPassword,constClass.rolPlayer);
                        //db.setClearEmailValidation(user);

                        usuarioId = user.Id;
                        //var res = Login(user.Id, user.Email, usuPassword, false);

                        enviarEmailParticipanteAviso(jugador, user.Email, jugador.tblTorneos.torNombreTorneo, jugador.tblEquipos.equNombreEquipo, usuPassword);
                    }else
                    {
                        usuarioId = usr.Id;
                    }
                    
                    if (db.setJugadoresEquipo_ConfirmarParticipacion(email, torId, equId, usuarioId, jugador.jugCodigoConfirmacion))
                    {
                        string usuarioNombre = null;
                        var user = db.getUserByUserEmail(email);
                        if (user == null)
                            usuarioNombre = email;
                        else
                        {
                            var prof = db.getUserMainProfile(user.Id);
                            if (prof == null)
                                prof = new schemaUsersProfiles();
                            usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                            if (usuarioNombre == "")
                                usuarioNombre = user.Email;

                        }

                        string nombreEquipo = null;

                        ViewBag.UsuarioNombre = usuarioNombre;
                        ViewBag.esEquipo = false;
                        if (equId > 0)
                        {
                            ViewBag.esEquipo = true;
                            nombreEquipo = jugador.tblEquipos.equNombreEquipo;
                            ViewBag.EquipoNombre = nombreEquipo;
                        }
                        ViewBag.TorneoNombre = jugador.tblTorneos.torNombreTorneo;
                        //var res = Login(user.Id, user.Email, user.PasswordHash, false);

                        var usuario = UserManager.FindById(user.Id);

                        
                        enviarEmailParticipanteAvisoConfirmacion(jugador, usuarioNombre, jugador.tblTorneos.torNombreTorneo, nombreEquipo);

                        if (usuario != null)
                        {
                            //SignInManager.SignInAsync(usuario, true, true);
                            
                            var rolActual = (usuario.usuRolActual != null)? usuario.usuRolActual:"";
                            var rolPlayer = db.getRoleByName(constClass.rolPlayer);
                            if (rolPlayer.Id.ToUpper() != rolActual.ToUpper())
                            {
                                var rolUser = db.getUserRoles(usuario).Where(l => l.rolName.ToUpper() == constClass.rolPlayer.ToUpper()).ToList();
                                if (rolUser.Count <= 0)
                                    UserManager.AddToRole(usuario.Id, constClass.rolPlayer);
                                db.setCurrentUserRole(usuario.Id, rolPlayer.Id);
                            }
                        }
                        SignInManager.SignIn(usuario, false, false);
                        ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                        return Redirect("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error confirmando la participacion. Redireccionando . . .");
                        ViewBag.jsScript = redirectHome;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "El codigo de confirmación no es válido, favor de verificar los datos. Redireccionando . . .");
                    ViewBag.jsScript = redirectHome;
                    return View("Home/_ErrorCode");
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "No se encontro el participante, favor de verificar los datos. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }
            return Redirect("Index");
            //return View("AccionesCorreos/_ParticipanteConfirmar");
        }

       
        public String RegisterPlayer(ApplicationUser user, string usuPassword,string role)
        {


            ApplicationUser usr = db.getUserByUserEmail(user.Email);
            if (usr != null)
            {
                ModelState.AddModelError(constClass.info, "El correo electrónico ya está registrado. Inicia sesión o recupera la contraseña.");
            }
            else
            {
                var result = UserManager.Create(user, usuPassword);
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
        /// <summary>
        /// El participante rechaza que no quiere ser registrado en el torneo y/o equipo.
        /// </summary>
        /// <param name="torId"></param>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ParticipanteRechazar(string email, int torId, int equId, string code)
        {
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            var jugador = db.getJugadorEquipoByIds(email, torId, equId);

            if (jugador != null)
            {
                if(jugador.jugCodigoConfirmacion == code.Trim())
                {
                    if (db.setJugadoresEquipo_RechazarParticipacion(email, torId, equId))
                    {
                        string usuarioNombre = null;
                        var user = db.getUserByUserEmail(email);
                        if (user == null)
                            usuarioNombre = email;
                        else
                        {
                            var prof = db.getUserMainProfile(user.Id);
                            if (prof == null)
                                prof = new schemaUsersProfiles();
                            usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                            if (usuarioNombre == "")
                                usuarioNombre = user.Email;
                            
                        }

                        string nombreEquipo = null;

                        ViewBag.UsuarioNombre = usuarioNombre;
                        ViewBag.esEquipo = false;
                        if (equId > 0)
                        {
                            ViewBag.esEquipo = true;
                            nombreEquipo = jugador.tblEquipos.equNombreEquipo;
                            ViewBag.EquipoNombre = nombreEquipo;
                        }
                        ViewBag.TorneoNombre = jugador.tblTorneos.torNombreTorneo;

                        enviarEmailParticipanteAvisoRechazo(jugador, usuarioNombre, jugador.tblTorneos.torNombreTorneo, nombreEquipo);
                        
                        ModelState.AddModelError(constClass.success, "La solicitud ha sido rechazada.");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error rechazando la participacion. Redireccionando . . .");
                        ViewBag.jsScript = redirectHome;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "El codigo de confirmación no es válido, favor de verificar los datos. Redireccionando . . .");
                    ViewBag.jsScript = redirectHome;
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "No se encontro el participante, favor de verificar los datos. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }

            return View("AccionesCorreos/_ParticipanteRechazar");
        }

        #endregion
        [AllowAnonymous]
        public ActionResult AdminEquipoConfirmar(string email, int torId, int equId, string code)
        {
            bool cerrarSesion = false;
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

            var user = db.getUserByUserEmail(email);
            var usuPassword = "";var usuarioId = "";
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
                var modelRegister = new RegisterViewModel();
                usuPassword = db.generator_Pass();
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
                    user = userId;
                    usuarioId = userId.Id;
                }


                db.setClearEmailValidation(user);
                usuarioId = user.Id;
                //var res = Login(user.Id, user.Email, usuPassword, false);
                var tor = db.getTorneoById(torId);
                var equ = db.getEquipoById(equId);
                enviarEmailParticipanteAdminEquipoAviso(user,tor.torNombreTorneo, equ.equNombreEquipo,usuPassword);
            }
            else
            {
                usuarioId = usr.Id;
            }

            if (user != null)
            {
                if (db.setAdminEquipo_ConfirmarParticipacion(user,torId,equId))
                {
                    string usuarioNombre = null;
                    //var user = db.getUserByUserName(email);
                    if (user == null)
                        usuarioNombre = email;
                    else
                    {
                        var prof = db.getUserMainProfile(user.Id);
                        if (prof == null)
                            prof = new schemaUsersProfiles();
                        usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                        if (usuarioNombre == "")
                            usuarioNombre = user.Email;
                        //var arb=db.getArbitroByEmail(user.Email);
                    }

                    ViewBag.UsuarioNombre = usuarioNombre;
                    if (user != null)
                    {
                        var rolActual = user.usuRolActual;
                        var rolPlayer = db.getRoleByName(constClass.rolCoach);
                        if (rolActual != null)
                        {
                            if (rolPlayer.Id.ToUpper() != rolActual.ToUpper())
                            {
                                var rolUser = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolCoach.ToUpper());
                                if (!rolUser.Any())
                                    UserManager.AddToRole(user.Id, constClass.rolCoach);
                                db.setCurrentUserRole(user.Id, rolPlayer.Id);
                            }
                        }else
                        {
                            UserManager.AddToRole(user.Id, constClass.rolCoach);
                            db.setCurrentUserRole(user.Id, rolPlayer.Id);
                        }                        
                    }
                    SignInManager.SignIn(user, false, false);
                    ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "Hubo un error confirmando la participacion. Redireccionando . . .");
                    ViewBag.jsScript = redirectHome;
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "No se encontro el participante, favor de verificar los datos. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }

            return View("AccionesCorreos/_ParticipanteConfirmar");
        }
        [AllowAnonymous]
        public ActionResult AdminEquipoRechazar(string email, int torId, int equId, string code) {


            ViewBag.UsuarioNombre = email;
            ViewBag.esEquipo = true;

            var equipo = db.getEquipoById(equId);
            var torneo = db.getTorneoById(torId);

            var torCoa = db.getTorneoCoadministradoresById(torId);

            var torEmail = torneo.tblUserCreador.Email;

            var ligCoad = db.getCoAdminLigasByLigIg(torneo.ligId);

            if (ligCoad.Any())
            {
                torEmail += "," + String.Join(",", ligCoad.Select(s => s.tblUsuario.Email).ToArray());
            }
            if (torCoa.Any())
            {
                torEmail += ","+String.Join(",", torCoa.Select(s => s.tcaEmail).ToArray());
            }

            ViewBag.EquipoNombre = equipo.equNombreEquipo;
            ViewBag.TorneoNombre = torneo.torNombreTorneo;

            var admLiga = db.getLigaById(torneo.ligId);

            var envio  = enviarEmailCoachAvisoRechazo(email, admLiga.ligCorreoContacto, torEmail, torneo.torNombreTorneo, equipo.equNombreEquipo);

            var aviso="";

            if (envio)
            {
                aviso = "se envio el correo";
            }
            ViewBag.aviso = aviso;

            return View("AccionesCorreos/_ParticipanteRechazar");
        }

        protected SignInStatus Login(string userId, string email, string password, bool remember)
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            //AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = remember }); //, await user.GenerateUserIdentityAsync(UserManager));

            SignInStatus resultLogin = SignInManager.PasswordSignIn(email, password, remember, true);
            if (resultLogin == SignInStatus.Success)
            {
                UserManager.ResetAccessFailedCount(userId);
                //Registrar en el log (AspNetUserLogins)
            }
            return resultLogin;
        }
        #region Arbitro Confirmar
        [AllowAnonymous]
        public ActionResult ArbitroConfirmar(string email, int ligId, string code)
        {
            //Valida que la persona que está confirmando la cuenta exista y tenga la sesión iniciada.
            bool cerrarSesion = false;
            var usuPassword=""; var usuarioId = "";
            var usr = db.getUserByUserEmail(email);
            if (usr == null)
                cerrarSesion = true;
            else
                if (usr.Id != User.Identity.GetUserId())
                cerrarSesion = true;

            if (cerrarSesion)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
               // TempData["ReturnUrl"] = Url.Action("ArbitroConfirmar", "Admin", new { email = email, torId = torId, code = code });
               // return RedirectToAction("Index", "Home");
            }
            if (usr == null)
            {
                ApplicationUser user = new ApplicationUser
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
                    user = userId;
                    usuarioId = user.Id;
                }
                
                db.setClearEmailValidation(user);
                usuarioId = user.Id;
                var usuario = db.getUserById(usuarioId);
                //var res = Login(user.Id, user.Email, usuPassword, false);

                enviarEmailArbitroAviso(usuario, usuPassword);
            }
            else
            {
                usuarioId = usr.Id;
            }

            //Si el usuario inicio sesión con su cuenta, se procede a validar la información.
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";
            
            var arbUser = db.getArbitroByEmail(email).FirstOrDefault();
            if (arbUser != null)
            {
                if (arbUser.arbUserId == null)
                {
                    db.setArbitroUserId(usuarioId,email);
                }
                var arbitro = db.getArbitroLigId(email, ligId).FirstOrDefault();
                if (arbitro.arbCodigoConfirmacion == code.Trim())
                {
                    if (db.setArbitros_ConfirmarParticipacion(usuarioId, email, ligId))
                    {
                        var liga = db.getLigaById(ligId);
                        enviarEmailArbitroConfirmacion(email, liga.tblUserCreador.Email,liga.ligNombreLiga);
                        string usuarioNombre = null;
                        var user = db.getUserByUserEmail(email);
                        if (user == null)
                            usuarioNombre = email;
                        else
                        {
                            var prof = db.getUserMainProfile(user.Id);
                            if (prof == null)
                                prof = new schemaUsersProfiles();
                            usuarioNombre = (prof.uprNombres + " " + prof.uprApellidos).Replace("-", "").Trim();
                            if (usuarioNombre == "")
                                usuarioNombre = user.Email;
                           //var arb=db.getArbitroByEmail(user.Email);
                        }

                        //string nombreEquipo = null;
                        //var torneo = db.getTorneoById();
                        ViewBag.UsuarioNombre = usuarioNombre;
                        if (user != null)
                        {                          

                            var rolActual = user.usuRolActual;
                            var rolReferee = db.getRoleByName(constClass.rolReferee);
                            if (rolActual!=null)
                            {                            
                                if (rolReferee.Id.ToUpper() != rolActual.ToUpper())
                                {
                                    var rolUser = db.getUserRoles(user).Where(l => l.rolName.ToUpper() == constClass.rolReferee.ToUpper());
                                    if (!rolUser.Any())
                                        UserManager.AddToRole(user.Id, constClass.rolReferee);
                                    db.setCurrentUserRole(user.Id, rolReferee.Id);
                                }
                                else
                                {
                                    //UserManager.AddToRole(user.Id, constClass.rolReferee);
                                    db.setCurrentUserRole(user.Id, rolReferee.Id);
                                }
                            }
                            else
                            {
                                UserManager.AddToRole(user.Id, constClass.rolReferee);
                                db.setCurrentUserRole(user.Id, rolReferee.Id);
                            }
                            SignInManager.SignIn(user, false, false);
                        }
                        ModelState.AddModelError(constClass.success, "La solicitud ha sido confirmada.");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error confirmando la participacion. Redireccionando . . .");
                        ViewBag.jsScript = redirectHome;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "El codigo de confirmación no es válido, favor de verificar los datos. Redireccionando . . .");
                    ViewBag.jsScript = redirectHome;
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "No se encontro el participante, favor de verificar los datos. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }

            return Redirect("Index");
        }

        [AllowAnonymous]
        public ActionResult ArbitroRechazar(string email, int ligId, string code)
        {
            string rand = Global_Functions.getRandomString(10);
            string redirectHome = @"function jsRedirect_Home" + rand + @"(){
                                        document.location.replace('" + Url.Action("Index", "Home") + "');" +
                                            @"}
                                    setTimeout(jsRedirect_Home" + rand + ",2500);";

            var arbitro = db.getArbitros(ligId).Where(l=> l.arbCorreo == email).First();
            if (arbitro != null)
            {
                var arb = db.getArbitroLigId(arbitro.arbCorreo, ligId).FirstOrDefault();
                if (arb.arbCodigoConfirmacion == code.Trim())
                {
                    if (db.setArbitroTorneo_RechazarParticipacion(email, ligId))
                    {                       
                        var lig = db.getLigaById(ligId);
                        var emailTo = lig.tblUserCreador.Email;
                        var CoAdminLig = db.getCoAdminLigasByLigIg(ligId).ToList();
                        if (CoAdminLig.Any())
                        {
                            emailTo += "," + String.Join(",", CoAdminLig.Select(s => s.tblUsuario.Email).ToArray());
                        }
                        
                        enviarEmailArbitroAvisoRechazo(email, emailTo , lig.ligNombreLiga);
                        //enviarEmailArbitroAvisoRechazo(usuarioNombre, lig.tblUserCreador.Email, lig.ligNombreLiga);
                        
                        ModelState.AddModelError(constClass.success, "La solicitud ha sido rechazada.");
                    }
                    else
                    {
                        ModelState.AddModelError(constClass.error, "Hubo un error rechazando la participacion. Redireccionando . . .");
                        ViewBag.jsScript = redirectHome;
                    }
                }
                else
                {
                    ModelState.AddModelError(constClass.error, "El codigo de confirmación no es válido, favor de verificar los datos. Redireccionando . . .");
                    ViewBag.jsScript = redirectHome;
                }
            }
            else
            {
                ModelState.AddModelError(constClass.error, "No se encontro el participante, favor de verificar los datos. Redireccionando . . .");
                ViewBag.jsScript = redirectHome;
            }

            return View("AccionesCorreos/_ParticipanteRechazar");
        }
        #endregion
        #region Funciones
        [NonAction]
        public bool enviarEmailParticipanteAdminEquipoAviso(ApplicationUser user,string nombreTorneo, string nombreEquipo, string password)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = user.Email;

                string body = Global_Functions.getBodyHTML("~/Emails/AdminEquipoAvisoConfirmacion.html");

                body = body.Replace("<%= NombreAdmEquipo %>", user.UserName);
                body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
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

        public bool enviarEmailArbitroConfirmacion(string emailArb,string emailAdmLig,string liga)
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

        [NonAction]
        public bool enviarEmailParticipanteAviso(schemaJugadorEquipos jugador, string nombreJugador, string nombreTorneo, string nombreEquipo,string password){
         // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    //string emailTo = nombreJugador+","+ jugador.tblEquipos.tblUsuarioCreador.Email;
                    string emailTo = nombreJugador;
                    string  body = Global_Functions.getBodyHTML("~/Emails/ParticipanteAvisoConfirmacion.html");
                      body = body.Replace("<%= NombreJugador %>", nombreJugador);
                    body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                    body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                    body = body.Replace("<%= usuario %>", nombreJugador);
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
        /// Envia el correo de confirmacion de participacion al administrador de torneo o equipo.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        [NonAction]
        public bool enviarEmailParticipanteAvisoConfirmacion(schemaJugadorEquipos jugador, string nombreJugador, string nombreTorneo, string nombreEquipo)
        {
            try
            {
                // Send an email with this link
                var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
                var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

                schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
                if (siteConfig != null)
                {
                    //   string emailTo = jugador.tblTorneos.tblUserCreador.Email;
                    string emailTo = (jugador.tblEquipos.equAdminCorreo != null)? jugador.tblEquipos.equAdminCorreo: jugador.tblTorneos.tblUserCreador.Email;

                    string body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoEquipoAvisoConfirmacion.html");
                    if (nombreEquipo == null)
                        body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoAvisoConfirmacion.html");
                    
                    body = body.Replace("<%= NombreJugador %>", nombreJugador);
                    body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                    body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                    body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                    body = body.Replace("<%= UrlEnligate %>", homeUrl);

                    bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Confirmacion de participacion en torneo", body,
                                                                siteConfig.scoSenderEmail,
                                                                Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                                siteConfig.scoSenderSMTPServer,
                                                                siteConfig.scoSenderPort,
                                                                null, "", "", true, "luizhdz072@gmail.com");
                    if (mailSended)
                        return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
                throw;
               ;
            }
            return false;
        }

        /// <summary>
        /// Envia los correos de rechazo de participacion al administrador de torneo o equipo.
        /// </summary>
        /// <param name="emails">correo1@correo.com,correo2@correo.com,...</param>
        /// <returns></returns>
        [NonAction]

        public bool enviarEmailCoachAvisoRechazo(string emailCoach,string emailLiga,string emailTorneo, string nombreTorneo, string nombreEquipo)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = emailLiga + ',' + emailTorneo;

                string body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoEquipoAvisoRechazo.html");
               
                body = body.Replace("<%= NombreJugador %>", emailCoach);
                body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);



                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Rechazo de participacion en torneo", body,
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
        public bool enviarEmailParticipanteAvisoRechazo(schemaJugadorEquipos jugador, string nombreJugador, string nombreTorneo, string nombreEquipo)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo =jugador.tblEquipos.equAdminCorreo;

                string body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoEquipoAvisoRechazo.html");
                if(nombreEquipo == null)
                    body = Global_Functions.getBodyHTML("~/Emails/ParticipanteTorneoAvisoRechazo.html");
                
                body = body.Replace("<%= NombreJugador %>", nombreJugador);
                body = body.Replace("<%= NombreEquipo %>", nombreEquipo);
                body = body.Replace("<%= NombreTorneo %>", nombreTorneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);


                
                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Rechazo de participacion en torneo", body,
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

        public bool enviarEmailArbitroAvisoRechazo(string nombre,string email ,string torneo)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);
            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = email;

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroTorneoEquipoAvisoRechazo.html");
                
                body = body.Replace("<%= NombreJugador %>", nombre);
                body = body.Replace("<%= NombreTorneo %>", torneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);
                
                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Rechazo de participacion en liga", body,
                                                            siteConfig.scoSenderEmail,
                                                            Global_Functions.getDecryptPrivateKey(siteConfig.scoSenderEmailPassword, constClass.encryptionKey),
                                                            siteConfig.scoSenderSMTPServer,
                                                            siteConfig.scoSenderPort,
                                                            null, "", "", true, "luizhdz072@gmail.com");
                if (mailSended)
                    return true;
            }

            return false;
        }

        public bool eviarEmailAdminTorneoRechazo(string nombre, string email, string torneo)
        {
            // Send an email with this link
            var faqsUrl = Url.Action("FAQs", "Home", null, protocol: Request.Url.Scheme);
            var homeUrl = Url.Action("", "", null, protocol: Request.Url.Scheme);

            schemaSiteConfigs siteConfig = db.getLastSiteConfRow();
            if (siteConfig != null)
            {
                string emailTo = email;

                string body = Global_Functions.getBodyHTML("~/Emails/ArbitroTorneoEquipoAvisoRechazo.html");

                body = body.Replace("<%= NombreJugador %>", nombre);
                body = body.Replace("<%= NombreTorneo %>", torneo);
                body = body.Replace("<%= UrlFAQs %>", faqsUrl);
                body = body.Replace("<%= UrlEnligate %>", homeUrl);



                bool mailSended = Global_Functions.sendMail(emailTo, siteConfig.scoSenderDisplayEmailName, "Rechazo de participacion en torneo", body,
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

        public ActionResult Calendario()
        {
            var user = db.getUserById(User.Identity.GetUserId());
            var partido = db.getPartidosPlayer(user);

            var model = new AdminLigasController().filtros_Calendario(user);

            model.numPartidos = partido.Count;
            
            return View(model);
            //return View();
        }
        public ActionResult Calendario_ChangeLiga(int? ligId = null, int? torId = null)
        {
            var user = db.getUserById(User.Identity.GetUserId());

            var partido = db.getPartidosPlayer(user);

            var model = new AdminLigasController().filtros_Calendario(user,ligId, torId);

            return PartialView("Ligas/_CalendarioNuevo_Filtros", model);
        }
        public ActionResult _Partidos_Player(int parId)
        {
            PartidosViewModel model = new PartidosViewModel();
            var data = db.getPartidosById(parId);
            var fecha = data.parFecha_Fin;
            var result = false;
            var parEstado = data.parEstado;
            var equResultadoUno = data.equResultadoUno;
            var equResultadoDos = data.equResultadoDos;
            model.arbNombre = (data.arbNombre!=null)? data.arbNombre:(data.arbId!=null && data.arbId > 0)? db.getArbitroById(data.arbId).arbCorreo : "" ;
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
                model.canchaDireccion = cancha.lcatdomicilio+" #"+cancha.lcatNumExtInt+" "+cancha.lcatColonia+" "+cancha.lcatMunicipio+" "+cancha.lcatEstado+" , C.P: "+cancha.lcatCodigoPostal;
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

        public ActionResult MisPagos()
        {
            return View();
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

        public JsonResult ChangePassword(string passActual, string passNew)
        {
            
            var user = db.getUserById(User.Identity.GetUserId());

            if (user != null)
            {
                UserManager.RemovePassword(user.Id);
                UserManager.AddPassword(user.Id,passNew);
                return Json("success");
            }
            return Json("success");
        }
        public ActionResult _Avisos_Callback()
        {
            var usr = User.Identity.GetUserId();
            var user = db.getUserById(usr);
            var model = db.getAvisos(user.Id,user.usuRolActual);
            var i = 0;
            model.ForEach(f => f.aId = i++);
            return PartialView("Admin/_Avisos", model.OrderByDescending(o=> o.fecha_Registro));
        }
    }
}