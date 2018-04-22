function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.nature = $scope.natures[0].Value;
    }

	angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
    mattersTable();
    characteristic();
}
