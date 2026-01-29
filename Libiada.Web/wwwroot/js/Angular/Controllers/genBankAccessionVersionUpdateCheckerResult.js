import { initScopeFromServer } from "functions";
class GenBankAccessionVersionUpdateCheckerResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const genBankAccessionVersionUpdateCheckerResult = ($scope, $http) => {
            $scope.calculateStatusClass = (result) => {
                return result.Updated ? result.NameUpdated ? "table-warning" : "table-danger" : result.NameUpdated ? "" : "table-info";
            };
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("GenBankAccessionVersionUpdateCheckerResultCtrl", ["$scope", "$http", genBankAccessionVersionUpdateCheckerResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of GenBank accession version update check result handler
 */
export default function GenBankAccessionVersionUpdateCheckerResultController() {
    return new GenBankAccessionVersionUpdateCheckerResultHandler();
}
//# sourceMappingURL=genBankAccessionVersionUpdateCheckerResult.js.map