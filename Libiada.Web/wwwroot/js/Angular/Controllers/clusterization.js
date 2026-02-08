import { MapModelFromJson } from "functions";
// Controller class
class ClusterizationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const clusterization = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            $scope.clusterizationType = $scope.clusterizatorsTypes[0];
        };
        angular.module("libiada").controller("ClusterizationCtrl", ["$scope", "filterFilter", clusterization]);
    }
}
// Wrapper function for backwards compatibility
export default function ClusterizationController(data) {
    return new ClusterizationOperator(data);
}
//# sourceMappingURL=clusterization.js.map