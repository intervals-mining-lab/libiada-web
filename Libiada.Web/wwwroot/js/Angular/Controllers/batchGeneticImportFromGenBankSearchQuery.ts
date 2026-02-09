/**
 * Interface for controller scope
 */
interface BatchGeneticImportFromGenBankSearchQueryScope extends angular.IScope {
    searchQuery: string;
    filterMinLength: boolean;
    filterMaxLength: boolean;
}

/**
 * Controller for batch genetic import from GenBank search query
 */
class BatchGeneticImportFromGenBankSearchQueryHandler {
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
        const batchGeneticImportFromGenBankSearchQuery = ($scope: BatchGeneticImportFromGenBankSearchQueryScope): void => {

            $scope.searchQuery = "";
            $scope.filterMinLength = false;
            $scope.filterMaxLength = false;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchQueryCtrl", ["$scope", batchGeneticImportFromGenBankSearchQuery]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch genetic import from GenBank search query handler
 */
export default function BatchGeneticImportFromGenBankSearchQueryController(): BatchGeneticImportFromGenBankSearchQueryHandler {
    return new BatchGeneticImportFromGenBankSearchQueryHandler();
}
