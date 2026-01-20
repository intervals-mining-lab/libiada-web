import { MapModelFromJson } from "functions";
/**
* Controller for displaying order interval distribution correspondence
*/
class OrdersIntervalsDistributionsAccordanceHandler {
    /**
    * Creates a new controller instance
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const ordersIntervalsDistributionsAccordance = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns OrdersIntervalsDistributionsAccordanceHandler instance
*/
export default function OrdersIntervalsDistributionsAccordanceController(data) {
    return new OrdersIntervalsDistributionsAccordanceHandler(data);
}
//# sourceMappingURL=ordersIntervalsDistributionsAccordance.js.map