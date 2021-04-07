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

            bundles.Add(new ScriptBundle("~/bundles/plotlyjs").Include(
                        "~/Scripts/plotly/plotly.js"));

            bundles.Add(new ScriptBundle("~/bundles/slider").Include(
                        "~/Scripts/jQAllRangeSliders-withRuler-min.js",
                        "~/Scripts/jquery.mousewheel.js"));

            bundles.Add(new ScriptBundle("~/bundles/alertify").Include(
                        "~/Scripts/alertify.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/AccordanceController").Include(
                        "~/Scripts/Angular/Controllers/accordance.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchGeneticImportFromGenBankSearchFileController").Include(
              "~/Scripts/Angular/Controllers/batchGeneticImportFromGenBankSearchFile.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchMusicImportController").Include(
                "~/Scripts/Angular/Controllers/batchMusicImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchPoemsImportController").Include(
                "~/Scripts/Angular/Controllers/batchPoemsImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchSequenceImportController").Include(
                "~/Scripts/Angular/Controllers/batchSequenceImport.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BatchSequenceImportResultController").Include(
                        "~/Scripts/Angular/Controllers/batchSequenceImportResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/BuildingsSimilarityController").Include(
                        "~/Scripts/Angular/Controllers/buildingsSimilarity.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CalculationController").Include(
                        "~/Scripts/Angular/Controllers/calculation.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/calculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/ChartsController").Include(
                "~/Scripts/Angular/Directives/tableParse.js",
                "~/Scripts/Angular/Controllers/charts.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/CustomCalculationController").Include(
                        "~/Scripts/Angular/Controllers/customCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/FmotifsDictionaryController").Include(
                "~/Scripts/Angular/Controllers/fmotifsDictionary.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/GenesImportController").Include(
                        "~/Scripts/Angular/Controllers/genesImport.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/LocalCalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/localCalculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/MatterEditController").Include(
                        "~/Scripts/Angular/Controllers/matterEdit.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/MatterSequenceCreateController").Include(
                        "~/Scripts/Angular/Controllers/matterSequenceCreate.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/MusicFilesResultController").Include(
                        "~/Scripts/Angular/Controllers/musicFilesResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/midijs").Include(
                        "~/Scripts/midijs/inc/shim/Base64.js",
                        "~/Scripts/midijs/inc/shim/Base64binary.js",
                        "~/Scripts/midijs/inc/shim/WebAudioAPI.js",
                        "~/Scripts/midijs/js/midi/audioDetect.js",
                        "~/Scripts/midijs/js/midi/gm.js",
                        "~/Scripts/midijs/js/midi/loader.js",
                        "~/Scripts/midijs/js/midi/plugin.audiotag.js",
                        "~/Scripts/midijs/js/midi/plugin.webaudio.js",
                        "~/Scripts/midijs/js/midi/plugin.webmidi.js",
                        "~/Scripts/midijs/js/util/dom_request_xhr.js",
                        "~/Scripts/midijs/js/util/dom_request_script.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderCalculationController").Include(
                "~/Scripts/Angular/Controllers/orderCalculation.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformationCharacteristicsDynamicVisualizationResultController").Include(
                "~/Scripts/Angular/Controllers/orderTransformationCharacteristicsDynamicVisualizationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformationResultController").Include(
                "~/Scripts/Angular/Controllers/orderTransformationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformationVisualizationController").Include(
                "~/Scripts/Angular/Controllers/orderTransformationVisualization.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformationVisualizationResultController").Include(
                "~/Scripts/Angular/Controllers/orderTransformationVisualizationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrderTransformerController").Include(
                "~/Scripts/Angular/Controllers/orderTransformer.js",
                "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequenceGroupsController").Include(
                "~/Scripts/Angular/Controllers/sequenceGroups.js",
                "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequencePredictionResultController").Include(
                "~/Scripts/Angular/Controllers/sequencePredictionResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequencesAlignmentController").Include(
                        "~/Scripts/Angular/Controllers/sequencesAlignment.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequencesOrderDistributionController").Include(
                "~/Scripts/Angular/Controllers/sequencesOrderDistribution.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SequencesOrderDistributionResultController").Include(
                "~/Scripts/Angular/Controllers/sequencesOrderDistributionResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrdersIntervalsDistributionsAccordanceController").Include(
                "~/Scripts/Angular/Controllers/ordersIntervalsDistributionsAccordance.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/OrdersIntervalsDistributionsAccordanceResultController").Include(
                "~/Scripts/Angular/Controllers/ordersIntervalsDistributionsAccordanceResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/IntervalsCharacteristicsDistributionController").Include(
                "~/Scripts/Angular/Controllers/intervalsCharacteristicsDistribution.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/IntervalsCharacteristicsDistributionResultController").Include(
                "~/Scripts/Angular/Controllers/intervalsCharacteristicsDistributionResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesCalculationController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesCalculation.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesCalculationResultController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesCalculationResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesComparerResultController").Include(
                "~/Scripts/Angular/Controllers/subsequencesComparerResult.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesDistributionController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesDistribution.js",
                        "~/Scripts/Angular/Components/mattersTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers/SubsequencesDistributionResultController").Include(
                        "~/Scripts/Angular/Controllers/subsequencesDistributionResult.js"));


            bundles.Add(new ScriptBundle("~/bundles/controllers/TaskManagerController").Include(
                        "~/Scripts/jquery.signalR-{version}.js",
                        "~/Scripts/Angular/Controllers/taskManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/scrollJumper").Include(
                        "~/Scripts/Angular/Components/scrollJumper.js"));

            bundles.Add(new ScriptBundle("~/bundles/Characteristic").Include(
                        "~/Scripts/Angular/Components/characteristic.js"));

            bundles.Add(new ScriptBundle("~/bundles/Characteristics").Include(
                        "~/Scripts/Angular/Components/characteristics.js"));

            bundles.Add(new ScriptBundle("~/bundles/CharacteristicsWithoutNotation").Include(
                "~/Scripts/Angular/Components/CharacteristicsWithoutNotation.js"));

            bundles.Add(new ScriptBundle("~/bundles/loadingWindow").Include(
                        "~/Scripts/Angular/Components/loadingWindow.js"));

            bundles.Add(new ScriptBundle("~/bundles/orderTransformations").Include(
                       "~/Scripts/Angular/Components/orderTransformations.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/site.css",
                        "~/Content/scrollUpDown.css",
                        "~/Content/alertify.min.css",
                        "~/Content/themes/alertify-bootstrap.min.css"
                        ));

            bundles.Add(new StyleBundle("~/Content/genesMap").Include(
                        "~/Content/GenesMap.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
