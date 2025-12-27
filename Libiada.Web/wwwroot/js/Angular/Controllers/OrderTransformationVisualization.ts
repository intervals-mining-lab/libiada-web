/// <reference types="angular" />

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
* Interface for the transformation type
*/
interface ITransformation {
    Text: string;
    Value: string;
    selected?: boolean;
}

/**
* Interface for the order transformer type
*/
interface IOrderTransformerType {
    Text: string;
    Value: string;
}

/**
* Interface for order transformation visualization controller data
*/
interface IOrderTransformationVisualizationData {
    // Basic data
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];
    transformationsPossible?: ITransformation[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;
    transformationsSelected?: ITransformation[];
}

/**
* Interface for order transformation visualization controller scope
*/
interface IOrderTransformationVisualizationScope extends ng.IScope {
    // Basic data
    researchObjects?: IResearchObject[];
    orderTransformerTypes?: IOrderTransformerType[];
    transformationsPossible?: ITransformation[];

    // Selected values
    selectedResearchObjects?: number[];
    orderTransformerType?: IOrderTransformerType;
    transformationsSelected?: ITransformation[];
}

/**
* Controller for visualizing order transformation
*/
class OrderTransformationVisualizationHandler {
    /**
    * Creates an instance of the order transformation visualization controller
    * @param data Data for initializing the controller
    */
    constructor(data: IOrderTransformationVisualizationData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: IOrderTransformationVisualizationData): void {
        const orderTransformationVisualization = ($scope: IOrderTransformationVisualizationScope): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformationVisualizationCtrl", ["$scope", orderTransformationVisualization]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation visualization controller instance
*/
function OrderTransformationVisualizationController(data: IOrderTransformationVisualizationData): OrderTransformationVisualizationHandler {
    return new OrderTransformationVisualizationHandler(data);
}
