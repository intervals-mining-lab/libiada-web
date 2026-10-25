import { initScopeFromServer } from "functions";
/**
 * Controller for custom sequence order transformation calculation result visualization
 */
class CustomSequenceOrderTransformationCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const customSequenceOrderTransformationCalculationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            await initScopeFromServer($http, $scope, "Loading custom sequence order transformation calculation results", "Failed loading custom sequence order transformation calculation results");
            // initializing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceOrderTransformationCalculationResultCtrl", ["$scope", "$http", customSequenceOrderTransformationCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceOrderTransformationCalculationResultController() {
    return new CustomSequenceOrderTransformationCalculationResultHandler();
}
//# sourceMappingURL=customSequenceOrderTransformationCalculationResult.js.map