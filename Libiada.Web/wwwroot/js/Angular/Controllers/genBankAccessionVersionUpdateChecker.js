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
    ngOnInit() {
        const genBankAccessionVersionUpdateChecker = () => {
        };
        // Register controller in Angular module
        angular.module("libiada").controller("GenBankAccessionVersionUpdateCheckerCtrl", [genBankAccessionVersionUpdateChecker]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of GenBank accession version update checker handler
 */
export default function GenBankAccessionVersionUpdateCheckerController() {
    return new GenBankAccessionVersionUpdateCheckerHandler();
}
//# sourceMappingURL=genBankAccessionVersionUpdateChecker.js.map