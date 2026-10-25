/**
 * Controller for batch music import
 */
class BatchMusicImportHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const batchMusicImport = ($scope) => {
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
        angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch music import handler
 */
export default function BatchMusicImportController() {
    return new BatchMusicImportHandler();
}
//# sourceMappingURL=batchMusicImport.js.map