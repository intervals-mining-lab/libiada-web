function OrderTransformationVisualizationController(data) {
    "use strict";

    function orderTransformationVisualization($scope) {
        MapModelFromJson($scope, data);

        $scope.submitName = "Generate orders";
    }

    angular.module("libiada").controller("OrderTransformationVisualizationCtrl", ["$scope", orderTransformationVisualization]);
}
