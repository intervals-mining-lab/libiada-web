function OrdersIntervalsDistributionsAccordanceController(data) {
    "use strict";

    function ordersIntervalsDistributionsAccordance($scope) {
        MapModelFromJson($scope, data);

        $scope.submitName = "Generate orders";
    }

    angular.module("libiada").controller("OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
}