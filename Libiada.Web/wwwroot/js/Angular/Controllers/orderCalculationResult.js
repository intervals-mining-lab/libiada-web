import { initScopeFromServer } from "functions";
/**
 * Controller for order calculation result visualization
 */
class OrderCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const orderCalculationResult = ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
            initScopeFromServer($http, $scope, "Loading order calculation results", "Failed loading order calculation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderCalculationResultCtrl", ["$scope", "$http", orderCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function OrderCalculationResultController() {
    return new OrderCalculationResultHandler();
}
//# sourceMappingURL=orderCalculationResult.js.map