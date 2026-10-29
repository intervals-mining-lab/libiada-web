/**
* Controller for displaying order interval distribution correspondence
*/
class OrdersIntervalsDistributionsAccordanceHandler {
    /**
    * Creates a new controller instance
    * @param data Data for initializing the controller
    */
    constructor() {
        this.ngOnInit();
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit() {
        const ordersIntervalsDistributionsAccordance = ($scope) => {
        };
        angular.module("libiada").controller("OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns OrdersIntervalsDistributionsAccordanceHandler instance
*/
export default function OrdersIntervalsDistributionsAccordanceController() {
    return new OrdersIntervalsDistributionsAccordanceHandler();
}
//# sourceMappingURL=ordersIntervalsDistributionsAccordance.js.map