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
interface IBatchMusicImportData {
    // Define properties that would be passed to the controller
    // Add specific properties as needed based on actual data
    [key: string]: any;
}

/**
 * Interface for controller scope
 */
interface IBatchMusicImportScope extends ng.IScope {
    // File status properties
    fileSelected: IBooleanValue;

    // Methods
    fileChanged: (filePath: HTMLInputElement) => void;
}

/**
 * Controller for batch music import
 */
class BatchMusicImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IBatchMusicImportData) {
        this.initializeController(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private initializeController(data: IBatchMusicImportData): void {
        "use strict";

        const batchMusicImport = ($scope: IBatchMusicImportScope): void => {
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
        angular.module("libiada").controller("BatchMusicImportCtrl", ["$scope", batchMusicImport]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of batch music import handler
 */
function BatchMusicImportController(data: IBatchMusicImportData): BatchMusicImportHandler {
    return new BatchMusicImportHandler(data);
}
