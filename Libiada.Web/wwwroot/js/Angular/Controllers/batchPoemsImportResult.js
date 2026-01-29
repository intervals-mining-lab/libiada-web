import { initScopeFromServer } from "functions";
class BatchPoemsImportResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchPoemsImportResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchPoemsImportResultCtrl", ["$scope", "$http", batchPoemsImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch poems import result handler
 */
export default function BatchPoemsImportResultController() {
    return new BatchPoemsImportResultHandler();
}
//# sourceMappingURL=batchPoemsImportResult.js.map