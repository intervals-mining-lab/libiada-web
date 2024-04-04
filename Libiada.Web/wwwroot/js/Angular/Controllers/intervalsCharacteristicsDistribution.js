function IntervalsCharacteristicsDistributionController(data) {
    "use strict";

    function intervalsCharacteristicsDistribution($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("IntervalsCharacteristicsDistributionCtrl", ["$scope", intervalsCharacteristicsDistribution]);
}