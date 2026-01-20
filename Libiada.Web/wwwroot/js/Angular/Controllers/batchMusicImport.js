import { MapModelFromJson } from "functions";
/**
 * Controller for batch music import
 */
class BatchMusicImportHandler {
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
        const batchMusicImport = ($scope) => {
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
        angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch music import handler
 */
export default function BatchMusicImportController(data) {
    return new BatchMusicImportHandler(data);
}
//# sourceMappingURL=batchMusicImport.js.map