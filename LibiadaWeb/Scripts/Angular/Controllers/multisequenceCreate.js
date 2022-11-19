function MultisequenceCreateController(data) {
    "use strict";

    function multisequenceCreate($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {

        }

        $scope.filterByNature = filterByNature;

        $scope.nature = $scope.natures[0].Value;
        $scope.name = "";
        $scope.displayMultisequenceNumber = true;
    }

    angular.module("libiada").controller("MultisequenceCreateCtrl", ["$scope", "filterFilter", multisequenceCreate]);
}
