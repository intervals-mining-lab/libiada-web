import { MapModelFromJson } from "functions";
import type {
    SequenceType,
    Nature,
    Notation,
    Language,
    PauseTreatment,
    Trajectory,
    Translator,
    Group,
    OrderTransformation
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface OrderTransformerData {
    groups: Group[];
    languages: Language[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    natures: Nature[];
    notations: Notation[];
    pauseTreatments: PauseTreatment[];
    sequenceTypes: SequenceType[];
    trajectories: Trajectory[];
    transformations: OrderTransformation[];
    translators: Translator[];
}

/**
 * Interface for the angular controller's scope
 */
interface OrderTransformerScope extends ng.IScope, OrderTransformerData {
    nature: string;
    selectedResearchObjectsCount: number;
}
/**
* Angular controller class
*/
class OrderTransformerHandler {
    /**
    * Creates an instance of the order transformation controller
    * @param data Data for initializing the controller
    */
    constructor(data: OrderTransformerData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: OrderTransformerData): void {
        const orderTransformer = ($scope: OrderTransformerScope, filterFilter: ng.IFilterFilter): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation controller instance
*/
export default function OrderTransformerController(data: OrderTransformerData): OrderTransformerHandler {
    return new OrderTransformerHandler(data);
}
