"use strict";
/// <reference types="angular" />
/**
 * Controller for batch poems import
 */
class BatchPoemsImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        "use strict";
        const batchPoemsImport = ($scope) => {
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
            $scope.notation = $scope.notations[0];
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchPoemsImportCtrl", ["$scope", batchPoemsImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch poems import handler
 */
function BatchPoemsImportController(data) {
    return new BatchPoemsImportHandler(data);
}
//# sourceMappingURL=batchPoemsImport.js.map