/**
 * Interface for the controller's scope
 */
interface NCBINuclorSearchScope extends angular.IScope {
    searchQuery: string;
    filterMinLength: boolean;
    filterMaxLength: boolean;
}

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
    private ngOnInit(): void {
        const ncbiNuccoreSearch = ($scope: NCBINuclorSearchScope): void => {

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
export default function NcbiNuccoreSearchController(): NCBINuclorSearchHandler {
    return new NCBINuclorSearchHandler();
}
