import { MapModelFromJson } from "functions";
/**
* Controller for calculating subsequences
*/
class SubsequencesCalculationHandler {
    /**
    * Creates an instance of the subsequence calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const subsequencesCalculation = ($scope) => {
            // Initialize scope with data from server
            MapModelFromJson($scope, data);
            // Initialize default properties values
            $scope.filters = [];
            $scope.hideNotation = true;
        };
        // Register controller in Angular
        angular.module("libiada").controller("SubsequencesCalculationCtrl", ["$scope", subsequencesCalculation]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Subsequence calculation controller instance
*/
export default function SubsequencesCalculationController(data) {
    return new SubsequencesCalculationHandler(data);
}
//# sourceMappingURL=subsequencesCalculation.js.map