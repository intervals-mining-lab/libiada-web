function BatchMusicImportController(data) {
    "use strict";

    function batchMusicImport($scope) {
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
    }

    angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
}
