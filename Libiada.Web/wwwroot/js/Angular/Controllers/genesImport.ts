/// <reference types="angular" />

/**
 * Interface for genes import data
 */
interface IGenesImportData {

}

/**
 * Interface for controller scope
 */
interface IGenesImportScope extends ng.IScope {

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
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: IGenesImportData): void {
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
