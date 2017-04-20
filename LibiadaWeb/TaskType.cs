using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb
{
    public enum TaskType : short
    {
        AccordanceCalculation = 1,
        BuildingsSimilarity = 2,
        Calculation = 3,
        Clusterization = 4,
        CongenericCalculation = 5,
        CustomSequenceCalculation = 6,
        CustomSequenceOrderTransformationCalculation = 7,
        FilteredSubsequenceCalculation = 8,
        LocalCalculation = 9,
        OrderTransformationCalculation = 10,
        RelationCalculation = 11,
        SequencesAlignment = 12,
        SubsequencesCalculation = 13,
        SubsequencesComparer = 14,
        SubsequencesDistribution = 15,
        SubsequencesSimilarity = 16,
        AttributesCheck = 17,
        BatchGenesImport = 18,
        BatchSequenceImport = 19,
        CustomSequenceOrderTransformer = 20,
        GenesImport = 21,
        OrderTransformer = 22,
        SequenceCheck = 23,
        SequencesMatters = 24
    }
}