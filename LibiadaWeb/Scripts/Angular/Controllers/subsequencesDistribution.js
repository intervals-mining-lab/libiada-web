function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
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

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.firstCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.secondCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notationsFiltered[0]
        };
    }

    angular.module("SubsequencesDistribution", []).controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
}
