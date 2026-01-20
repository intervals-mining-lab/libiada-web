import { MapModelFromJson } from "functions";

/**
 * Interface for custom calculation data
 */
interface ICustomCalculationData {
}

/**
 * Interface for controller scope
 */
interface ICustomCalculationScope extends ng.IScope {

}

/**
 * Controller for custom calculation functionality
 */
class CustomCalculationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: ICustomCalculationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: ICustomCalculationData): void {
        const customCalculation = ($scope: ICustomCalculationScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CustomCalculationCtrl", ["$scope", customCalculation]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of custom calculation handler
 */
export default function CustomCalculationController(data: ICustomCalculationData): CustomCalculationHandler {
    return new CustomCalculationHandler(data);
}
