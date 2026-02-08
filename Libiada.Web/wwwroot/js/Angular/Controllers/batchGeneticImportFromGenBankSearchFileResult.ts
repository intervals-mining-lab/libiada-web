import { initScopeFromServer } from "functions";
import type { SequenceImportResult } from "viewDataTypes";

interface BatchGeneticImportFromGenBankSearchFileResultData {
    Results: SequenceImportResult[];
}

interface BatchGeneticImportFromGenBankSearchFileResultScope extends ng.IScope, BatchGeneticImportFromGenBankSearchFileResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class BatchGeneticImportFromGenBankSearchFileResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchGeneticImportFromGenBankSearchFileResult = ($scope: BatchGeneticImportFromGenBankSearchFileResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<BatchGeneticImportFromGenBankSearchFileResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileResultCtrl", ["$scope", "$http", batchGeneticImportFromGenBankSearchFileResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search file result handler
 */
export default function BatchGeneticImportFromGenBankSearchFileResultController(): BatchGeneticImportFromGenBankSearchFileResultHandler {
    return new BatchGeneticImportFromGenBankSearchFileResultHandler();
}
