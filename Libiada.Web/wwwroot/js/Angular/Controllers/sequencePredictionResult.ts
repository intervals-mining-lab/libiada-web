import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

interface PredictionResult {
    ActualCharacteristic: number;
    Fragment: string;
    Predicted: string;
    TheoreticalCharacteristic: number;

}

interface SequencePredictionResultData {
    matching: number;
    researchObjectName: string;
    result: PredictionResult[];
}

/**
 * Interface for the angular controller's scope
 */
interface SequencePredictionResultScope extends ng.IScope, SequencePredictionResultData, ResultsLoadingScreenData {
}

/**
* Controller for sequence prediction results
*/
class SequencePredictionResultHandler {
    /**
    * Creates an instance of the sequence prediction results controller
    */
    constructor() {
        this.ngOnInit();
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(): void {
        const sequencePredictionResult = ($scope: SequencePredictionResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<SequencePredictionResultData>(
                $http, $scope, "Loading sequence prediction data", "Failed loading sequence prediction data");
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencePredictionResultCtrl", ["$scope", "$http", sequencePredictionResult]);
    }
}

/**
* Wrapper function for backward compatibility
* @returns Sequence prediction result controller instance
*/
export default function SequencePredictionResultController(): SequencePredictionResultHandler {
    return new SequencePredictionResultHandler();
}
