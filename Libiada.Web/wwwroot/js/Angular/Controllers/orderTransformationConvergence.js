import { MapModelFromJson } from "functions";
// Angular controller class
class OrderTransformationConvergenceHandler {
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
        const orderTransformationConvergence = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationConvergenceCtrl", ["$scope", "filterFilter", orderTransformationConvergence]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of order transformation convergence handler
 */
export default function OrderTransformationConvergenceController(data) {
    return new OrderTransformationConvergenceHandler(data);
}
//# sourceMappingURL=orderTransformationConvergence.js.map