import { initScopeFromServer, InitializeBootstrapTooltips } from "functions";
/**
 * Angular controller class for cluster analysis results visualization
 */
class ClusterizationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const clusterizationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            await initScopeFromServer($http, $scope, "Loading cluster analysis results", "Failed loading cluster analysis results");
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
export default function ClusterizationResultController() {
    return new ClusterizationResultHandler();
}
//# sourceMappingURL=clusterizationResult.js.map