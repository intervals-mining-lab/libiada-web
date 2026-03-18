import { MapModelFromJson } from "functions";
import type { SequenceGroupType, Group, Nature, SequenceType } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SequenceGroupCreateData {
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    natures: Nature[];
    sequenceGroupTypes: SequenceGroupType[];
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the angular controller's scope
 */
interface SequenceGroupCreateScope extends ng.IScope, SequenceGroupCreateData {
    selectedResearchObjectsCount: number;
    sequenceGroupType: string;
}

/**
 * Angular controller class for sequences group creation page
 */
class SequenceGroupCreateHandler {
    constructor(data: SequenceGroupCreateData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: SequenceGroupCreateData): void {
        const sequenceGroupCreate = ($scope: SequenceGroupCreateScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupCreateCtrl", ["$scope", sequenceGroupCreate]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns SequenceGroupCreateHandler instance
*/
export default function SequenceGroupCreateController(data: SequenceGroupCreateData): SequenceGroupCreateHandler {
    return new SequenceGroupCreateHandler(data);
}
