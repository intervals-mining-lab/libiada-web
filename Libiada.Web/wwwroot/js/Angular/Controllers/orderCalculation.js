import { MapModelFromJson } from "functions";
/**
 * Controller for order calculations
 */
class OrderCalculationHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit(data) {
        const orderCalculation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("OrderCalculationCtrl", ["$scope", orderCalculation]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of OrderCalculationHandler
 */
export default function OrderCalculationController(data) {
    return new OrderCalculationHandler(data);
}
//# sourceMappingURL=orderCalculation.js.map