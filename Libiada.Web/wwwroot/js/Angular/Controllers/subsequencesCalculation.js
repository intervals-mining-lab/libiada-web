function SubsequencesCalculationController(data) {
    "use strict";

    function subsequencesCalculation($scope) {
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

        function applyFilter(filter) {
        }

        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.applyFilter = applyFilter;

        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.filters = [];
        $scope.hideNotation = true;
    }

    angular.module("libiada").controller("SubsequencesCalculationCtrl", ["$scope", subsequencesCalculation]);
}
