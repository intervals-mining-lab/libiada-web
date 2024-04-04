function OrderCalculationController(data) {
    "use strict";

    function orderCalculation($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("OrderCalculationCtrl", ["$scope", orderCalculation]);
}
