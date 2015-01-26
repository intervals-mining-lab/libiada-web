function GenesDistributionController(data) {
    "use strict";

    function genesDistribution($scope) {
        MapModelFromJson($scope, data);

        $scope.selectedMatters = 0;

        $scope.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        $scope.disableMattersSelect = function () {
            return false;
        };

        $scope.disableSubmit = function () {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        $scope.firstCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.secondCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.isLinkable = function (characteristic) {
            return characteristic.characteristicType.Linkable;
        };
    }

    angular.module("GenesDistribution", []).controller("GenesDistributionCtrl", ["$scope", genesDistribution]);
}
