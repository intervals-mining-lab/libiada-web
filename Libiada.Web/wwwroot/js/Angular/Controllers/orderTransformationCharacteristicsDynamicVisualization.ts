import { MapModelFromJson } from "functions";
import type {
    Nature,
    Notation,
    Language,
    Translator,
    PauseTreatment,
    Trajectory,
    OrderTransformation,
    SequenceType,
    Group,
    CharacteristicType
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface OrderTransformationCharacteristicsDynamicVisualizationData {
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
    translators: Translator[];
    transformations: OrderTransformation[];
}

/**
 * Interface for the controller's scope
 */
interface OrderTransformationCharacteristicsDynamicVisualizationScope extends angular.IScope, OrderTransformationCharacteristicsDynamicVisualizationData {
    nature: string;
    selectedResearchObjectsCount: number;
}

/**
 * Controller for order transformation characteristics dynamic visualization
 */
class OrderTransformationCharacteristicsDynamicVisualizationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: OrderTransformationCharacteristicsDynamicVisualizationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: OrderTransformationCharacteristicsDynamicVisualizationData): void {
        const orderTransformationCharacteristicsDynamicVisualization = ($scope: OrderTransformationCharacteristicsDynamicVisualizationScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationCharacteristicsDynamicVisualizationCtrl", ["$scope", orderTransformationCharacteristicsDynamicVisualization]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of order transformation characteristics dynamic visualization handler
 */
export default function OrderTransformationCharacteristicsDynamicVisualizationController(data: OrderTransformationCharacteristicsDynamicVisualizationData): OrderTransformationCharacteristicsDynamicVisualizationHandler {
    return new OrderTransformationCharacteristicsDynamicVisualizationHandler(data);
}
