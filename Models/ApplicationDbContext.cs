using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using sw_EnligateWeb.Engine;
using sw_EnligateWeb.Models.HelperClasses;
using System.Collections.Generic;

namespace sw_EnligateWeb.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DbConnection", throwIfV1Schema: false)
        {
            //this.Configuration.LazyLoadingEnabled = false;
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbContextInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<schemaCountry> tblCountry{ get; set; }
        public DbSet<schemaReporteBugs> tblReporteBugs { get; set; }
        public DbSet<schemaLoginHistory> tblLoginHistory { get; set; }
        public DbSet<schemaDeportes> tblDeportes { get; set; }
        public DbSet<schemaTipoTorneos> tblTipoTorneo { get; set; }
        public DbSet<schemaCiudades> tblCiudades { get; set; }
        public DbSet<schemaZonas> tblZonas { get; set; }
        public DbSet<schemaSiteConfigs> tblSiteConfigs { get; set; }

        #region Usuarios / Cuentas

        public DbSet<schemaUsersProfiles> tblUsersProfiles { get; set; }
        public DbSet<schemaUsersMisCuentas> tblUsersMisCuentas { get; set; }

        #endregion

        #region Ligas
        public DbSet<schemaLigas> tblLigas { get; set; }
        public DbSet<schemaLigaCoAdminInit> tblLigaCoAdmnInit { get; set; }
        public DbSet<schemaLigaCoAdministradores> tblLigaCoAdministradores { get; set; }
        public DbSet<schemaLigaDatosFiscales> tblLigaDatosFiscales { get; set; }
        public DbSet<schemaLigaDireccionComercial> tblLigaDireccionComercial { get; set; }
        public DbSet<schemaLigaCategorias> tblLigaCategorias { get; set; }
        public DbSet<schemaLigaCategoriasTarifasFormasPago> tblLigaCategoriasTarifasFormasPago { get; set; }
        public DbSet<schemaLigaPrincipalUsuario> tblLigaPrincipalUsuario { get; set; }
        #endregion

        #region Torneos
        public DbSet<schemaLigaCategoriasTorneos> tblLigaCategoriasTorneos { get; set; }
        public DbSet<schemaLigaCanchasTorneos> tblLigaCanchasTorneos { get; set; }
        public DbSet<schemaTorneos> tblTorneos { get; set; }
        public DbSet<schemaTorneoEstructura> tblTorneoEstructura { get; set; }
        public DbSet<schemaTorneoDireccion> tblTorneoDireccion { get; set; }
        public DbSet<schemaTorneoTarifas> tblTorneoTarifas { get; set; }
        public DbSet<schemaTorneoCoAdministradores> tblTorneoCoAdministradores { get; set; }
        public DbSet<schemaTorneoComentarios> tblTorneoComentarios { get; set; }
        #endregion

        #region Partidos
        public DbSet<schemaPartidos> tblPartidos { get; set; }
        public DbSet<schemaJuegosFutbolEstadisticasJugador> tblJuegosFutbolEstadisticasJugador { get; set; }
        #endregion
        #region Menú
        public DbSet<schemaMenus> tblMenus { get; set; }
        public DbSet<schemaSubMenus> tblSubMenus { get; set; }
        #endregion

        #region Tarifas
        public DbSet<schemaTarifas> tblTarifas { get; set; }
        public DbSet<schemaTarifasConceptos> tblTarifasConceptos { get; set; }
        public DbSet<schemaTarifasFormasPago> tblTarifasFormasPago { get; set; }
        public DbSet<schemaTarifasConceptosFormasPago> tblTarifasConceptosFormasPago { get; set; }
        public DbSet<schemaTarifasPeriodicidades> tblTarifasPeriodicidades { get; set; }
        public DbSet<schemaTarifasCfpPeriodicidades> tblTarifasCfpPeriodicidades { get; set; }
        public DbSet<schemaTarifasTiposPago> tblTarifasTiposPago { get; set; }
        public DbSet<schemaTarifasCfppTiposPago> tblTarifasCfppTiposPago { get; set; }
        public DbSet<schemaTarifasMetodosPago> tblTarifasMetodosPago { get; set; }
        public DbSet<schemaTarifasCfpptpMetodosPago> tblTarifasCfpptpMetodosPago { get; set; }
        #endregion

        #region Equipos y Jugadores

        public DbSet<schemaEquipos> tblEquipos { get; set; }
        public DbSet<schemaEquiposCoAdministradores> tblEquiposCoAdministradores { get; set; }

        public DbSet<schemaJugadores> tblJugadores { get; set; }
        public DbSet<schemaJugadorEquipos> tblJugadorEquipos { get; set; }
        #endregion

        #region Arbitros
        public DbSet<schemaArbitros> tblArbitros { get; set; }
        //public DbSet<schemaArbitrosTorneos> tblArbitrosTorneos { get; set; }
        public DbSet<schemaArbitrosLigas> tblArbitrosLigas { get; set; }

        public DbSet<schemaArbitrosPartidos> tblArbitrosPartidos { get; set; }
        #endregion
        #region Pagos
        public DbSet<schemaPagos> tblPagos { get; set; }
        public DbSet<schemaDetallesPago> tblDetallesPago { get; set; }
        //public DbSet<schemaDatosTarjeta> tblDatosTarjeta { get; set; }
        
        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<schemaLigaCoAdministradores>().HasRequired(ca => ca.tblUsuario)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaLigaPrincipalUsuario>().HasRequired(ca => ca.tblRole)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaLigaPrincipalUsuario>().HasRequired(ca => ca.tblUser)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneoDireccion>()
                        .HasKey(k => k.torId)
                        .HasRequired(r => r.tblTorneo)
                        .WithRequiredDependent(g => g.tblTorneoDireccion);

            modelBuilder.Entity<schemaTorneos>().HasRequired(r => r.tblLiga)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneos>().HasRequired(r => r.tblUserCreador)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneos>().HasOptional(r => r.tblTarifasCfppTiposPago)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneos>().HasRequired(r => r.tblTorneoEstructura)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneoTarifas>().HasRequired(ca => ca.tblTorneos)
                        .WithMany()
                        .WillCascadeOnDelete(false);

//            modelBuilder.Entity<schemaTorneoCoAdministradores>().HasRequired(ca => ca.tblUsuario)
 //                       .WithMany()
  //                      .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneoComentarios>().HasRequired(e => e.tblTorneo)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneoComentarios>().HasRequired(e => e.tblUsuarioComenta)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaTorneoComentarios>().HasOptional(j => j.tblEquipo)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaEquipos>().HasRequired(e => e.tblTorneos)
                        .WithMany()
                        .WillCascadeOnDelete(false);
            /*
            modelBuilder.Entity<schemaEquipos>().HasRequired(e => e.tblUsuarioCreador)
                        .WithMany()
                        .WillCascadeOnDelete(false);
                        */
            modelBuilder.Entity<schemaPartidos>().HasRequired(e => e.tblLigas)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaPartidos>().HasRequired(e => e.tblTorneos)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            /*modelBuilder.Entity<schemaPartidos>().HasRequired(e => e.tblArbitros)
                       .WithMany()
                       .WillCascadeOnDelete(false);*/
            modelBuilder.Entity<schemaJugadores>().HasOptional(j => j.tblUsuario)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaEquiposCoAdministradores>().HasRequired(e => e.tblEquipo)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaEquiposCoAdministradores>().HasOptional(e => e.tblUsuario)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaJugadorEquipos>().HasOptional(j => j.tblTorneos)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaJugadorEquipos>().HasOptional(j => j.tblEquipos)
                        .WithMany(j => j.listJugadores)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaArbitros>().HasOptional(j => j.tblUsuario)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaUsersProfiles>().HasRequired(j => j.tblUsers)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaUsersMisCuentas>().HasRequired(j => j.tblUserAdmin)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaUsersMisCuentas>().HasRequired(j => j.tblUser)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<schemaPagos>().HasRequired(j => j.tblUsers)
                        .WithMany()
                        .WillCascadeOnDelete(false);
            /*modelBuilder.Entity<schemaPagos>().HasOptional(j => j.tblLiga)
                       .WithMany()
                       .WillCascadeOnDelete(false);
                       */
            modelBuilder.Entity<schemaDetallesPago>().HasRequired(j => j.tblPagos)
                        .WithMany()
                        .WillCascadeOnDelete(false);
            /*modelBuilder.Entity<schemaJugadorEquipos>().HasRequired(j => j.tblEquipos)
                        .WithMany()
                        .WillCascadeOnDelete(false);*/

            /*modelBuilder.Entity<schemaDatosTarjeta>().HasRequired(j => j.tblUsers)
                        .WithMany()
                        .WillCascadeOnDelete(false);
            */

            base.OnModelCreating(modelBuilder);
        }
    }

    //DropCreateDatabaseAlways
    //DropCreateDatabaseIfModelChanges
    public class ApplicationDbContextInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            seedRoles(context);
            seedUsuarios(context);
            seedDeportes(context);
            seedTiposTorneos(context);
            seedCiudades(context);
            seedZonas(context);
            seedSiteConfig(context);

            base.Seed(context);
        }

        protected void seedRoles(ApplicationDbContext context)
        {
            //ROLES
            var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            role.Create(new IdentityRole(constClass.rolAdmin));
            role.Create(new IdentityRole(constClass.rolOwners));
            role.Create(new IdentityRole(constClass.rolReferee));
            role.Create(new IdentityRole(constClass.rolCoach));
            role.Create(new IdentityRole(constClass.rolPlayer));
            role.Create(new IdentityRole(constClass.rolVisitor));
        }

        protected void seedUsuarios(ApplicationDbContext context)
        {
            //USUARIOS
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser user = new ApplicationUser
            {
                UserName = "kikismach@hotmail.com",
                Email = "kikismach@hotmail.com",
                PhoneNumber = "",
                EmailConfirmed = true,
                usuEstatus = true
            };

            userManager.Create(user, "1qaz1qaz");
            userManager.AddToRole(user.Id, constClass.rolPlayer);
        }

        protected void seedDeportes(ApplicationDbContext context)
        {
            //DEPORTES
            string[] arrDeportes = new string[] { 
                "Futbol", "01soccer_negro.png",
                "Voleibol", "05volleyball_negro.png",
                "Beisbol", "04baseball_negro.png",
                "Futbol Americano", "02football_negro.png",
                "Basquetbol", "03basketball_negro.png",
                "Corredores", "06_zapato_negro.png"
            };
            schemaDeportes dep;
            List<schemaDeportes> lisDep = new List<schemaDeportes>();
            for (int i = 0; i < arrDeportes.Length; i++)
            {
                dep = new schemaDeportes();
                dep.depNombre = arrDeportes[i];
                dep.depIcono = arrDeportes[++i];
                dep.depPrioridad = i / 2 + 1;
                lisDep.Add(dep);
            }
            context.tblDeportes.AddRange(lisDep);
        }

        protected void seedTiposTorneos(ApplicationDbContext context)
        {
            //TIPOS DE TORNEO
            string[] arrTipoTorneo = new string[] { 
                "Varonil", 
                "Femenil",
                "Infantil",
                "Mixto"
            };
            schemaTipoTorneos tto;
            List<schemaTipoTorneos> lisTto = new List<schemaTipoTorneos>();
            for (int i = 0; i < arrTipoTorneo.Length; i++)
            {
                tto = new schemaTipoTorneos();
                tto.ttoNombre = arrTipoTorneo[i];
                lisTto.Add(tto);
            }
            context.tblTipoTorneo.AddRange(lisTto);
        }

        protected void seedCiudades(ApplicationDbContext context)
        {
            //CIUDADES
            schemaCiudades ciu = new schemaCiudades() { ciuNombre = "Monterrey" };
            context.tblCiudades.Add(ciu);
        }

        protected void seedZonas(ApplicationDbContext context)
        {
            //ZONAS
            string[] arrZonas = new string[] { 
                "Centro", 
                "Oriente",
                "Poniente",
                "Norte",
                "Sur"
            };
            schemaZonas zon;
            List<schemaZonas> lisZon = new List<schemaZonas>();
            for (int i = 0; i < arrZonas.Length; i++)
            {
                zon = new schemaZonas();
                zon.zonZona = arrZonas[i];
                lisZon.Add(zon);
            }
            context.tblZonas.AddRange(lisZon);   
        }

        protected void seedSiteConfig (ApplicationDbContext context)
        {
            schemaSiteConfigs sconf = new schemaSiteConfigs();
            sconf.scoAppName = "ENLIGATE_WEB";
            sconf.scoCompanyName = "ENLIGATE";
            sconf.scoSenderDisplayEmailName = "Notificacion " + sconf.scoCompanyName;
            sconf.scoSenderEmail = "enligate@rmachain.com";
            sconf.scoSenderEmailPassword = Global_Functions.getEncriptPrivateKey("1qaz1qaz", constClass.encryptionKey);
            sconf.scoSenderSMTPServer = "mail.rmachain.com";
            sconf.scoSenderPort = "26";

            context.tblSiteConfigs.Add(sconf);
        }
    }
}

//----------AddRoleToUser

//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.AspNet.Identity.EntityFramework;

//#region Constructores

//        private ApplicationSignInManager _signInManager;
//        private ApplicationUserManager _userManager;

//        public HomeController()
//        {
            
//        }

//        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//        }

//        public ApplicationSignInManager SignInManager
//        {
//            get
//            {
//                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            }
//            private set
//            {
//                _signInManager = value;
//            }
//        }

//        public ApplicationUserManager UserManager
//        {
//            get
//            {
//                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            }
//            private set
//            {
//                _userManager = value;
//            }
//        }

//        #endregion

//var usr = db.getUserByUserName(System.Web.HttpContext.Current.User.Identity.Name);
//UserManager.AddToRole(usr.Id, constClass.rolAdmin);
//UserManager.AddToRole(usr.Id, constClass.rolCoach);
//UserManager.AddToRole(usr.Id, constClass.rolReferee);