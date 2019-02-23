function BatchMusicImportController(data) {
    "use strict";

    function batchMusicImport($scope) {
        MapModelFromJson($scope, data);

        $scope.notation = $scope.notations[0];
    }

    angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
}
