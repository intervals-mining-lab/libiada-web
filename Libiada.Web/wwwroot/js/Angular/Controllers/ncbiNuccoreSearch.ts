/**
 * Interface for the controller's scope
 */
interface NCBINuccoreSearchScope extends ng.IScope {
    searchQuery: string;
    filterMinLength: boolean;
    filterMaxLength: boolean;
}

// Angular controller class
class NCBINuccoreSearchHandler {
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
        const ncbiNuccoreSearch = ($scope: NCBINuccoreSearchScope): void => {
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
export default function NcbiNuccoreSearchController(): NCBINuccoreSearchHandler {
    return new NCBINuccoreSearchHandler();
}
