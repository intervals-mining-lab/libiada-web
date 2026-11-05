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
    ngOnInit() {
        const orderTransformationVisualization = (_$scope) => {
        };
        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformationVisualizationCtrl", ["$scope", orderTransformationVisualization]);
    }
}
/**
* Wrapper function for backward compatibility
* @returns Order transformation visualization controller instance
*/
export default function OrderTransformationVisualizationController() {
    return new OrderTransformationVisualizationHandler();
}
//# sourceMappingURL=orderTransformationVisualization.js.map