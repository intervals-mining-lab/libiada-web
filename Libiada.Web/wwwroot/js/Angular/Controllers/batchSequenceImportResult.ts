import { initScopeFromServer } from "functions";
import { SequenceImportResult } from "viewDataTypes";

interface BatchSequenceImportResultData {
    Results: SequenceImportResult[];
}

interface BatchSequenceImportResultScope extends ng.IScope, BatchSequenceImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchSequenceImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchSequenceImportResult = ($scope: BatchSequenceImportResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<BatchSequenceImportResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportResultCtrl", ["$scope", "$http", batchSequenceImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function BatchSequenceImportResultController(): BatchSequenceImportResultHandler {
    return new BatchSequenceImportResultHandler();
}
