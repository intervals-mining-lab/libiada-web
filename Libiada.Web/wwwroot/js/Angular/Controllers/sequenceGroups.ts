import { MapModelFromJson } from "functions";

/**
* Interface for the object being researched
*/
interface IResearchObject {
    id: number;
    name: string;
    nature?: number;
    group?: number;
    sequenceType?: number;
    selected?: boolean;
}

/**
* Interface for the group type
*/
interface IGroup {
    id: number;
    name: string;
}

/**
* Interface for the sequence nature
*/
interface INature {
    id: number;
    name: string;
}

/**
* Interface for the sequence type
*/
interface ISequenceType {
    id: number;
    name: string;
}

/**
 * Interface for the data object passed from the server
 */
interface ISequenceGroupsData {
    // Basic data
    researchObjects?: IResearchObject[];
    natures?: INature[];
    groups?: IGroup[];
    sequenceTypes?: ISequenceType[];

    // Selected data
    selectedResearchObjects?: number[];
    newGroupName?: string;
    selectedNature?: INature;
    selectedGroup?: IGroup;
    selectedSequenceType?: ISequenceType;
}

/**
 * Interface for the angular controller's scope
 */
interface ISequenceGroupsScope extends ng.IScope {
    // Basic data
    researchObjects?: IResearchObject[];
    natures?: INature[];
    groups?: IGroup[];
    sequenceTypes?: ISequenceType[];

    // Selected data
    selectedResearchObjects?: number[];
    newGroupName?: string;
    selectedNature?: INature;
    selectedGroup?: IGroup;
    selectedSequenceType?: ISequenceType;

    // Methods
    selectAll?: () => void;
    deselectAll?: () => void;
}

/**
* Controller for sequence groups
*/
class SequenceGroupsHandler {
    /**
    * Creates an instance of the sequence groups controller
    * @param data Data to initialize the controller
    */
    constructor(data: ISequenceGroupsData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: ISequenceGroupsData): void {
        const sequenceGroups = ($scope: ISequenceGroupsScope): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupsCtrl", ["$scope", sequenceGroups]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns Sequence Groups Controller instance
*/
export default function SequenceGroupsController(data: ISequenceGroupsData): SequenceGroupsHandler {
    return new SequenceGroupsHandler(data);
}
