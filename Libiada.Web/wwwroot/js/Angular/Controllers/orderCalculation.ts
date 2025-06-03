/// <reference types="angular" />

/**
 * Interface for order calculation data
 */
interface IOrderCalculationData {
    // Add properties that are passed in from the server
    // This is a generic structure that should be updated based on the actual data
    [key: string]: any;
}

/**
 * Interface for the order calculation scope
 */
interface IOrderCalculationScope extends ng.IScope {
    // Add properties and methods that are used on the scope
    // These properties get populated from the data parameter through MapModelFromJson
    [key: string]: any;
}

/**
 * Controller for order calculations
 */
class OrderCalculationHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
     */
    constructor(private data: IOrderCalculationData) {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        "use strict";

        const orderCalculation = ($scope: IOrderCalculationScope): void => {
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
function OrderCalculationController(data: IOrderCalculationData): OrderCalculationHandler {
    return new OrderCalculationHandler(data);
}
