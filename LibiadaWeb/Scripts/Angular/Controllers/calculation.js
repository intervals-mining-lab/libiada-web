function CalculationController(data) {
    "use strict";

    function calculation($scope, filterFilter) {
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

        function filterByNature() {
            var notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

            // if notation is not linked to characteristic
            $scope.notation = notation;

            // if notation is part of characterisitcs
            angular.forEach($scope.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        }

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                // if notation is part of characterisitcs
                notation: filterFilter($scope.notations, { Nature: $scope.nature })[0],
                language: $scope.languages[0],
                translator: $scope.translators[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        }


        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;
        $scope.filterByNature = filterByNature;
        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.disableMattersSelect = FakeDisableMattersSelect;

        $scope.selectedMatters = 0;
        $scope.characteristics = [];
        $scope.nature = $scope.natures[0].Value;
        // if notation is not linked to characteristic
        $scope.language = $scope.languages[0];
        $scope.translator = $scope.translators[0];
    }

    angular.module("Calculation", []).controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
}
