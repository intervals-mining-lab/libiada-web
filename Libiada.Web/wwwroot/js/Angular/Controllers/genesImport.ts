/// <reference types="angular" />

/**
 * Interface for genes import data
 */
interface IGenesImportData {
    // Define properties that would be passed to the controller
    // Add specific properties as needed based on actual data
    [key: string]: any;
}

/**
 * Interface for controller scope
 */
interface IGenesImportScope extends ng.IScope {
    // Add specific properties as needed based on actual scope usage
    [key: string]: any;
}

/**
 * Controller for genes import functionality
 */
class GenesImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IGenesImportData) {
        this.initializeController(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private initializeController(data: IGenesImportData): void {
        "use strict";

        const genesImport = ($scope: IGenesImportScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("GenesImportCtrl", ["$scope", genesImport]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of genes import handler
 */
function GenesImportController(data: IGenesImportData): GenesImportHandler {
    return new GenesImportHandler(data);
}
