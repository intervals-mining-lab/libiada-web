function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function addTransformation() {
            $scope.transformations.push({
                link: $scope.transformationLinks[0],
                operation: $scope.operations[0]
            });
        }

        function deleteTransformation(transformation) {
            $scope.transformations.splice($scope.transformations.indexOf(transformation), 1);
        }

        $scope.addTransformation = addTransformation;
        $scope.deleteTransformation = deleteTransformation;

        $scope.transformations = [];
        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
