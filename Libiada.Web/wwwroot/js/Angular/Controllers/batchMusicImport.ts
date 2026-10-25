/**
 * Interface for the angular controller's scope
 */
interface BatchMusicImportScope extends ng.IScope {
    fileSelected: { value: boolean; };

    fileChanged: (filePath: HTMLInputElement) => void;
}

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
    private ngOnInit(): void {
        const batchMusicImport = ($scope: BatchMusicImportScope): void => {

            $scope.fileChanged = function (filePath: HTMLInputElement): void {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                } else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }

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
export default function BatchMusicImportController(): BatchMusicImportHandler {
    return new BatchMusicImportHandler();
}
