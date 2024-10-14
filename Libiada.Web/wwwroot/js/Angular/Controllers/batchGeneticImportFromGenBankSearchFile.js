function BatchGeneticImportFromGenBankSearchFileController(data) {
    "use strict";

    function batchGeneticImportFromGenBankSearchFile($scope) {
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
    
    angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileCtrl", ["$scope", batchGeneticImportFromGenBankSearchFile]);
}
