import { MapModelFromJson } from "functions";

/**
 * Interface for the data object passed from the server
 */
interface IOrdersIntervalsDistributionsAccordanceData {

}

/**
 * Interface for the angular controller's scope
 */
interface IOrdersIntervalsDistributionsAccordanceScope extends ng.IScope {

}

/**
* Controller for displaying order interval distribution correspondence
*/
class OrdersIntervalsDistributionsAccordanceHandler {
    /**
    * Creates a new controller instance
    * @param data Data for initializing the controller
    */
    constructor(data: IOrdersIntervalsDistributionsAccordanceData) {
        this.ngOnInit(data);
    }

    /** 
    * Initializes the Angular controller 
    */
    private ngOnInit(data: IOrdersIntervalsDistributionsAccordanceData): void {
        const ordersIntervalsDistributionsAccordance = ($scope: IOrdersIntervalsDistributionsAccordanceScope): void => {
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
export default function OrdersIntervalsDistributionsAccordanceController(data: IOrdersIntervalsDistributionsAccordanceData): OrdersIntervalsDistributionsAccordanceHandler {
    return new OrdersIntervalsDistributionsAccordanceHandler(data);
}
