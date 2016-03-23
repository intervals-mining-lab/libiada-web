function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        }

        function disableMattersSelect(matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        }

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        }

        function filterByNature() {
            $scope.characteristic.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        }

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableMattersSelect = disableMattersSelect;
        $scope.disableSubmit = disableSubmit;
        $scope.filterByNature = filterByNature;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;

        $scope.selectedMatters = 0;
        $scope.nature = $scope.natures[0].Value;
        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: filterFilter($scope.notations, { Nature: $scope.nature })[0]
        };
    }

    angular.module("Accordance", []).controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
}
