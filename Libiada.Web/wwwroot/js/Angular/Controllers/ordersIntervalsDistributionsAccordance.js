function OrdersIntervalsDistributionsAccordanceController(data) {
    "use strict";

    function ordersIntervalsDistributionsAccordance($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("OrdersIntervalsDistributionsAccordanceCtrl", ["$scope", ordersIntervalsDistributionsAccordance]);
}