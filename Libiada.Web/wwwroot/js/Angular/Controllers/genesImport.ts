import { MapModelFromJson } from "functions";
import type {
    SequenceType,
    Group
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface GenesImportData {
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    sequenceTypes: SequenceType[];
}

/**
 * Interface for controller's scope
 */
interface GenesImportScope extends ng.IScope, GenesImportData {
    selectedResearchObjectsCount: number;
}

/**
 * Angular controller class
 */
class GenesImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: GenesImportData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: GenesImportData): void {
        const genesImport = ($scope: GenesImportScope): void => {
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
export default function GenesImportController(data: GenesImportData): GenesImportHandler {
    return new GenesImportHandler(data);
}
