"use strict";

function genesCalculation($scope) {
    MapModelFromJson($scope, data);

    $scope.matterSelectionLimit = Number.MAX_VALUE;
    $scope.selectedMatters = 0;

    $scope.matterCheckChanged = function (matter) {
        if (matter.Selected) {
            $scope.selectedMatters++;
        } else {
            $scope.selectedMatters--;
        }
    }

    $scope.disableMattersSelect = function (matter) {
        return false;
    }

    $scope.characteristics = [];

    $scope.addCharacteristic = function () {
        $scope.characteristics.push({
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0],
            notation: $scope.notationsFiltered[0]
        });
    };

    $scope.deleteCharacteristic = function (characteristic) {
        $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
    };

    $scope.isLinkable = function (characteristic) {
        return characteristic.characteristicType.Linkable;
    };
}

angular.module("GenesCalculation", []).controller("GenesCalculationCtrl", ["$scope", genesCalculation]);

