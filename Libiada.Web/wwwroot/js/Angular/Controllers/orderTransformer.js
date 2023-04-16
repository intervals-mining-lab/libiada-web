function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.nature = $scope.natures[0].Value;
        $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        $scope.language = $scope.languages[0];
        $scope.translator = $scope.translators[0];
        $scope.pauseTreatment = $scope.pauseTreatments ? $scope.pauseTreatments[0] : null;
    }

    angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
