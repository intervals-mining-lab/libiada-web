/// <reference types="angular" />
/**
 * Controller for batch genetic import from GenBank search file
 */
class BatchGeneticImportFromGenBankSearchFileHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.initializeController(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    initializeController(data) {
        "use strict";
        const batchGeneticImportFromGenBankSearchFile = ($scope) => {
            MapModelFromJson($scope, data);
            function fileChanged(filePath) {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                }
                else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }
            $scope.fileChanged = fileChanged;
            $scope.fileSelected = { value: false };
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileCtrl", ["$scope", batchGeneticImportFromGenBankSearchFile]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch genetic import from GenBank search file handler
 */
function BatchGeneticImportFromGenBankSearchFileController(data) {
    return new BatchGeneticImportFromGenBankSearchFileHandler(data);
}
//# sourceMappingURL=batchGeneticImportFromGenBankSearchFile.js.map