function SequencesAlignmentController(data) {
    "use strict";

    function sequencesAlignment($scope) {
        MapModelFromJson($scope, data);

        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });
                $scope.newFilter = "";
            }
        }

        function deleteFilter(filter) {
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
        }

        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;

        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.filters = [];
        
        $scope.subsequencesCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };
    }

    angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
    mattersTable();
    characteristic();
}
