import { initScopeFromServer  } from "functions";
import type { Characteristic, SequenceCharacteristics, Cluster } from "viewDataTypes";


/**
 * Interface for the data object fetched from the server
 */
interface ClusterizationResultData {
    transformationsList: string[];
    iterationsCount: number;
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];
    characteristicsList: Characteristic[];
    sequenceGroups: Cluster[];
}

/**
 * Interface for the angular controller's scope
 */
interface ClusterizationResultScope extends ng.IScope, ClusterizationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

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
            
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            initScopeFromServer<ClusterizationResultData>(
                $http,
                $scope,
                "Loading cluster analysis results",
                "Failed loading cluster analysis results");
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
