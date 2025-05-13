/// <reference types="angular" />
/// <reference types="functions" />

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
    //[key: string]: any;
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
* Interface for sequence group controller data
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

    // Additional properties
    [key: string]: any;
}

/**
* Interface for sequence group controller scope
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
    toggleResearchObjectSelection?: (researchObject: IResearchObject) => void;
    selectAll?: () => void;
    deselectAll?: () => void;

    // Additional properties
    [key: string]: any;
}

/**
* Controller for sequence groups
*/
class SequenceGroupsHandler {
    private data: ISequenceGroupsData;

    /**
    * Creates an instance of the sequence groups controller
    * @param data Data to initialize the controller
    */
    constructor(data: ISequenceGroupsData) {
        this.data = data;
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const sequenceGroups = ($scope: ISequenceGroupsScope): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);
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
function SequenceGroupsController(data: ISequenceGroupsData): SequenceGroupsHandler {
    return new SequenceGroupsHandler(data);
}