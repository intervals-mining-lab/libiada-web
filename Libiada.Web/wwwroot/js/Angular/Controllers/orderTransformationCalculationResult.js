import { initScopeFromServer } from "functions";
/**
 * Angular controller class for order transformation calculation result visualization
 */
class OrderTransformationCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const orderTransformationCalculationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
            initScopeFromServer($http, $scope, "Loading order transformation characteristics", "Failed loading order transformation characteristics");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationCalculationResultCtrl", ["$scope", "$http", orderTransformationCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformationCalculationResultController() {
    return new OrderTransformationCalculationResultHandler();
}
//# sourceMappingURL=orderTransformationCalculationResult.js.map