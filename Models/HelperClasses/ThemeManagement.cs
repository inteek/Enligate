using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace sw_EnligateWeb.Models.HelperClasses
{

    /// <summary>
    /// Its used for BundleConfig to get the path of the theme.
    /// </summary>
    public class ThemeManagement
    {
        protected List<string> pageThemesList = WebConfigurationManager.AppSettings["pageThemes"].Split(',').ToList();
        protected const string stylePath =  "/Style.css";
        public string themePath;

        /// <summary>
        /// Get the list of the current themes configured in Web.config separated by ","
        /// </summary>
        /// <returns></returns>
        public List<string> getPageThemesList()
        {
            return pageThemesList;
        }

        /// <summary>
        /// Return the path of the theme to load.
        /// </summary>
        /// <param name="themeListPos"></param>
        /// <returns></returns>
        public string getThemePath(string themeName)
        {
            return "~/Content/Themes/" + themeName + stylePath;
        }
    }
}