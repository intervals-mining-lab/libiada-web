﻿namespace LibiadaWeb.Tasks
{
    using System.ComponentModel.DataAnnotations;

    using LibiadaWeb.Attributes;
    using LibiadaWeb.Controllers.Calculators;
    using LibiadaWeb.Controllers.Sequences;

    /// <summary>
    /// The task type.
    /// </summary>
    public enum TaskType : byte
    {
        /// <summary>
        /// The accordance calculation.
        /// </summary>
        [Display(Name = "Accordance calculation")]
        [TaskClass(typeof(AccordanceCalculationController))]
        AccordanceCalculation = 1,

        /// <summary>
        /// The calculation.
        /// </summary>
        [Display(Name = "Characteristics calculation")]
        [TaskClass(typeof(CalculationController))]
        Calculation = 2,

        /// <summary>
        /// The clusterization.
        /// </summary>
        [Display(Name = "Clusterization")]
        [TaskClass(typeof(ClusterizationController))]
        Clusterization = 3,

        /// <summary>
        /// The congeneric calculation.
        /// </summary>
        [Display(Name = "Congeneric calculation")]
        [TaskClass(typeof(CongenericCalculationController))]
        CongenericCalculation = 4,

        /// <summary>
        /// The custom sequence calculation.
        /// </summary>
        [Display(Name = "Custom sequence calculation")]
        [TaskClass(typeof(CustomSequenceCalculationController))]
        CustomSequenceCalculation = 5,

        /// <summary>
        /// The custom sequence order transformation calculation.
        /// </summary>
        [Display(Name = "Custom sequences order transformation/derivative characteristics calculation")]
        [TaskClass(typeof(CustomSequenceOrderTransformationCalculationController))]
        CustomSequenceOrderTransformationCalculation = 6,

        /// <summary>
        /// Matter creation and sequence import.
        /// </summary>
        [Display(Name = "Music files processing")]
        [TaskClass(typeof(MusicFilesController))]
        MusicFiles = 7,

        /// <summary>
        /// The local calculation.
        /// </summary>
        [Display(Name = "Local calculation")]
        [TaskClass(typeof(LocalCalculationController))]
        LocalCalculation = 8,

        /// <summary>
        /// The order transformation calculation.
        /// </summary>
        [Display(Name = "Order transformation/derivative characteristics calculation")]
        [TaskClass(typeof(OrderTransformationCalculationController))]
        OrderTransformationCalculation = 9,

        /// <summary>
        /// The relation calculation.
        /// </summary>
        [Display(Name = "Relation calculation")]
        [TaskClass(typeof(RelationCalculationController))]
        RelationCalculation = 10,

        /// <summary>
        /// The sequences alignment.
        /// </summary>
        [Display(Name = "Sequences alignment")]
        [TaskClass(typeof(SequencesAlignmentController))]
        SequencesAlignment = 11,

        /// <summary>
        /// The subsequences calculation.
        /// </summary>
        [Display(Name = "Subsequences characteristics calculation")]
        [TaskClass(typeof(SubsequencesCalculationController))]
        SubsequencesCalculation = 12,

        /// <summary>
        /// The subsequences comparer.
        /// </summary>
        [Display(Name = "Subsequences similarity matrix")]
        [TaskClass(typeof(SubsequencesComparerController))]
        SubsequencesComparer = 13,

        /// <summary>
        /// The subsequences distribution.
        /// </summary>
        [Display(Name = "Map of genes")]
        [TaskClass(typeof(SubsequencesDistributionController))]
        SubsequencesDistribution = 14,

        /// <summary>
        /// The subsequences similarity.
        /// </summary>
        [Display(Name = "Subsequences similarity")]
        [TaskClass(typeof(SubsequencesSimilarityController))]
        SubsequencesSimilarity = 15,

        /// <summary>
        /// Calculates distribution of sequences by order.
        /// </summary>
        [Display(Name = "Distribution of sequences by order")]
        [TaskClass(typeof(SequencesOrderDistributionController))]
        SequencesOrderDistribution = 16,

        /// <summary>
        /// The batch genes import.
        /// </summary>
        [Display(Name = "Batch genes import")]
        [TaskClass(typeof(BatchGenesImportController))]
        BatchGenesImport = 17,

        /// <summary>
        /// The batch sequence import.
        /// </summary>
        [Display(Name = "Batch sequences import")]
        [TaskClass(typeof(BatchSequenceImportController))]
        BatchSequenceImport = 18,

        /// <summary>
        /// The custom sequence order transformer.
        /// </summary>
        [Display(Name = "Custom sequences order transformation")]
        [TaskClass(typeof(CustomSequenceOrderTransformerController))]
        CustomSequenceOrderTransformer = 19,

        /// <summary>
        /// The genes import.
        /// </summary>
        [Display(Name = "Genes import")]
        [TaskClass(typeof(GenesImportController))]
        GenesImport = 20,

        /// <summary>
        /// The order transformer.
        /// </summary>
        [Display(Name = "Order transformation")]
        [TaskClass(typeof(OrderTransformerController))]
        OrderTransformer = 21,

        /// <summary>
        /// Checks if sequence in database equals one in file.
        /// </summary>
        [Display(Name = "Sequence check")]
        [TaskClass(typeof(SequenceCheckController))]
        SequenceCheck = 22,

        /// <summary>
        /// Sequences import for existing matter.
        /// </summary>
        [Display(Name = "Sequence upload")]
        [TaskClass(typeof(CommonSequencesController))]
        CommonSequences = 23,

        /// <summary>
        /// Matter creation and sequence import.
        /// </summary>
        [Display(Name = "Sequence import")]
        [TaskClass(typeof(MattersController))]
        Matters = 24,

        /// <summary>
        /// The sequence prediction.
        /// </summary>
        [Display(Name = "Sequence prediction")]
        [TaskClass(typeof(SequencePredictionController))]
        SequencePrediction = 25,

        /// <summary>
        /// Batch poems import.
        /// </summary>
        [Display(Name = "Batch poems import")]
        [TaskClass(typeof(BatchPoemsImportController))]
        BatchPoemsImport = 26,

        /// <summary>
        /// Calculates distribution of sequences by order.
        /// </summary>
        [Display(Name = "Order calculation")]
        [TaskClass(typeof(OrderCalculationController))]
        OrderCalculation = 27,

        /// <summary>
        /// Cflcelftes order transformations convergence.
        /// </summary>
        [Display(Name = "Order transformation convergence")]
        [TaskClass(typeof(OrderTransformationConvergenceController))]
        OrderTransformationConvergence = 28,

        /// <summary>
        /// Batch music import.
        /// </summary>
        [Display(Name = "Batch music import")]
        [TaskClass(typeof(BatchMusicImportController))]
        BatchMusicImport = 29,

        /// <summary>
        /// Visualizes order transformations.
        /// </summary>
        [Display(Name = "Order transformation visualization")]
        [TaskClass(typeof(OrderTransformationVisualizationController))]
        OrderTransformationVisualization = 30,

        /// <summary>
        /// Fmotifs dictionary.
        /// </summary>
        [Display(Name = "Fmotifs dictionary")]
        [TaskClass(typeof(FmotifsDictionaryController))]
        FmotifsDictionary = 31,

        /// <summary>
        /// Calculates dynamic visualization of order transformations characteristics.
        /// </summary>
        [Display(Name = "Order transformation characteristics dynamic visualization")]
        [TaskClass(typeof(OrderTransformationCharacteristicsDynamicVisualizationController))]
        OrderTransformationCharacteristicsDynamicVisualization = 32,

        /// <summary>
        /// Calculates accordance of orders by intervals distributions.
        /// </summary>
        [Display(Name = "Calculate accordance of orders by intervals distributions")]
        [TaskClass(typeof(OrdersIntervalsDistributionsAccordanceController))]
        OrdersIntervalsDistributionsAccordance = 33,

        /// <summary>
        /// Calculates characteristics of intervals distributions.
        /// </summary>
        [Display(Name = "Calculate characteristics of intervals distributions")]
        [TaskClass(typeof(IntervalsCharacteristicsDistributionController))]
        IntervalsCharacteristicsDistribution = 34,

        /// <summary>
        /// Batch images import.
        /// </summary>
        [Display(Name = "Batch images import")]
        [TaskClass(typeof(BatchImagesImportController))]
        BatchImagesImport = 35,

        /// Imports genetic sequences and their annotations
        /// using genbank search results file.
        /// </summary>
        [Display(Name = "Batch genetic import from GenBank search file")]
        [TaskClass(typeof(BatchGeneticImportFromGenBankSearchFileController))]
        BatchGeneticImportFromGenBankSearchFile = 36,

        /// <summary>
        /// Imports genetic sequences and their annotations
        /// using genbank search results query.
        /// </summary>
        [Display(Name = "Batch genetic import from GenBank search query")]
        [TaskClass(typeof(BatchGeneticImportFromGenBankSearchQueryController))]
        BatchGeneticImportFromGenBankSearchQuery = 37,

        /// <summary>
        /// Searches genbank nuccore database as in
        /// <see cref="TaskType.BatchGeneticImportFromGenBankSearchQuery"/>
        /// and displays results in tabular form.
        /// </summary>
        [Display(Name = "Ncbi nuccore search")]
        [TaskClass(typeof(NcbiNuccoreSearchController))]
        NcbiNuccoreSearch = 38,

        /// <summary>
        /// Checks if there is GenBank accession versions update 
        /// and displays results in tabular form.
        /// </summary>
        [Display(Name = "GenBank accession versions update check")]
        [TaskClass(typeof(GenBankAccessionVersionUpdateCheckerController))]
        GenBankAccessionVersionUpdateChecker = 39,

        /// <summary>
        /// Concatenates sequences.
        /// </summary>
        [Display(Name = "Sequence concatenator")]
        [TaskClass(typeof(SequenceConcatenatorController))]
        SequenceConcatenator = 40
    }
}
