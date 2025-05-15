/// <reference types="angular" />

/**
* Interface for the research object
*/
interface IResearchObject {
    id: number;
    name: string;
    nature?: number;
    group?: number;
    sequenceType?: number;
    selected?: boolean;
    [key: string]: any;
}

/**
* Interface for the alignment type
*/
interface IAlignerType {
    Text: string;
    Value: string;
}

/**
* Interface for the similarity type
*/
interface ISimilarityType {
    Text: string;
    Value: string;
}

/**
* Interface for the sequence alignment controller data
*/
interface ISequencesAlignmentData {
    // Basic data properties
    researchObjects?: IResearchObject[];
    alignerTypes?: IAlignerType[];
    similarityTypes?: ISimilarityType[];

    // Selected values
    selectedResearchObjects?: number[];
    alignerType?: IAlignerType;
    similarityType?: ISimilarityType;

    // Additional properties
    [key: string]: any;
}

/**
* Interface for the sequence alignment scope controller
*/
interface ISequencesAlignmentScope extends ng.IScope {
    // Basic data
    researchObjects?: IResearchObject[];
    alignerTypes?: IAlignerType[];
    similarityTypes?: ISimilarityType[];

    // Selected values
    selectedResearchObjects?: number[];
    alignerType?: IAlignerType;
    similarityType?: ISimilarityType;

    // Additional properties
    [key: string]: any;
}

/**
* Sequence alignment controller
*/
class SequencesAlignmentHandler {
    private data: ISequencesAlignmentData;

    /**
    * Creates an instance of the sequence alignment controller
    * @param data Data for initializing the controller
    */
    constructor(data: ISequencesAlignmentData) {
        this.data = data;
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const sequencesAlignment = ($scope: ISequencesAlignmentScope): void => {
            // Initialize the scope with data from the parameter
            MapModelFromJson($scope, this.data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for initializing the controller
* @returns An instance of the sequence alignment controller
*/
function SequencesAlignmentController(data: ISequencesAlignmentData): SequencesAlignmentHandler {
    return new SequencesAlignmentHandler(data);
}