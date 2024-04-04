function OrderTransformationVisualizationController(data) {
    "use strict";

    function orderTransformationVisualization($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("OrderTransformationVisualizationCtrl", ["$scope", orderTransformationVisualization]);
}
