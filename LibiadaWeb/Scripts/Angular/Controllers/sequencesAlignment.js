function SequencesAlignmentController(data) {
    "use strict";

    function sequencesAlignment($scope) {
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

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableMattersSelect = disableMattersSelect;
        $scope.disableSubmit = disableSubmit;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.selectedMatters = 0;
        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };

        $scope.subsequencesCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };
    }

    angular.module("SequencesAlignment", []).controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
}
