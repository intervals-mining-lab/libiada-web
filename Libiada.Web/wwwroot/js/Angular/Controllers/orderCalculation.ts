import { MapModelFromJson } from "functions";
import { CharacteristicType } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface OrderCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
}

/**
 * Interface for the angular controller's scope
 */
interface OrderCalculationScope extends ng.IScope, OrderCalculationData {

}

/**
 * Controller for order calculations
 */
class OrderCalculationHandler {
    constructor(data: OrderCalculationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: OrderCalculationData): void {
        const orderCalculation = ($scope: OrderCalculationScope): void => {
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
export default function OrderCalculationController(data: OrderCalculationData): OrderCalculationHandler {
    return new OrderCalculationHandler(data);
}
