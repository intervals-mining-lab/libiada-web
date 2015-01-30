function GenesAlignmentController(data) {
    "use strict";

    function genesAlignment($scope) {
        MapModelFromJson($scope, data);

        $scope.selectedMatters = 0;

        $scope.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        $scope.disableMattersSelect = function (matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        };

        $scope.disableSubmit = function () {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.links[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.isLinkable = function (characteristic) {
            return characteristic.characteristicType.Linkable;
        };
    }

    angular.module("GenesAlignment", []).controller("GenesAlignmentCtrl", ["$scope", genesAlignment]);
}
