function BuildingsSimilarityController(data) {
    "use strict";

    function buildingsSimilarity($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.natureId = $scope.natures[0].Value;

        $scope.filterByNature = function () {
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.natureId })[0];
        };

        $scope.selectedMatters = 0;

        $scope.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        $scope.disableMattersSelect = function (matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        };

        $scope.disableSubmit = function () {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };
    }

    angular.module("BuildingsSimilarity", []).controller("BuildingsSimilarityCtrl", ["$scope", "filterFilter", buildingsSimilarity]);
}
