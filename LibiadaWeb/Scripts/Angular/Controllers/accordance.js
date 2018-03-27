function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            $scope.characteristic.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        }

        $scope.filterByNature = filterByNature;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;


        $scope.nature = $scope.natures[0].Value;
        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: filterFilter($scope.notations, { Nature: $scope.nature })[0]
        };
    }

	angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
    mattersTable();
}
