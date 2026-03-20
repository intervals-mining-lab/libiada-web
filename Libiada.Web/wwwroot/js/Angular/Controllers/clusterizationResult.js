import { initScopeFromServer } from "functions";
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
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
            initScopeFromServer($http, $scope, "Loading cluster analysis results", "Failed loading cluster analysis results");
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