﻿function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        }

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        }

        function addTransformation() {
            $scope.transformations.push({
                link: $scope.transformationLinks[0],
                operation: $scope.operations[0]
            });
        }

        function deleteTransformation(transformation) {
            $scope.transformations.splice($scope.transformations.indexOf(transformation), 1);
        }

        function filterByNature() {
            var notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

            angular.forEach($scope.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        }

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: filterFilter($scope.notations, { Nature: $scope.nature })[0],
                language: $scope.languages[0],
                translator: $scope.translators[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        }

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;
        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;
        $scope.filterByNature = filterByNature;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.disableMattersSelect = FakeDisableMattersSelect;
        $scope.addTransformation = addTransformation;
        $scope.deleteTransformation = deleteTransformation;

        $scope.transformations = [];
        $scope.characteristics = [];
        $scope.flags = { showRefSeqOnly: true };
        $scope.selectedMatters = 0;
        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("OrderTransformer", []).controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
