function GenesImportController(data) {
    "use strict";

    function genesImport($scope) {
        MapModelFromJson($scope, data);

        $scope.localFile = false;

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
    }

    angular.module("GenesImport", []).controller("GenesImportCtrl", ["$scope", genesImport]);
}
