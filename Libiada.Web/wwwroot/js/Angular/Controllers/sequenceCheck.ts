import { MapModelFromJson } from "functions";
import type { Group, SequenceType } from "viewDataTypes"; 

/**
 * Interface for initial controller data
 */
interface SequenceCheckData {
    nature: string;
    groups: Group[];
    sequenceTypes: SequenceType[];

    minimumSelectedResearchObjects: number;
    maximumSelectedResearchObjects: number;
}

/**
 * Interface for controller scope
 */
interface SequenceCheckScope extends ng.IScope, SequenceCheckData {
    // Research objects selection
    selectedResearchObjectsCount: number;

    // File status
    fileSelected: { value: boolean };

    // Methods
    fileChanged: (filePath: HTMLInputElement) => void;

}

/**
 * Controller for sequence check
 */
class SequenceCheckOperator {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: SequenceCheckData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: SequenceCheckData): void {
        const sequenceCheck = ($scope: SequenceCheckScope): void => {
            MapModelFromJson($scope, data);

            /**
             * Handles file selection change event
             * @param filePath The input element with file path
             */
            function fileChanged(filePath: HTMLInputElement): void {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                } else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }

            // Initialize file selected status
            $scope.fileSelected = { value: false };

            $scope.fileChanged = fileChanged;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SequenceCheckCtrl", ["$scope", sequenceCheck]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of sequence check operator
 */
export default function SequenceCheckController(data: SequenceCheckData): SequenceCheckOperator {
    return new SequenceCheckOperator(data);
}
