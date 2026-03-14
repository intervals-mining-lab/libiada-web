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
    CharacteristicType,
    OrderTransformation
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface OrderTransformationCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
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
 * Interface for the controller's scope
 */
interface OrderTransformationCalculationScope extends ng.IScope, OrderTransformationCalculationData {
    nature: string;
    selectedResearchObjectsCount: number;
}
/**
* Angular controller class
*/
class OrderTransformationCalculationHandler {
    /**
    * Creates an instance of the order transformation calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data: OrderTransformationCalculationData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: OrderTransformationCalculationData): void {
        const orderTransformationCalculation = ($scope: OrderTransformationCalculationScope, filterFilter: ng.IFilterFilter): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("OrderTransformationCalculationCtrl", ["$scope", "filterFilter", orderTransformationCalculation]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation calculation controller instance
*/
export default function OrderTransformationCalculationController(data: OrderTransformationCalculationData): OrderTransformationCalculationHandler {
    return new OrderTransformationCalculationHandler(data);
}
