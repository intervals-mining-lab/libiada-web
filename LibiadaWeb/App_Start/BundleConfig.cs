namespace LibiadaWeb
{
    using System.Web.Optimization;

    /// <summary>
    /// The bundle config.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// The register bundles.
        /// </summary>
        /// <param name="bundles">
        /// The bundles.
        /// </param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js", 
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs").Include(
                     "~/Scripts/angular.js",
                     "~/Scripts/smart-table.debug.js"));

            bundles.Add(new ScriptBundle("~/bundles/functions").Include(
                        "~/Scripts/Angular/functions.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/accordance").Include(
                        "~/Scripts/Angular/Controllers/accordance.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/genesAlignment").Include(
                        "~/Scripts/Angular/Controllers/genesAlignment.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/buildingsSimilarity").Include(
                        "~/Scripts/Angular/Controllers/buildingsSimilarity.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/calculation").Include(
                        "~/Scripts/Angular/Controllers/calculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/customCalculation").Include(
                        "~/Scripts/Angular/Controllers/customCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/genesCalculation").Include(
                        "~/Scripts/Angular/Controllers/genesCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/genesDistribution").Include(
                        "~/Scripts/Angular/Controllers/genesDistribution.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/genesImport").Include(
                        "~/Scripts/Angular/Controllers/genesImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/matterSequenceCreate").Include(
                        "~/Scripts/Angular/Controllers/matterSequenceCreate.js"));

            bundles.Add(new ScriptBundle("~/bundles/highCharts").Include(
                        "~/Scripts/Highcharts-2.3.5/highcharts.src.js"));

            bundles.Add(new ScriptBundle("~/bundles/slider").Include(
                        "~/Scripts/jQAllRangeSliders-withRuler-min.js",
                        "~/Scripts/jquery.mousewheel.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css", 
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/slider").Include(
                "~/Content/themes/JQAllRange/iThing.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
