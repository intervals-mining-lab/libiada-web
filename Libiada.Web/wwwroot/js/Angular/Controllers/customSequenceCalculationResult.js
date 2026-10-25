import { initScopeFromServer } from "functions";
/**
 * Controller for custom sequence calculation result visualization
 */
class CustomSequenceCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const customSequenceCalculationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            await initScopeFromServer($http, $scope, "Loading custom sequence calculation results", "Failed loading custom sequence calculation results");
            // initializing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceCalculationResultCtrl", ["$scope", "$http", customSequenceCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceCalculationResultController() {
    return new CustomSequenceCalculationResultHandler();
}
//# sourceMappingURL=customSequenceCalculationResult.js.map