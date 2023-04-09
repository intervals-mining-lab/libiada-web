function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
        MapModelFromJson($scope, data);

        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;
    }

    angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
}
