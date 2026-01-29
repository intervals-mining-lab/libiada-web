import { initScopeFromServer } from "functions";
class GenesImportResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const genesImportResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading genes import results", "Failed loading genes import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("GenesImportResultCtrl", ["$scope", "$http", genesImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of genes import result handler
 */
export default function GenesImportResultController() {
    return new GenesImportResultHandler();
}
//# sourceMappingURL=genesImportResult.js.map