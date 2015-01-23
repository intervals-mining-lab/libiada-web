"use strict";

function BuildingComparison($scope) {
    MapModelFromJson($scope, data);

    $scope.natureId = $scope.natures[0].Value;

    $scope.selectedMatters = 0;

    $scope.matterCheckChanged = function (matter) {
        if (matter.Selected) {
            $scope.selectedMatters++;
        } else {
            $scope.selectedMatters--;
        }
    }

    $scope.disableMattersSelect = function (matter) {
        return ($scope.selectedMatters == $scope.maximumSelectedMatters) && !matter.Selected;
    }

    $scope.disableSubmit = function () {
        return $scope.selectedMatters < $scope.minimumSelectedMatters;
    }
}

angular.module("BuildingComparison", []).controller("BuildingComparisonCtrl", ["$scope", BuildingComparison]);