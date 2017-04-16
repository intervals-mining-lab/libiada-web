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
                        "~/Scripts/Angular/functions.js",
                        "~/Scripts/angular.js",
                        "~/Scripts/smart-table.js"));

            bundles.Add(new ScriptBundle("~/bundles/d3js").Include(
                        "~/Scripts/d3/d3.js"));

            bundles.Add(new ScriptBundle("~/bundles/slider").Include(
                        "~/Scripts/jQAllRangeSliders-withRuler-min.js",
                        "~/Scripts/jquery.mousewheel.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/AccordanceController").Include(
                        "~/Scripts/Angular/Controllers/accordance.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchSequenceImportController").Include(
                        "~/Scripts/Angular/Controllers/batchSequenceImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchSequenceImportResultController").Include(
                "~/Scripts/Angular/Controllers/batchSequenceImportResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BuildingsSimilarityController").Include(
                        "~/Scripts/Angular/Controllers/buildingsSimilarity.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CalculationController").Include(
                        "~/Scripts/Angular/Controllers/calculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/calculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CustomCalculationController").Include(
                        "~/Scripts/Angular/Controllers/customCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/GenesImportController").Include(
                        "~/Scripts/Angular/Controllers/genesImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/LocalCalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/localCalculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/MatterEditController").Include(
                        "~/Scripts/Angular/Controllers/matterEdit.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/MatterSequenceCreateController").Include(
                        "~/Scripts/Angular/Controllers/matterSequenceCreate.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformerController").Include(
                        "~/Scripts/Angular/Controllers/orderTransformer.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequencesAlignmentController").Include(
                        "~/Scripts/Angular/Controllers/sequencesAlignment.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesCalculationController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesCalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesCalculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesDistributionController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesDistribution.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesComparerResultController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesComparerResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesDistributionResultController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesDistributionResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/TaskManagerController").Include(
                        "~/Scripts/jquery.signalR-{version}.js",
                        "~/Scripts/Angular/Controllers/taskManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/ChartsController").Include(
                        "~/Scripts/Angular/Directives/tableParse.js",
                        "~/Scripts/Angular/Controllers/charts.js"));

            bundles.Add(new ScriptBundle("~/bundles/scrollJumper").Include(
                        "~/Scripts/scrollJumper.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/site.css",
                        "~/Content/scrollUpDown.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
