/**
 * Interface for the angular controller's scope
 */
interface BatchImagesImportScope extends angular.IScope {
    fileSelected: { value: boolean; };
    fileChanged: (filePath: HTMLInputElement) => void;
}

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
    private ngOnInit(): void {
        const batchImagesImport = ($scope: BatchImagesImportScope): void => {

            function fileChanged(filePath: HTMLInputElement): void {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                } else {
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
export default function BatchImagesImportController(): BatchImagesImportHandler {
    return new BatchImagesImportHandler();
}
