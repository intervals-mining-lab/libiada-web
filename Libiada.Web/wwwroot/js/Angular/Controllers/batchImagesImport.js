/**
 * Angular controller class
 */
class BatchImagesImportHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const batchImagesImport = ($scope) => {
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