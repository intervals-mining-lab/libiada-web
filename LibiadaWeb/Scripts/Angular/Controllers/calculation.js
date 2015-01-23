﻿"use strict";

function calculation($scope, filterFilter) {
    MapModelFromJson($scope, data);

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

    $scope.disableSubmit = function () {
        return $scope.selectedMatters < $scope.minimumSelectedMatters;
    }

    $scope.characteristics = [];
    $scope.notationsFiltered = [];

    $scope.natureId = $scope.natures[0].Value;

    var filterByNature = function () {
        FilterOptionsByNature($scope, filterFilter, "notations");
        var notation = $scope.notationsFiltered[0];

        angular.forEach($scope.characteristics, function (characteristic) {
            characteristic.notation = notation;
        });
    };

    $scope.addCharacteristic = function () {
        $scope.characteristics.push({
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0],
            notation: $scope.notationsFiltered[0],
            language: $scope.languages[0],
            translator: $scope.translators[0]
        });
    };

    $scope.deleteCharacteristic = function (characteristic) {
        $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
    };

    $scope.isLinkable = function (characteristic) {
        return characteristic.characteristicType.Linkable;
    };

    $scope.$watch("natureId", filterByNature, true);
}

angular.module("Calculation", []).controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);