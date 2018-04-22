function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
        MapModelFromJson($scope, data);

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: $scope.notations[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        }

        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;

        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.characteristics = [];
    }

    angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    mattersTable();
    characteristic();
}
