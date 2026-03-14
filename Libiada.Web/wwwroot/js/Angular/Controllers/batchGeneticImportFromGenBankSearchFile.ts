import { MapModelFromJson } from "functions";

/**
 * Interface for the data object passed from the server
 */
interface BatchGeneticImportFromGenBankSearchFileData {
}

/**
 * Interface for the angular controller's scope
 */
interface BatchGeneticImportFromGenBankSearchFileScope extends ng.IScope {
    // File status properties
    fileSelected: {value : boolean};

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
    constructor(data: BatchGeneticImportFromGenBankSearchFileData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: BatchGeneticImportFromGenBankSearchFileData): void {
        const batchGeneticImportFromGenBankSearchFile = ($scope: BatchGeneticImportFromGenBankSearchFileScope): void => {
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
export default function BatchGeneticImportFromGenBankSearchFileController(
    data: BatchGeneticImportFromGenBankSearchFileData
): BatchGeneticImportFromGenBankSearchFileHandler {
    return new BatchGeneticImportFromGenBankSearchFileHandler(data);
}
