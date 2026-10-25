import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData, SequenceImportResult } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface BatchGeneticImportFromGenBankSearchQueryResultData {
    Results: SequenceImportResult[];
}

/**
 * Interface for the angular controller's scope
 */
interface BatchGeneticImportFromGenBankSearchQueryResultScope
    extends ng.IScope, BatchGeneticImportFromGenBankSearchQueryResultData, ResultsLoadingScreenData {
}

class BatchGeneticImportFromGenBankSearchQueryResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchGeneticImportFromGenBankSearchQueryResult = (
            $scope: BatchGeneticImportFromGenBankSearchQueryResultScope, 
            $http: ng.IHttpService): void => {
            
                initScopeFromServer<BatchGeneticImportFromGenBankSearchQueryResultData>(
                    $http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchQueryResultCtrl", 
            ["$scope", "$http", batchGeneticImportFromGenBankSearchQueryResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search query result handler
 */
export default function BatchGeneticImportFromGenBankSearchQueryResultController(): BatchGeneticImportFromGenBankSearchQueryResultHandler {
    return new BatchGeneticImportFromGenBankSearchQueryResultHandler();
}
