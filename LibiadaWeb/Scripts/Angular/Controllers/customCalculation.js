"use strict";

function customCalculation($scope) {
    MapModelFromJson($scope, data);

    $scope.disableSubmit = function () {
        return false;
    }

    $scope.characteristics = [];

    $scope.addCharacteristic = function () {
        $scope.characteristics.push({
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0]
        });
    };

    $scope.deleteCharacteristic = function (characteristic) {
        $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
    };

    $scope.isLinkable = function (characteristic) {
        return characteristic.characteristicType.Linkable;
    };
}

angular.module("CustomCalculation", []).controller("CustomCalculationCtrl", ["$scope", customCalculation]);
