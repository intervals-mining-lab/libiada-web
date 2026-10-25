import { initScopeFromServer } from "functions";
import type { Characteristic, ResultsLoadingScreenData, SequenceCharacteristics } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface CustomSequenceCalculationResultData {
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the angular controller's scope
 */
interface CustomSequenceCalculationResultScope extends ng.IScope, CustomSequenceCalculationResultData, ResultsLoadingScreenData {
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
        const customSequenceCalculationResult = async (
            $scope: CustomSequenceCalculationResultScope, $http: ng.IHttpService): Promise<void> => {

            $scope.characteristicsTableTabSelected = false;

            await initScopeFromServer<CustomSequenceCalculationResultData>(
                $http,
                $scope,
                "Loading custom sequence calculation results",
                "Failed loading custom sequence calculation results");

            // initializing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
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
