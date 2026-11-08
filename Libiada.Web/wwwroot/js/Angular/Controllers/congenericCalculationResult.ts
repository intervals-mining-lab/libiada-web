import { InitializeBootstrapTooltips, initScopeFromServer } from "functions";
import type { Characteristic, ResultsLoadingScreenData, SequenceCharacteristics, SequencesGroup } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface CongenericCalculationResultData {
    characteristicNames: string[];
    characteristics: SequenceCharacteristics[];
    characteristicsList: Characteristic[];
    theoreticalRanks: number[][][];
    sequenceGroups?: SequencesGroup[]
}

/**
 * Interface for the angular controller's scope
 */
interface CongenericCalculationResultScope extends ng.IScope, CongenericCalculationResultData, ResultsLoadingScreenData {
    characteristicsTableTabSelected: boolean;
}

/**
 * Angular controller class for congeneric characteristics calculation results visualization
 */
class CongenericCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const congenericCalculationResult = async (
            $scope: CongenericCalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;

            await initScopeFromServer<CongenericCalculationResultData>(
                $http,
                $scope,
                "Loading characteristics calculation results",
                "Failed loading characteristics calculation results");

            InitializeBootstrapTooltips();
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CongenericCalculationResultCtrl", ["$scope", "$http", congenericCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CongenericCalculationResultController(): CongenericCalculationResultHandler {
    return new CongenericCalculationResultHandler();
}
