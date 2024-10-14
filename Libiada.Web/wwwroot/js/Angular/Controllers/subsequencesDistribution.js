function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
}
