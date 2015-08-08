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

        $scope.natureId = $scope.natures[0].Value;

        $scope.filterByNature = function () {
            $scope.characteristic.notation = filterFilter($scope.notations, { Nature: $scope.natureId })[0];
        };

        $scope.isLinkable = IsLinkable;

        $scope.selectLink = SelectLink;

        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: filterFilter($scope.notations, { Nature: $scope.natureId })[0]
        };
    }

    angular.module("Accordance", []).controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
}
