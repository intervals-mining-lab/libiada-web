function SubsequencesCalculationController(data) {
    "use strict";

    function subsequencesCalculation($scope) {
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

        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });
                $scope.newFilter = "";
            }
        }

        function deleteFilter(filter) {
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
        }

        function applyFilter(filter) {
        }

        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.applyFilter = applyFilter;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.characteristics = [];
        $scope.filters = [];
        $scope.hideNotation = true;
    }

    angular.module("libiada", []).controller("SubsequencesCalculationCtrl", ["$scope", "filterFilter", subsequencesCalculation]);
    mattersTable();
}
