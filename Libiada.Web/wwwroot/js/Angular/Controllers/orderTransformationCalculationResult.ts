import { initScopeFromServer  } from "functions";
import type { Characteristic, ResultsLoadingScreenData, SequenceCharacteristics } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface OrderTransformationCalculationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the controller's scope
 */
interface OrderTransformationCalculationResultScope 
    extends ng.IScope, OrderTransformationCalculationResultData, ResultsLoadingScreenData {
    characteristicsTableTabSelected: boolean;
}

/**
 * Angular controller class for order transformation calculation result visualization
 */
class OrderTransformationCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const orderTransformationCalculationResult = async ($scope: OrderTransformationCalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;
            
            // initializing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<OrderTransformationCalculationResultData>(
                $http,
                $scope,
                "Loading order transformation characteristics",
                "Failed loading order transformation characteristics");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationCalculationResultCtrl", ["$scope", "$http", orderTransformationCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformationCalculationResultController(): OrderTransformationCalculationResultHandler {
    return new OrderTransformationCalculationResultHandler();
}
