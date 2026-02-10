import { MapModelFromJson } from "functions";
import type {
    SequenceType,
    Group
} from "viewDataTypes";

/**
 * Interface for the data object that is passed to the controller
 */
interface BatchGenesImportData {
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    sequenceTypes: SequenceType[];
}

/**
 * Interface for controller's scope
 */
interface BatchGenesImportScope extends ng.IScope, BatchGenesImportData {
    selectedResearchObjectsCount: number;
}

/**
 * Angular controller class
 */
class BatchGenesImportHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: BatchGenesImportData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: BatchGenesImportData): void {
        const batchGenesImport = ($scope: BatchGenesImportScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchGenesImportCtrl", ["$scope", batchGenesImport]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of genes import handler
 */
export default function BatchGenesImportController(data: BatchGenesImportData): BatchGenesImportHandler {
    return new BatchGenesImportHandler(data);
}
