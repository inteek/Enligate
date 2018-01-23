using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sw_EnligateWeb.Models.HelperClasses
{
    public class constClass
    {
        //Roles
        public const string rolAdmin = "ADMINISTRADOR";
        public const string rolOwners = "ADMINISTRADOR DE LIGAS";
        public const string rolAdminTorneos = "ADMINISTRADOR DE TORNEOS";
        public const string rolReferee = "ARBITRO";
        public const string rolCoach = "COACH";
        public const string rolPlayer = "JUGADOR";
        public const string rolVisitor = "VISITANTE";

        //Encryption Key
        public const string encryptionKey = "Crypt0K3y_swEnligate";

        //Model States Keys
        public const string error = "ERROR";
        public const string info = "INFO";
        public const string success = "SUCCESS";

        //Formats
        public const string formatMoney = "$ #,##0.00";
        public const string formatDateTime = "dd/MM/yyyy HH:mm:ss";
        public const string formatDateOnly = "dd/MM/yyyy";
        public const string formatDateToken = "ddMMyyyyhhmmssfff";

        //URL Paths
        public const string urlProfileImages = "~/Images/Profiles";
        public const string urlLeaguesImages = "~/Images/Leagues";
        public const string urlTorneosImagenes = "~/Images/Torneos";
        public const string urlEquiposImagenes = "~/Images/Equipos";

        //Images allowed extensions
        public static readonly string[] imgLeaguesAllowedExtensions = new string[] {".JPG", ".JPEG", ".PNG", ".BMP", ".GIF"};

        //Data annotation messages
        public const string requiredMsg = "Campo obligatorio.";
        public const string maxLengthMsg = "Máximo de caracteres permitidos({1})";
        public const string emailAddressMsg = "Ingresa una cuenta de correo válida.";

        //Tipos de torneo
        public const string torTipoInterno = "INTERNO";
        public const string torTipoCoaching = "COACHING";

        //Color Deporte
        public const string colFutbol = "#27ae60";
        public const string colBeisbol = "#e67e22";
        public const string colCarreras = "#f1c40f";
        public const string colBasquetbol = "#c0392b";
        public const string colVoleibol = "#825a2c";
        public const string colAmericano = "#f472d0";
        public const string colDefault = "#3498db";
    }

}