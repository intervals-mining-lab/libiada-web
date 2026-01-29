import { initScopeFromServer } from "functions";
class BatchMusicImportResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchMusicImportResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchMusicImportResultCtrl", ["$scope", "$http", batchMusicImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch music import result handler
 */
export default function BatchMusicImportResultController() {
    return new BatchMusicImportResultHandler();
}
//# sourceMappingURL=batchMusicImportResult.js.map