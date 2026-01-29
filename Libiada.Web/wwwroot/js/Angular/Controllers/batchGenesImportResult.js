import { initScopeFromServer } from "functions";
class BatchGenesImportResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchGenesImportResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchGenesImportResultCtrl", ["$scope", "$http", batchGenesImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genes import result handler
 */
export default function BatchGenesImportResultController() {
    return new BatchGenesImportResultHandler();
}
//# sourceMappingURL=batchGenesImportResult.js.map