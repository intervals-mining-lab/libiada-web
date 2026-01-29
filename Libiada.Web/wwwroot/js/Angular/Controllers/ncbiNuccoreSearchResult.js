import { initScopeFromServer } from "functions";
class NcbiNuccoreSearchResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const ncbiNuccoreSearchResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("NcbiNuccoreSearchResultCtrl", ["$scope", "$http", ncbiNuccoreSearchResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function NcbiNuccoreSearchResultController() {
    return new NcbiNuccoreSearchResultHandler();
}
//# sourceMappingURL=ncbiNuccoreSearchResult.js.map