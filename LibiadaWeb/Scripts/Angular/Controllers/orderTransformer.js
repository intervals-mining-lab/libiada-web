function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
