import { MapModelFromJson } from "functions";


/**
 * Interface for the data object passed from the server
 */
interface OrderTransformationVisualizationData {

}

/**
 * Interface for the controller's scope
 */
interface OrderTransformationVisualizationScope extends ng.IScope {
}

/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    /**
    * Creates an instance of the order transformation visualization controller
    * @param data Data for initializing the controller
    */
    constructor(data: OrderTransformationVisualizationData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: OrderTransformationVisualizationData): void {
        const orderTransformationVisualization = ($scope: OrderTransformationVisualizationScope): void => {
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
export default function OrderTransformationVisualizationController(data: OrderTransformationVisualizationData): OrderTransformationVisualizationHandler {
    return new OrderTransformationVisualizationHandler(data);
}
