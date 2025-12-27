"use strict";
/**
* Controller for displaying order interval distribution correspondence
*/
class OrdersIntervalsDistributionsAccordanceHandler {
    /**
    * Creates a new controller instance
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.data = data;
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        "use strict";
        const ordersIntervalsDistributionsAccordance = ($scope) => {
            MapModelFromJson($scope, this.data);
        };
        angular.module("libiada").controller("OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns OrdersIntervalsDistributionsAccordanceHandler instance
*/
function OrdersIntervalsDistributionsAccordanceController(data) {
    return new OrdersIntervalsDistributionsAccordanceHandler(data);
}
//# sourceMappingURL=ordersIntervalsDistributionsAccordance.js.map