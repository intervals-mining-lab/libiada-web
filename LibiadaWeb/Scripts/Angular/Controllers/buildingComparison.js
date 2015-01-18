"use strict";

function BuildingComparison($scope) {
    MapModelFromJson($scope, data);

    $scope.natureId = $scope.natures[0].Value;
    $scope.matterSelectionLimit = 2;
    $scope.selectedMatters = 0;

    $scope.matterCheckChanged = function (matter) {
        if (matter.Selected) {
            $scope.selectedMatters++;
        } else {
            $scope.selectedMatters--;
        }
    }

    $scope.disableMattersSelect = function (matter) {
        return $scope.selectedMatters == $scope.matterSelectionLimit && !matter.Selected;
    }
}

angular.module("BuildingComparison", []).controller("BuildingComparisonCtrl", ["$scope", BuildingComparison]);