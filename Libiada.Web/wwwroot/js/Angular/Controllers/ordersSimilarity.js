function OrdersSimilarityController(data) {
    "use strict";

    function ordersSimilarity($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        }

        $scope.filterByNature = filterByNature;

        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("libiada").controller("OrdersSimilarityCtrl", ["$scope", "filterFilter", ordersSimilarity]);
}
