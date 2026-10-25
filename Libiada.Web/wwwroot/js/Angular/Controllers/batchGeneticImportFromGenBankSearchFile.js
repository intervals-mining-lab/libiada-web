/**
 * Controller for batch genetic import from GenBank search file
 */
class BatchGeneticImportFromGenBankSearchFileHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit() {
        const batchGeneticImportFromGenBankSearchFile = ($scope) => {
            $scope.fileChanged = function (filePath) {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                }
                else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            };
            $scope.fileSelected = { value: false };
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileCtrl", ["$scope", batchGeneticImportFromGenBankSearchFile]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch genetic import from GenBank search file handler
 */
export default function BatchGeneticImportFromGenBankSearchFileController() {
    return new BatchGeneticImportFromGenBankSearchFileHandler();
}
//# sourceMappingURL=batchGeneticImportFromGenBankSearchFile.js.map