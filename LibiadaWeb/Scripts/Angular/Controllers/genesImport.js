function GenesImportController(data) {
    "use strict";

    function genesImport($scope) {
        MapModelFromJson($scope, data);

        $scope.localFile = false;
    }

    angular.module("libiada", []).controller("GenesImportCtrl", ["$scope", genesImport]);
    mattersTable();
}
