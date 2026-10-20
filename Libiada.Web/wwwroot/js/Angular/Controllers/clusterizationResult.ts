import { initScopeFromServer, InitializeBootstrapTooltips } from "functions";
import type { Characteristic, ResultsLoadingScreenData, SequenceCharacteristics, SequencesGroup } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface ClusterizationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
    sequenceGroups: SequencesGroup[];
}

/**
 * Interface for the angular controller's scope
 */
interface ClusterizationResultScope extends ng.IScope, ClusterizationResultData, ResultsLoadingScreenData {
    characteristicsTableTabSelected: boolean;
}

/**
 * Angular controller class for cluster analysis results visualization
 */
class ClusterizationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const clusterizationResult = async ($scope: ClusterizationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.characteristicsTableTabSelected = false;
            
            await initScopeFromServer<ClusterizationResultData>(
                $http,
                $scope,
                "Loading cluster analysis results",
                "Failed loading cluster analysis results");

            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
        };

        // Register controller in Angular module
        angular.module("libiada").controller("ClusterizationResultCtrl", ["$scope", "$http", clusterizationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function ClusterizationResultController():  ClusterizationResultHandler {
    return new  ClusterizationResultHandler();
}
