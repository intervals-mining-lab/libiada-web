function CalculationController(data) {
    "use strict";

    function calculation($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.nature = $scope.natures[0].Value;
        $scope.disableSubmit = $scope.minimumSelectedMatters > 0;

        // if notation is not linked to characteristic
        $scope.language = $scope.languages[0];
        $scope.translator = $scope.translators[0];

        // if we are in clusterization
        if ($scope.ClusterizatorsTypes) {
            $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
        }
    }

    angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
}
