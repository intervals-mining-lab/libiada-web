import { initScopeFromServer } from "functions";
class BatchImagesImportResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchImagesImportResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading images import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchImagesImportResultCtrl", ["$scope", "$http", batchImagesImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch images import result handler
 */
export default function BatchImagesImportResultController() {
    return new BatchImagesImportResultHandler();
}
//# sourceMappingURL=batchImagesImportResult.js.map