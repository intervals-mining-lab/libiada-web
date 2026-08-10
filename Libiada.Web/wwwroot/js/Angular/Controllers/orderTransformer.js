import { MapModelFromJson } from "functions";
/**
* Angular controller class
*/
class OrderTransformerHandler {
    /**
    * Creates an instance of the order transformation controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const orderTransformer = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", orderTransformer]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation controller instance
*/
export default function OrderTransformerController(data) {
    return new OrderTransformerHandler(data);
}
//# sourceMappingURL=orderTransformer.js.map