import { initScopeFromServer } from "functions";
import { SequenceImportResult } from "viewDataTypes";

interface BatchGenesImportResultData {
    Results: SequenceImportResult[];
}

interface BatchGenesImportResultScope extends ng.IScope, BatchGenesImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchGenesImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchGenesImportResult = ($scope: BatchGenesImportResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<BatchGenesImportResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGenesImportResultCtrl", ["$scope", "$http", batchGenesImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genes import result handler
 */
export default function BatchGenesImportResultController(): BatchGenesImportResultHandler {
    return new BatchGenesImportResultHandler();
}
