import { MapModelFromJson } from "functions";
/**
 * Controller for order transformation characteristics dynamic visualization
 */
class OrderTransformationCharacteristicsDynamicVisualizationHandler {
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
        const orderTransformationCharacteristicsDynamicVisualization = ($scope) => {
            MapModelFromJson($scope, data);
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationCharacteristicsDynamicVisualizationCtrl", ["$scope", orderTransformationCharacteristicsDynamicVisualization]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of order transformation characteristics dynamic visualization handler
 */
export default function OrderTransformationCharacteristicsDynamicVisualizationController(data) {
    return new OrderTransformationCharacteristicsDynamicVisualizationHandler(data);
}
//# sourceMappingURL=orderTransformationCharacteristicsDynamicVisualization.js.map