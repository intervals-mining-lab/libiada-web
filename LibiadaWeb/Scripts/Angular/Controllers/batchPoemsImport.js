function BatchPoemsImportController(data) {
    "use strict";

    function batchPoemsImport($scope) {
        MapModelFromJson($scope, data);

        $scope.notation = $scope.notations[0];
    }

    angular.module("libiada").controller("BatchPoemsImportCtrl", ["$scope", batchPoemsImport]);
}
