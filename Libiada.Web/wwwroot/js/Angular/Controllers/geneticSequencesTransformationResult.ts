import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData, SequenceImportResult } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface GeneticSequencesTransformationResultData {
    Results: SequenceImportResult[];
}

/**
 * Interface for the controller's scope
 */
interface GeneticSequencesTransformationResultScope 
    extends ng.IScope, GeneticSequencesTransformationResultData, ResultsLoadingScreenData {
}

class GeneticSequencesTransformationResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const geneticSequencesTransformationResult = ($scope: GeneticSequencesTransformationResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<GeneticSequencesTransformationResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("GeneticSequencesTransformationResultCtrl", ["$scope", "$http", geneticSequencesTransformationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of genetic sequences transformation result handler
 */
export default function GeneticSequencesTransformationResultController(): GeneticSequencesTransformationResultHandler {
    return new GeneticSequencesTransformationResultHandler();
}
