using System.Web;
using System.Web.Optimization;

namespace LibiadaWeb
{
    public class BundleConfig
    {
        // Дополнительные сведения о Bundling см. по адресу http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/packed.js"));

            bundles.Add(new ScriptBundle("~/bundles/Slider").Include(
                        "~/Scripts/Slider/jQRangeSlider-min.js",
                        "~/Scripts/Slider/jquery.mousewheel.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/menu").Include(
                        "~/Scripts/tinydropdown.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryDataTables").Include(
                        "~/Scripts/DataTables-1.9.4/media/js/jquery.dataTables.js",
                        "~/Scripts/datatables.plugins.js"));

            bundles.Add(new ScriptBundle("~/bundles/HighCharts").Include(
                       "~/Scripts/Highcharts-2.3.5/highcharts.src.js"));


            bundles.Add(new ScriptBundle("~/bundles/Procedures").Include(
                      "~/Scripts/procs.js"));
           

            // Используйте версию Modernizr для разработчиков, чтобы учиться работать. Когда вы будете готовы перейти к работе,
            // используйте средство построения на сайте http://modernizr.com, чтобы выбрать только нужные тесты.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/JQAllRange").Include("~/Content/themes/JQAllRange/iThing.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css",
                        "~/Content/DataTables-1.9.4/media/css/jquery.dataTables_themeroller.css",
                        "~/Content/themes/base/tinydropdown.css"
                        ));
        }
    }
}