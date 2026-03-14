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
    Group
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface OrderTransformationConvergenceData {
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
interface OrderTransformationConvergenceScope extends angular.IScope, OrderTransformationConvergenceData {
    nature: string;
    selectedResearchObjectsCount: number;
}

// Angular controller class
class OrderTransformationConvergenceHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: OrderTransformationConvergenceData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: OrderTransformationConvergenceData): void {
        const orderTransformationConvergence = ($scope: OrderTransformationConvergenceScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationConvergenceCtrl", ["$scope", "filterFilter", orderTransformationConvergence]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of order transformation convergence handler
 */
export default function OrderTransformationConvergenceController(data: OrderTransformationConvergenceData): OrderTransformationConvergenceHandler {
    return new OrderTransformationConvergenceHandler(data);
}
