import { initScopeFromServer  } from "functions";
import type { Characteristic, SequenceCharacteristics } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface CalculationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
}

/**
 * Interface for the angular controller's scope
 */
interface CalculationResultScope extends ng.IScope, CalculationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

    characteristicsTableTabSelected: boolean;
}

/**
 * Angular controller class for integral characteristics calculation results visualization
 */
class CalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const calculationResult = async ($scope: CalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;
            
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<CalculationResultData>(
                $http,
                $scope,
                "Loading characteristics calculation results",
                "Failed loading characteristics calculation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CalculationResultCtrl", ["$scope", "$http", calculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CalculationResultController():  CalculationResultHandler {
    return new  CalculationResultHandler();
}
