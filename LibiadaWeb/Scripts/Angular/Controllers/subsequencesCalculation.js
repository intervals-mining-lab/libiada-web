function SubsequencesCalculationController(data) {
    "use strict";

    function subsequencesCalculation($scope) {
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

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.addCharacteristic = function () {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: $scope.notationsFiltered[0]
            });
        };

        $scope.deleteCharacteristic = function (characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        };
    }

    angular.module("SubsequencesCalculation", []).controller("SubsequencesCalculationCtrl", ["$scope", subsequencesCalculation]);
}
