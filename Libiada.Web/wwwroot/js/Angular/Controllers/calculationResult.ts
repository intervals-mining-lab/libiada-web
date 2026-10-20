import { initScopeFromServer, InitializeBootstrapTooltips } from "functions";
import type { Characteristic, ResultsLoadingScreenData, SequenceCharacteristics, SequencesGroup } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface CalculationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
    sequenceGroups?: SequencesGroup[]
}

/**
 * Interface for the angular controller's scope
 */
interface CalculationResultScope extends ng.IScope, CalculationResultData, ResultsLoadingScreenData {
    characteristicsTableTabSelected: boolean;
}

/**
 * Angular controller class for integral characteristics calculation results visualization
 */
class CalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const calculationResult = async ($scope: CalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;

            await initScopeFromServer<CalculationResultData>(
                $http,
                $scope,
                "Loading characteristics calculation results",
                "Failed loading characteristics calculation results");
            
            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
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
