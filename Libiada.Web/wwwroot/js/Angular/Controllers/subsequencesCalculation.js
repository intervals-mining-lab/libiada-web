function SubsequencesCalculationController(data) {
    "use strict";

    function subsequencesCalculation($scope) {
        MapModelFromJson($scope, data);

        function applyFilter(filter) {
        }

        $scope.applyFilter = applyFilter;

        $scope.filters = [];
        $scope.hideNotation = true;
    }

    angular.module("libiada").controller("SubsequencesCalculationCtrl", ["$scope", subsequencesCalculation]);
}
