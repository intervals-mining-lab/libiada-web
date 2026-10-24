import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface SubsequenceData {
    Attributes: number[],
    CharacteristicsValues: number[],
    FeatureId: number,
    Id: number,
    Lengths: number[],
    Starts: number[],
    Partial: boolean,
    RemoteId?: string,
}

// Interface for data passed to the controller from server on page load
interface SubsequencesSimilarityResultData {
    firstSequenceName: string;
    secondSequenceName: string;
    characteristicName: string;
    similarity: number;
    firstSequenceSimilarity: number;
    secondSequenceSimilarity: number;
    similarSubsequences: { Item1: number, Item2: number }[];
    firstSequenceSubsequences: SubsequenceData[];
    secondSequenceSubsequences: SubsequenceData[];
    features: { [name: number]: string };
    attributes: { [name: number]: string };
    firstSequenceAttributes: { AttributeId: number, Value: string }[][];
    secondSequenceAttributes: { AttributeId: number, Value: string }[][];
}

/**
 * Interface for the controller's scope
 */
interface SubsequencesSimilarityResultScope 
    extends ng.IScope, SubsequencesSimilarityResultData, ResultsLoadingScreenData {
}

// Controller for subsequences similarity result visualization
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.ngOnInit();
    }

    // Initializes the Angular controller
    private ngOnInit(): void {
        const subsequencesSimilarityResult = ($scope: SubsequencesSimilarityResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<SubsequencesSimilarityResultData>(
                $http, $scope, "Loading subsequences data", "Failed loading subsequences data");
        };

        // Register controller with Angular
        angular.module("libiada")
            .controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", subsequencesSimilarityResult]);
    }
}

// Wrapper function for backward compatibility
export default function SubsequencesSimilarityResultController(): SubsequencesSimilarityResultHandler {
    return new SubsequencesSimilarityResultHandler();
}
