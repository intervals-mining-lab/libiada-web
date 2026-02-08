import { initScopeFromServer } from "functions";
import type { SequenceImportResult } from "viewDataTypes";

interface BatchGeneticImportFromGenBankSearchQueryResultData {
    Results: SequenceImportResult[];
}

interface BatchGeneticImportFromGenBankSearchQueryResultScope extends ng.IScope, BatchGeneticImportFromGenBankSearchQueryResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchGeneticImportFromGenBankSearchQueryResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchGeneticImportFromGenBankSearchQueryResult = ($scope: BatchGeneticImportFromGenBankSearchQueryResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<BatchGeneticImportFromGenBankSearchQueryResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchQueryResultCtrl", ["$scope", "$http", batchGeneticImportFromGenBankSearchQueryResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search query result handler
 */
export default function BatchGeneticImportFromGenBankSearchQueryResultController(): BatchGeneticImportFromGenBankSearchQueryResultHandler {
    return new BatchGeneticImportFromGenBankSearchQueryResultHandler();
}
