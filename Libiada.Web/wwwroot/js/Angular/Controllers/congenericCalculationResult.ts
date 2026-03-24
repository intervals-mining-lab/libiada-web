import { initScopeFromServer  } from "functions";
import type { Characteristic, SequenceCharacteristics, SequencesGroup } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface CongenericCalculationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
    theoreticalRanks: number[][][];
    sequenceGroups?: SequencesGroup[]
}

/**
 * Interface for the angular controller's scope
 */
interface CongenericCalculationResultScope extends ng.IScope, CongenericCalculationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

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
        const congenericCalculationResult = async ($scope: CongenericCalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;
            
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<CongenericCalculationResultData>(
                $http,
                $scope,
                "Loading characteristics calculation results",
                "Failed loading characteristics calculation results");
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
