/// <reference types="angular" />
/// <reference types="functions" />
/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    /**
    * Creates an instance of the order transformation visualization controller
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
        const orderTransformationVisualization = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformationVisualizationCtrl", ["$scope", orderTransformationVisualization]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation visualization controller instance
*/
function OrderTransformationVisualizationController(data) {
    return new OrderTransformationVisualizationHandler(data);
}
//# sourceMappingURL=OrderTransformationVisualization.js.map