/**
* Interface for OrdersIntervalsDistributionsAccordance controller input data
*/
interface IOrdersIntervalsDistributionsAccordanceData {

    [key: string]: any;
}

/**
* Interface for OrdersIntervalsDistributionsAccordance controller scope
*/
interface IOrdersIntervalsDistributionsAccordanceScope extends ng.IScope {

    [key: string]: any;
}

/**
* Controller for displaying order interval distribution correspondence
*/
class OrdersIntervalsDistributionsAccordanceHandler {
    /**
    * Creates a new controller instance
    * @param data Data for initializing the controller
    */
    constructor(private data: IOrdersIntervalsDistributionsAccordanceData) {
        this.initializeController(data);
    }

    /** 
    * Initializes the Angular controller 
    */
    private initializeController(data: IOrdersIntervalsDistributionsAccordanceData): void {
        "use strict";

        const ordersIntervalsDistributionsAccordance = ($scope: IOrdersIntervalsDistributionsAccordanceScope): void => {
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
function OrdersIntervalsDistributionsAccordanceController(data: IOrdersIntervalsDistributionsAccordanceData): OrdersIntervalsDistributionsAccordanceHandler {
    return new OrdersIntervalsDistributionsAccordanceHandler(data);
}