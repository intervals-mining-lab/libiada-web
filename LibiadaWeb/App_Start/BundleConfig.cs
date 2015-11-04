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

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js", 
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs").Include(
                     "~/Scripts/angular.js",
                     "~/Scripts/smart-table.js"));

            bundles.Add(new ScriptBundle("~/bundles/dc.js").Include(
                        "~/Scripts/d3/d3.js",
                        "~/Scripts/CrossFilter/crossfilter.js",
                        "~/Scripts/DC/dc.js"));

            bundles.Add(new ScriptBundle("~/bundles/highCharts").Include(
                        "~/Scripts/Highcharts-2.3.5/highcharts.src.js"));

            bundles.Add(new ScriptBundle("~/bundles/slider").Include(
                        "~/Scripts/jQAllRangeSliders-withRuler-min.js",
                        "~/Scripts/jquery.mousewheel.js"));

            bundles.Add(new ScriptBundle("~/bundles/functions").Include(
                        "~/Scripts/Angular/functions.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/accordance").Include(
                        "~/Scripts/Angular/Controllers/accordance.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/sequencesAlignment").Include(
                        "~/Scripts/Angular/Controllers/sequencesAlignment.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/buildingsSimilarity").Include(
                        "~/Scripts/Angular/Controllers/buildingsSimilarity.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/calculation").Include(
                        "~/Scripts/Angular/Controllers/calculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/calculationResult").Include(
                        "~/Scripts/Angular/Controllers/calculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/customCalculation").Include(
                        "~/Scripts/Angular/Controllers/customCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/subsequencesCalculation").Include(
                        "~/Scripts/Angular/Controllers/subsequencesCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/subsequencesDistribution").Include(
                        "~/Scripts/Angular/Controllers/subsequencesDistribution.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/genesImport").Include(
                        "~/Scripts/Angular/Controllers/genesImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/matterSequenceCreate").Include(
                        "~/Scripts/Angular/Controllers/matterSequenceCreate.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/taskManager").Include(
                        "~/Scripts/Angular/Controllers/taskManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/charts").Include(
                        "~/Scripts/Angular/Directives/tableParse.js",
                        "~/Scripts/Angular/Controllers/charts.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css", 
                      "~/Content/site.css",
                      "~/Content/dc.css"));

            bundles.Add(new StyleBundle("~/Content/slider").Include(
                "~/Content/themes/JQAllRange/iThing.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
