function BatchPoemsImportController(data) {
    "use strict";

    function batchPoemsImport($scope) {
        MapModelFromJson($scope, data);

        function fileChanged(filePath) {
            if (filePath.value) {
                $scope.fileSelected.value = true;
            } else {
                $scope.fileSelected.value = false;
            }
            $scope.$apply();
        }

        $scope.fileChanged = fileChanged;

        $scope.fileSelected = { value: false };
        $scope.notation = $scope.notations[0];
    }

    angular.module("libiada").controller("BatchPoemsImportCtrl", ["$scope", batchPoemsImport]);
}
