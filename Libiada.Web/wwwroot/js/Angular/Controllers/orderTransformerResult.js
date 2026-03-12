import { initScopeFromServer } from "functions";
/**
 * Controller for order transformation result visualization
 */
class OrderTransformerResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const orderTransformerResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading order transformation results", "Failed loading order transformation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformerResultCtrl", ["$scope", "$http", orderTransformerResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformerResultController() {
    return new OrderTransformerResultHandler();
}
//# sourceMappingURL=orderTransformerResult.js.map