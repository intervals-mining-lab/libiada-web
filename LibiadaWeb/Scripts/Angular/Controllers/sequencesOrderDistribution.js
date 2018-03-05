function SequencesOrderDistributionController(data) {
    "use strict";

    function sequencesOrderDistribution($scope) {
        MapModelFromJson($scope, data);

        $scope.submitName = "Generate orders";
    }

    angular.module("libiada", []).controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
}