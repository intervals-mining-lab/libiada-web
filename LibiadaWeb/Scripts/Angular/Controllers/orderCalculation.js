function OrderCalculationController(data) {
    "use strict";

    function orderCalculation($scope) {
        MapModelFromJson($scope, data);
      
        $scope.submitName = "Calculate";

    }

    angular.module("libiada").controller("OrderCalculationCtrl", ["$scope", orderCalculation]);
}
