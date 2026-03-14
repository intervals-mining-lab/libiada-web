import { initScopeFromServer } from "functions";
import type { Characteristic, SequenceCharacteristics } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface CustomSequenceOrderTransformationCalculationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the angular controller's scope
 */
interface CustomSequenceOrderTransformationCalculationResultScope extends ng.IScope, CustomSequenceOrderTransformationCalculationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

    characteristicsTableTabSelected: boolean;
}

/**
 * Controller for custom sequence order transformation calculation result visualization
 */
class CustomSequenceOrderTransformationCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const customSequenceOrderTransformationCalculationResult = ($scope: CustomSequenceOrderTransformationCalculationResultScope, $http: ng.IHttpService): void => {
            $scope.characteristicsTableTabSelected = false;

            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<CustomSequenceOrderTransformationCalculationResultData>(
                $http,
                $scope,
                "Loading custom sequence order transformation calculation results",
                "Failed loading custom sequence order transformation calculation results");
        };       

        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceOrderTransformationCalculationResultCtrl",
            ["$scope", "$http", customSequenceOrderTransformationCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceOrderTransformationCalculationResultController(): CustomSequenceOrderTransformationCalculationResultHandler {
    return new CustomSequenceOrderTransformationCalculationResultHandler();
}
