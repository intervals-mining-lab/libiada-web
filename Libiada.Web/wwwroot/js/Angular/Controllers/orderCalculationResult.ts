import { initScopeFromServer } from "functions";
import type { Characteristic, SequenceCharacteristics } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface OrderCalculationResultData extends ng.IScope {
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the order calculation result scope
 */
interface OrderCalculationResultScope extends ng.IScope, OrderCalculationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

    characteristicsTableTabSelected: boolean;
}

/**
 * Controller for order calculation result visualization
 */
class OrderCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const orderCalculationResult = ($scope: OrderCalculationResultScope, $http: ng.IHttpService): void => {

            $scope.characteristicsTableTabSelected = false;

            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<OrderCalculationResultData>(
                $http,
                $scope,
                "Loading order calculation results",
                "Failed loading order calculation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderCalculationResultCtrl", ["$scope", "$http", orderCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function OrderCalculationResultController(): OrderCalculationResultHandler {
    return new OrderCalculationResultHandler();
}
