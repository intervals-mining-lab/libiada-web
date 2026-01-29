import { initScopeFromServer } from "functions";
class BatchGeneticImportFromGenBankSearchQueryResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchGeneticImportFromGenBankSearchQueryResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchQueryResultCtrl", ["$scope", "$http", batchGeneticImportFromGenBankSearchQueryResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search query result handler
 */
export default function BatchGeneticImportFromGenBankSearchQueryResultController() {
    return new BatchGeneticImportFromGenBankSearchQueryResultHandler();
}
//# sourceMappingURL=batchGeneticImportFromGenBankSearchQueryResult.js.map