import { initScopeFromServer } from "functions";
import type { SequenceImportResult } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface BatchPoemsImportResultData {
    Results: SequenceImportResult[];
}

/**
 * Interface for the angular controller's scope
 */
interface BatchPoemsImportResultScope extends ng.IScope, BatchPoemsImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchPoemsImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchPoemsImportResult = ($scope: BatchPoemsImportResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<BatchPoemsImportResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchPoemsImportResultCtrl", ["$scope", "$http", batchPoemsImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch poems import result handler
 */
export default function BatchPoemsImportResultController(): BatchPoemsImportResultHandler {
    return new BatchPoemsImportResultHandler();
}
