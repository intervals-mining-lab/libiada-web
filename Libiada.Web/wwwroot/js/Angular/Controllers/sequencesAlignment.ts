import { MapModelFromJson } from "functions";
import { ResearchObject } from "viewDataTypes";

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
    researchObjects?: ResearchObject[];
    alignerTypes?: IAlignerType[];
    similarityTypes?: ISimilarityType[];

    // Selected values
    selectedResearchObjects?: number[];
    alignerType?: IAlignerType;
    similarityType?: ISimilarityType;
}

/**
* Interface for the sequence alignment scope controller
*/
interface ISequencesAlignmentScope extends ng.IScope {
    // Basic data
    researchObjects?: ResearchObject[];
    alignerTypes?: IAlignerType[];
    similarityTypes?: ISimilarityType[];

    // Selected values
    selectedResearchObjects?: number[];
    alignerType?: IAlignerType;
    similarityType?: ISimilarityType;
}

/**
* Sequence alignment controller
*/
class SequencesAlignmentHandler {
    /**
    * Creates an instance of the sequence alignment controller
    * @param data Data for initializing the controller
    */
    constructor(data: ISequencesAlignmentData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: ISequencesAlignmentData): void {
        const sequencesAlignment = ($scope: ISequencesAlignmentScope): void => {
            // Initialize the scope with data from the parameter
            MapModelFromJson($scope, data);
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
export default function SequencesAlignmentController(data: ISequencesAlignmentData): SequencesAlignmentHandler {
    return new SequencesAlignmentHandler(data);
}
