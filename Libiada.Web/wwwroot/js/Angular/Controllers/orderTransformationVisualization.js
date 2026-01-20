import { MapModelFromJson } from "functions";
/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    /**
    * Creates an instance of the order transformation visualization controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const orderTransformationVisualization = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
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
export default function OrderTransformationVisualizationController(data) {
    return new OrderTransformationVisualizationHandler(data);
}
//# sourceMappingURL=orderTransformationVisualization.js.map