function CalculationController(data) {
    "use strict";

    function calculation($scope, filterFilter) {
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

        $scope.characteristics = [];
        $scope.notationsFiltered = [];

        $scope.natureId = $scope.natures[0].Value;

        var filterByNature = function () {
            FilterOptionsByNature($scope, filterFilter, "notations");
            var notation = $scope.notationsFiltered[0];

            angular.forEach($scope.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        };

        $scope.isLinkable = function (characteristic) {
            return characteristic.characteristicType.CharacteristicTypeLink.length > 1;
        };

        $scope.getLinks = function (characteristicType) {
            var characteristicTypeLinks = characteristicType.CharacteristicTypeLink;
            var links = [];
            for (var i = 0; i < characteristicTypeLinks.length; i++) {
                for (var j = 0; j < $scope.links.length; j++) {
                    if ($scope.links[j].CharacteristicTypeLink.indexOf(characteristicTypeLinks[i]) !== -1) {
                        links.push($scope.links[j]);
                    }
                }
            }

            return links;
        };

        $scope.addCharacteristic = function () {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0], 
                link: $scope.getLinks($scope.characteristicTypes[0])[0],
                notation: $scope.notationsFiltered[0],
                language: $scope.languages[0],
                translator: $scope.translators[0]
            });
        };

        $scope.deleteCharacteristic = function (characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        };

        $scope.$watch("natureId", filterByNature, true);
    }

    angular.module("Calculation", []).controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
}
