function CustomCalculationController(data) {
    "use strict";

    function customCalculation($scope) {
        MapModelFromJson($scope, data);

        $scope.disableSubmit = function () {
            return false;
        };

        $scope.characteristics = [];

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.addCharacteristic = function () {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0]
            });
        };

        $scope.deleteCharacteristic = function (characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        };
    }

    angular.module("CustomCalculation", []).controller("CustomCalculationCtrl", ["$scope", customCalculation]);
}
