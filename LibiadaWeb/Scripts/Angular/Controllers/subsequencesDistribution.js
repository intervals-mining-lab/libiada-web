function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        }

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        }

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: $scope.notations[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        }

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;
        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.disableMattersSelect = FakeDisableMattersSelect;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.selectedMatters = 0;
        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };
        $scope.characteristics = [];
    }

    angular.module("SubsequencesDistribution", []).controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
}
