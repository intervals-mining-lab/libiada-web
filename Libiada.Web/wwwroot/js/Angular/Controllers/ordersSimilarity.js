function OrdersSimilarityController(data) {
    "use strict";

    function ordersSimilarity($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        }

        $scope.filterByNature = filterByNature;
    }

    angular.module("libiada").controller("OrdersSimilarityCtrl", ["$scope", "filterFilter", ordersSimilarity]);
}
