"use strict";
/// <reference types="angular" />
/**
 * Controller for custom calculation functionality
 */
class CustomCalculationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        "use strict";
        const customCalculation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CustomCalculationCtrl", ["$scope", customCalculation]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of custom calculation handler
 */
function CustomCalculationController(data) {
    return new CustomCalculationHandler(data);
}
//# sourceMappingURL=customCalculation.js.map