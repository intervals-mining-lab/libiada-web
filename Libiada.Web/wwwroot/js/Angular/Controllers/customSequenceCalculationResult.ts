import { initScopeFromServer } from "functions";
import type { Characteristic, SequenceCharacteristics } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface CustomSequenceCalculationResultData extends ng.IScope {
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the custom sequence calculation result scope
 */
interface CustomSequenceCalculationResultScope extends ng.IScope, CustomSequenceCalculationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

    characteristicsTableTabSelected: boolean;
}

/**
 * Controller for custom sequence calculation result visualization
 */
class CustomSequenceCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const customSequenceCalculationResult = ($scope: CustomSequenceCalculationResultScope, $http: ng.IHttpService): void => {
            $scope.characteristicsTableTabSelected = false;

            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<CustomSequenceCalculationResultData>(
                $http,
                $scope,
                "Loading custom sequence calculation results",
                "Failed loading custom sequence calculation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceCalculationResultCtrl", ["$scope", "$http", customSequenceCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceCalculationResultController(): CustomSequenceCalculationResultHandler {
    return new CustomSequenceCalculationResultHandler();
}
