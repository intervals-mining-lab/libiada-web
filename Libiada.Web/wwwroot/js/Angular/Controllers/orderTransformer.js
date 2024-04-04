function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            if (!$scope.hideNotation) {
                $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                // if notation is not linked to characteristic
                angular.forEach($scope.characteristics, characteristic => {
                    characteristic.notation = notation;
                });
            }
        }

        $scope.filterByNature = filterByNature;

        $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        $scope.language = $scope.languages[0];
        $scope.translator = $scope.translators[0];
        $scope.pauseTreatment = $scope.pauseTreatments?.[0];
    }

    angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
