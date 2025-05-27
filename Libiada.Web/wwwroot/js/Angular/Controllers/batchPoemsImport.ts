/// <reference types="angular" />

/**
 * Interface for object with boolean value
 */
interface IBooleanValue {
    value: boolean;
}

/**
 * Interface for notation
 */
interface INotation {
    id: number;
    name: string;
    // Possible additional notation properties
}

/**
 * Interface for initial controller data
 */
interface IBatchPoemsImportData {
    notations: INotation[];
    // Other possible initial data properties
}

/**
 * Interface for controller scope
 */
interface IBatchPoemsImportScope extends ng.IScope {
    // File status properties
    fileSelected: IBooleanValue;

    // Notation properties
    notations: INotation[];
    notation: INotation;

    // Methods
    fileChanged: (filePath: HTMLInputElement) => void;
}

/**
 * Controller for batch poems import
 */
class BatchPoemsImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IBatchPoemsImportData) {
        this.initializeController(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private initializeController(data: IBatchPoemsImportData): void {
        "use strict";

        const batchPoemsImport = ($scope: IBatchPoemsImportScope): void => {
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
            $scope.notation = $scope.notations[0];
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchPoemsImportCtrl", ["$scope", batchPoemsImport]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch poems import handler
 */
function BatchPoemsImportController(data: IBatchPoemsImportData): BatchPoemsImportHandler {
    return new BatchPoemsImportHandler(data);
}
