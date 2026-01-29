import { initScopeFromServer } from "functions";
import { SequenceImportResult } from "viewDataTypes";

interface BatchMusicImportResultData {
    Results: SequenceImportResult[];
}

interface BatchMusicImportResultScope extends ng.IScope, BatchMusicImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchMusicImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchMusicImportResult = ($scope: BatchMusicImportResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<BatchMusicImportResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchMusicImportResultCtrl", ["$scope", "$http", batchMusicImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch music import result handler
 */
export default function BatchMusicImportResultController(): BatchMusicImportResultHandler {
    return new BatchMusicImportResultHandler();
}
