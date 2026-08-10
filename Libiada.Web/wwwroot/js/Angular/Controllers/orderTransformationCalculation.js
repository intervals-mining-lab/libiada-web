import { MapModelFromJson } from "functions";
/**
* Angular controller class
*/
class OrderTransformationCalculationHandler {
    /**
    * Creates an instance of the order transformation calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const orderTransformationCalculation = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("OrderTransformationCalculationCtrl", ["$scope", orderTransformationCalculation]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation calculation controller instance
*/
export default function OrderTransformationCalculationController(data) {
    return new OrderTransformationCalculationHandler(data);
}
//# sourceMappingURL=orderTransformationCalculation.js.map