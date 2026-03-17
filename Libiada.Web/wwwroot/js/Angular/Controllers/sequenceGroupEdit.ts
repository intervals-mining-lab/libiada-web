import { MapModelFromJson } from "functions";
import type { SequenceGroupType, Group, Nature, SequenceType, ResearchObject } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SequenceGroupEditData {
    groupIndex: number;
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    natures: Nature[];
    researchObjects: ResearchObject[];
    sequenceGroupTypeIndex: number;
    sequenceGroupTypes: SequenceGroupType[];
    sequenceTypeIndex: number;
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the angular controller's scope
 */
interface SequenceGroupEditScope extends ng.IScope, SequenceGroupEditData {
    selectedResearchObjectsCount: number;
    sequenceGroupType: string;
}

/**
 * Angular controller class for sequences group edit page
 */
class SequenceGroupEditHandler {
    constructor(data: SequenceGroupEditData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: SequenceGroupEditData): void {
        const sequenceGroupEdit = ($scope: SequenceGroupEditScope): void => {
            MapModelFromJson($scope, data);
            $scope.sequenceGroupTypeIndex ??= 0;
            $scope.sequenceGroupType = $scope.sequenceGroupTypes[$scope.sequenceGroupTypeIndex].Value;
        };

        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupEditCtrl", ["$scope", sequenceGroupEdit]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns SequenceGroupsEdit Controller instance
*/
export default function SequenceGroupEditController(data: SequenceGroupEditData): SequenceGroupEditHandler {
    return new SequenceGroupEditHandler(data);
}
