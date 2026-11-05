/**
 * Interface for the controller's scope
 */
interface OrderTransformationVisualizationScope extends ng.IScope {
}

/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(): void {
        const orderTransformationVisualization = (_$scope: OrderTransformationVisualizationScope): void => {
        };

        // Register the controller in Angular
        angular.module("libiada").controller(
            "OrderTransformationVisualizationCtrl", 
            ["$scope", orderTransformationVisualization]);
    }
}

/**
* Wrapper function for backward compatibility
* @returns Order transformation visualization controller instance
*/
export default function OrderTransformationVisualizationController(): OrderTransformationVisualizationHandler {
    return new OrderTransformationVisualizationHandler();
}
