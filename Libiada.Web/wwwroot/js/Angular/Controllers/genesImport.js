function GenesImportController(data) {
    "use strict";

    function genesImport($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("GenesImportCtrl", ["$scope", genesImport]);
}
