import { initScopeFromServer } from "functions";
import type { SequenceImportResult } from "viewDataTypes";

interface BatchImagesImportResultData {
    Results: SequenceImportResult[];
}

interface BatchImagesImportResultScope extends ng.IScope, BatchImagesImportResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchImagesImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchImagesImportResult = ($scope: BatchImagesImportResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<BatchImagesImportResultData>($http, $scope, "Loading import results", "Failed loading images import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchImagesImportResultCtrl", ["$scope", "$http", batchImagesImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch images import result handler
 */
export default function BatchImagesImportResultController(): BatchImagesImportResultHandler {
    return new BatchImagesImportResultHandler();
}
