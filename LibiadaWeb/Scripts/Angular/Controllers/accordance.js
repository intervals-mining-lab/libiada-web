"use strict";

function Accordance ($scope, filterFilter) {
    MapModelFromJson($scope, data);

    $scope.matterSelectionLimit = 2;
    $scope.selectedMatters = 0;

    $scope.matterCheckChanged = function(matter){
        if (matter.Selected) {
            $scope.selectedMatters++;
        } else {
            $scope.selectedMatters--;
        }
    }

    $scope.disableMattersSelect = function(matter) {
        return $scope.selectedMatters == $scope.matterSelectionLimit && !matter.Selected;
    }

    $scope.notationsFiltered = [];

    $scope.natureId = $scope.natures[0].Value;

    var filterByNature = function () {
        FilterOptionsByNature($scope, filterFilter, "notations");
        var notation = $scope.notationsFiltered[0];
        $scope.characteristic.notation = notation;
    };

    $scope.characteristic = {
        characteristicType: $scope.characteristicTypes[0],
        link: $scope.links[0],
        notation: $scope.notationsFiltered[0]
    };

    $scope.isLinkable = function(characteristic) {
        return characteristic.characteristicType.Linkable;
    };

    $scope.$watch("natureId", filterByNature, true);
}

angular.module("Accordance", []).controller("AccordanceCtrl", ["$scope", "filterFilter", Accordance]);