function IntervalsCharacteristicsDistributionController(data) {
    "use strict";

    function intervalsCharacteristicsDistribution($scope) {
        MapModelFromJson($scope, data);

        $scope.submitName = "Generate orders";
    }

    angular.module("libiada").controller("IntervalsCharacteristicsDistributionCtrl", ["$scope", intervalsCharacteristicsDistribution]);
}