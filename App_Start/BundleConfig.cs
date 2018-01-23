using System.Web;
using System.Web.Optimization;
using sw_EnligateWeb.Models.HelperClasses;

namespace sw_EnligateWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/banwire_checkout.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryAjax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                        "~/Scripts/jquery-ui*"));
            bundles.Add(new ScriptBundle("~/bundles/fullcalendarjs").Include(
                      "~/Scripts/moment.min.js",
                      "~/Scripts/fullcalendar.min.js"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            

            ThemeManagement thMgmt = new ThemeManagement();
            var list = thMgmt.getPageThemesList();

            foreach (string theme in thMgmt.getPageThemesList())
            {
                string bundleName = string.Format("~/Theme/{0}", theme);
                bundles.Add(new StyleBundle(bundleName).Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery-ui-accordion.css",
                      "~/Content/Site.css",
                      thMgmt.getThemePath(theme)));
            }

            //Calendar css file
            bundles.Add(new StyleBundle("~/Content/fullcalendarcss").Include(
                     "~/Content/fullcalendar.min.css",
                     "~/Content/c3.css"
                     ));
        }
    }
}
