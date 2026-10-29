/**
 * Interface for the angular controller's scope
 */
interface OrdersIntervalsDistributionsAccordanceScope extends ng.IScope {
}

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
    private ngOnInit(): void {
        const ordersIntervalsDistributionsAccordance = ($scope: OrdersIntervalsDistributionsAccordanceScope): void => {
        };

        angular.module("libiada").controller(
            "OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns OrdersIntervalsDistributionsAccordanceHandler instance
*/
export default function OrdersIntervalsDistributionsAccordanceController(): OrdersIntervalsDistributionsAccordanceHandler {
    return new OrdersIntervalsDistributionsAccordanceHandler();
}
