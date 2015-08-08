function CalculationController(data) {
    "use strict";

    function calculation($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.selectedMatters = 0;

        $scope.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        $scope.disableMattersSelect = function () {
            return false;
        };

        $scope.disableSubmit = function () {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        $scope.characteristics = [];

        $scope.natureId = $scope.natures[0].Value;

        $scope.filterByNature = function () {
            var notation = filterFilter($scope.notations, { Nature: $scope.natureId })[0];

            angular.forEach($scope.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        };

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.addCharacteristic = function () {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: filterFilter($scope.notations, { Nature: $scope.natureId })[0],
                language: $scope.languages[0],
                translator: $scope.translators[0]
            });
        };

        $scope.deleteCharacteristic = function (characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        };
    }

    angular.module("Calculation", []).controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
}
