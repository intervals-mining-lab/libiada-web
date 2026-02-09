/**
 * Interface for the controller's scope
 */
interface GenBankAccessionVersionUpdateCheckerScope extends angular.IScope {
}

/**
 * Angular controller class
 */
class GenBankAccessionVersionUpdateCheckerHandler {
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
        const genBankAccessionVersionUpdateChecker = ($scope: GenBankAccessionVersionUpdateCheckerScope): void => {
        };

        // Register controller in Angular module
        angular.module("libiada").controller("GenBankAccessionVersionUpdateCheckerCtrl", ["$scope", genBankAccessionVersionUpdateChecker]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of GenBank accession version update checker handler
 */
export default function GenBankAccessionVersionUpdateCheckerController(): GenBankAccessionVersionUpdateCheckerHandler {
    return new GenBankAccessionVersionUpdateCheckerHandler();
}
