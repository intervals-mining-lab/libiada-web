import { initScopeFromServer } from "functions";
class BatchGeneticImportFromGenBankSearchFileResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const batchGeneticImportFromGenBankSearchFileResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileResultCtrl", ["$scope", "$http", batchGeneticImportFromGenBankSearchFileResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search file result handler
 */
export default function BatchGeneticImportFromGenBankSearchFileResultController() {
    return new BatchGeneticImportFromGenBankSearchFileResultHandler();
}
//# sourceMappingURL=batchGeneticImportFromGenBankSearchFileResult.js.map