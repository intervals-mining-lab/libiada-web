import { MapModelFromJson } from "functions";
import type { Notation } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface BatchPoemsImportData {
    notations: Notation[];
}

/**
 * Interface for the angular controller's scope
 */
interface BatchPoemsImportScope extends ng.IScope, BatchPoemsImportData {
    notation: Notation;
    fileSelected: { value: boolean; };

    fileChanged: (filePath: HTMLInputElement) => void;
}

/**
 * Controller for batch poems import
 */
class BatchPoemsImportHandler {
    constructor(data: BatchPoemsImportData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: BatchPoemsImportData): void {
        const batchPoemsImport = ($scope: BatchPoemsImportScope): void => {
            MapModelFromJson($scope, data);

            $scope.fileChanged = function (filePath: HTMLInputElement): void {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                } else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }

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
export default function BatchPoemsImportController(data: BatchPoemsImportData): BatchPoemsImportHandler {
    return new BatchPoemsImportHandler(data);
}
