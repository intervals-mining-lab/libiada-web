function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
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

        $scope.notationsFiltered = [];

        $scope.natureId = $scope.natures[0].Value;

        var filterByNature = function () {
            FilterOptionsByNature($scope, filterFilter, "notations");
            var notation = $scope.notationsFiltered[0];
            $scope.characteristic.notation = notation;
        };

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.$watch("natureId", filterByNature, true);
    }

    angular.module("Accordance", []).controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
}
