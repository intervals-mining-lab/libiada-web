/// <reference types="angular" />
/**
 * Controller for order calculations
 */
class OrderCalculationHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
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
        const orderCalculation = ($scope) => {
            MapModelFromJson($scope, this.data);
        };
        angular.module("libiada").controller("OrderCalculationCtrl", ["$scope", orderCalculation]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of OrderCalculationHandler
 */
function OrderCalculationController(data) {
    return new OrderCalculationHandler(data);
}
//# sourceMappingURL=orderCalculation.js.map