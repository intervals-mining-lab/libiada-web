/// <reference types="angular" />
/**
* Controller for calculating subsequences
*/
class SubsequencesCalculationHandler {
    /**
    * Creates an instance of the subsequence calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    /**
    * Initializes the Angular controller
    */
    initializeController() {
        "use strict";
        const subsequencesCalculation = ($scope) => {
            // Initialize scope with data from parameter
            MapModelFromJson($scope, this.data);
            /**
            * Apply filter to data
            * @param filter Filter to apply
            */
            function applyFilter(filter) {
                // Implementation of filter application method
                // Empty function, as in original JavaScript code
            }
            // Assign methods to $scope
            $scope.applyFilter = applyFilter;
            // Initialize default properties
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
function SubsequencesCalculationController(data) {
    return new SubsequencesCalculationHandler(data);
}
//# sourceMappingURL=subsequencesCalculation.js.map