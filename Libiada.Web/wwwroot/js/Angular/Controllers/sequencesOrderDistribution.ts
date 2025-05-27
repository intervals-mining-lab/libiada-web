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

}

/**
* Interface for the order transformation type
*/
interface IOrderTransformerType {
    Text: string;
    Value: string;
}

/**
* Interface for the order distribution controller data
*/
interface ISequencesOrderDistributionData {
    // Basic data properties
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;

    // Additional properties
    //[key: string]: any;
}

/**
* Interface for scope order distribution controller
*/
interface ISequencesOrderDistributionScope extends ng.IScope {
    // Basic data
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;

    // Additional properties
    [key: string]: any;
}

/**
* Controller for sequence order distribution
*/
class SequencesOrderDistributionHandler {
    private data: ISequencesOrderDistributionData;

    /**
    * Creates an instance of the order distribution controller
    * @param data Data to initialize the controller
    */
    constructor(data: ISequencesOrderDistributionData) {
        this.data = data;
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const sequencesOrderDistribution = ($scope: ISequencesOrderDistributionScope): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns Order distribution controller instance
*/
function SequencesOrderDistributionController(data: ISequencesOrderDistributionData): SequencesOrderDistributionHandler {
    return new SequencesOrderDistributionHandler(data);
}