import { MapModelFromJson } from "functions";

/**
 * Interface for the data object passed from the server
 */
interface IOrderCalculationData {
}

/**
 * Interface for the angular controller's scope
 */
interface IOrderCalculationScope extends ng.IScope {

}

/**
 * Controller for order calculations
 */
class OrderCalculationHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
     */
    constructor(data: IOrderCalculationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: IOrderCalculationData): void {
        const orderCalculation = ($scope: IOrderCalculationScope): void => {
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
export default function OrderCalculationController(data: IOrderCalculationData): OrderCalculationHandler {
    return new OrderCalculationHandler(data);
}
