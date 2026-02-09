// Angular controller class
class NCBINuclorSearchHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const ncbiNuccoreSearch = ($scope) => {
            $scope.searchQuery = "";
            $scope.filterMinLength = false;
            $scope.filterMaxLength = false;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("NcbiNuccoreSearchCtrl", ["$scope", ncbiNuccoreSearch]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of NCBI Nuccore search handler
 */
export default function NcbiNuccoreSearchController() {
    return new NCBINuclorSearchHandler();
}
//# sourceMappingURL=ncbiNuccoreSearch.js.map