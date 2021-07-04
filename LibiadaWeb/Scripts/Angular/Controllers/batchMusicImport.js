function BatchMusicImportController(data) {
    "use strict";

    function batchMusicImport($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
}
