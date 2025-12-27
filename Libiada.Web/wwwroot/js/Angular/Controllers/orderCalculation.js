"use strict";
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
        this.ngOnInit(data);
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit(data) {
        const orderCalculation = ($scope) => {
            MapModelFromJson($scope, data);
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