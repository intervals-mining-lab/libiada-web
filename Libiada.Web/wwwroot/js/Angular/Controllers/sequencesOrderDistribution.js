function SequencesOrderDistributionController(data) {
    "use strict";

    function sequencesOrderDistribution($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
}