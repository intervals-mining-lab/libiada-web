function BuildingComparisonController(data) {
    "use strict";

    function buildingComparison($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.natureId = $scope.natures[0].Value;

        var filterByNature = function () {
            FilterOptionsByNature($scope, filterFilter, "notations");
            $scope.notation = $scope.notationsFiltered[0];
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

        $scope.$watch("natureId", filterByNature, true);
    }

    angular.module("BuildingComparison", []).controller("BuildingComparisonCtrl", ["$scope", "filterFilter", buildingComparison]);
}
