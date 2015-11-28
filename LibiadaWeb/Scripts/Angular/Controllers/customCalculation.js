function CustomCalculationController(data) {
    "use strict";

    function customCalculation($scope) {
        MapModelFromJson($scope, data);

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0]
            });
        };

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        };

        function addSequence() {
            $scope.customSequences.push({});
        };

        function deleteSequence(customSequence) {
            $scope.customSequences.splice($scope.customSequences.indexOf(customSequence), 1);
        };

        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;
        $scope.addSequence = addSequence;
        $scope.deleteSequence = deleteSequence;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.disableSubmit = fakeDisableSubmit;

        $scope.characteristics = [];
        $scope.customSequences = [];
    }

    angular.module("CustomCalculation", []).controller("CustomCalculationCtrl", ["$scope", customCalculation]);
}
