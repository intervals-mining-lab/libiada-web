/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    /**
    * Creates an instance of the order transformation visualization controller
    * @param data Data for initializing the controller
    */
    constructor() {
        this.ngOnInit();
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit() {
        const orderTransformationVisualization = ($scope) => {
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
export default function OrderTransformationVisualizationController() {
    return new OrderTransformationVisualizationHandler();
}
//# sourceMappingURL=orderTransformationVisualization.js.map