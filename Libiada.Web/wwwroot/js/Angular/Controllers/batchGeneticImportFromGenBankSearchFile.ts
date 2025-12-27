/// <reference types="angular" />

/**
 * Interface for object with boolean value
 */
interface IBooleanValue {
    value: boolean;
}

/**
 * Interface for initial controller data
 */
interface IBatchGeneticImportFromGenBankSearchFileData {
}

/**
 * Interface for controller scope
 */
interface IBatchGeneticImportFromGenBankSearchFileScope extends ng.IScope {
    // File status properties
    fileSelected: IBooleanValue;

    // Methods
    fileChanged: (filePath: HTMLInputElement) => void;
}

/**
 * Controller for batch genetic import from GenBank search file
 */
class BatchGeneticImportFromGenBankSearchFileHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IBatchGeneticImportFromGenBankSearchFileData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: IBatchGeneticImportFromGenBankSearchFileData): void {
        const batchGeneticImportFromGenBankSearchFile = ($scope: IBatchGeneticImportFromGenBankSearchFileScope): void => {
            MapModelFromJson($scope, data);

            function fileChanged(filePath: HTMLInputElement): void {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                } else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }

            $scope.fileChanged = fileChanged;

            $scope.fileSelected = { value: false };
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGeneticImportFromGenBankSearchFileCtrl",
            ["$scope", batchGeneticImportFromGenBankSearchFile]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch genetic import from GenBank search file handler
 */
function BatchGeneticImportFromGenBankSearchFileController(
    data: IBatchGeneticImportFromGenBankSearchFileData
): BatchGeneticImportFromGenBankSearchFileHandler {
    return new BatchGeneticImportFromGenBankSearchFileHandler(data);
}
