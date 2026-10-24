import { initScopeFromServer } from "functions";
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
    ngOnInit() {
        const sequencePredictionResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading sequence prediction data", "Failed loading sequence prediction data");
        };
        // Register the controller in Angular
        angular.module("libiada").controller("SequencePredictionResultCtrl", ["$scope", "$http", sequencePredictionResult]);
    }
}
/**
* Wrapper function for backward compatibility
* @returns Sequence prediction result controller instance
*/
export default function SequencePredictionResultController() {
    return new SequencePredictionResultHandler();
}
//# sourceMappingURL=sequencePredictionResult.js.map