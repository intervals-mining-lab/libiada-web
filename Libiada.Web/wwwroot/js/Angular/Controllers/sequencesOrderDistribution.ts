import { MapModelFromJson } from "functions";

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
 * Interface for the data object passed from the server
 */
interface ISequencesOrderDistributionData {
    // Basic data properties
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;
}

/**
 * Interface for the angular controller's scope
 */
interface ISequencesOrderDistributionScope extends ng.IScope {
    // Basic data
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;
}

/**
* Controller for sequence order distribution
*/
class SequencesOrderDistributionHandler {
    /**
    * Creates an instance of the order distribution controller
    * @param data Data to initialize the controller
    */
    constructor(data: ISequencesOrderDistributionData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: ISequencesOrderDistributionData): void {
        const sequencesOrderDistribution = ($scope: ISequencesOrderDistributionScope): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
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
export default function SequencesOrderDistributionController(data: ISequencesOrderDistributionData): SequencesOrderDistributionHandler {
    return new SequencesOrderDistributionHandler(data);
}
