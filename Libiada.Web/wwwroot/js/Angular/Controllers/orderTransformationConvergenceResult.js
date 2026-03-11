import { initScopeFromServer } from "functions";
/**
 *  Angular controller class for order transformation convergence result visualization
 */
class OrderTransformationConvergenceResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const orderTransformationConvergenceResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading order transformation convergence results", "Failed loading order transformation convergence results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationConvergenceResultCtrl", ["$scope", "$http", orderTransformationConvergenceResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformationConvergenceResultController() {
    return new OrderTransformationConvergenceResultHandler();
}
//# sourceMappingURL=orderTransformationConvergenceResult.js.map