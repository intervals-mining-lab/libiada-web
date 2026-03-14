import { initScopeFromServer } from "functions";
import type { SequenceImportResult } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface NcbiNuccoreSearchResultData {
    Results: SequenceImportResult[];
    accessions: string[];
}

/**
 * Interface for the angular controller's scope
 */
interface NcbiNuccoreSearchResultScope extends ng.IScope, NcbiNuccoreSearchResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class NcbiNuccoreSearchResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const ncbiNuccoreSearchResult = ($scope: NcbiNuccoreSearchResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<NcbiNuccoreSearchResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("NcbiNuccoreSearchResultCtrl", ["$scope", "$http", ncbiNuccoreSearchResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function NcbiNuccoreSearchResultController(): NcbiNuccoreSearchResultHandler {
    return new NcbiNuccoreSearchResultHandler();
}
