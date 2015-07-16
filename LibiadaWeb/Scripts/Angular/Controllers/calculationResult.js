function CalculationResultController(data) {
    "use strict";

    function calculationResult($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("CalculationResult", []).controller("CalculationResultCtrl", ["$scope", calculationResult]);
}
