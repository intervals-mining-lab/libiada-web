/**
 * Angular controller class
 */
class BatchImagesImportHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const batchImagesImport = ($scope) => {
            $scope.fileChanged = function (filePath) {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                }
                else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            };
            $scope.fileSelected = { value: false };
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchImagesImportCtrl", ["$scope", batchImagesImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch images import handler
 */
export default function BatchImagesImportController() {
    return new BatchImagesImportHandler();
}
//# sourceMappingURL=batchImagesImport.js.map