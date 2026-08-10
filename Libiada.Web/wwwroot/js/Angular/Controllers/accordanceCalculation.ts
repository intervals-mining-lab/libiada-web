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
    Feature,
    CharacteristicType
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface AccordanceCalculationData {
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    features: Feature[];
    groups: Group[];
    natures: Nature[];
    notations: Notation[];
    languages: Language[];
    pauseTreatments: PauseTreatment[];
    trajectories: Trajectory[];
    translators: Translator[];
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the angular controller's scope
 */
interface AccordanceCalculationScope extends angular.IScope, AccordanceCalculationData {
    nature: string;
    selectedResearchObjectsCount: number;
}

class AccordanceCalculationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: AccordanceCalculationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: AccordanceCalculationData): void {
        const accordanceCalculation = ($scope: AccordanceCalculationScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("AccordanceCalculationCtrl", ["$scope", accordanceCalculation]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance calculation handler
 */
export default function AccordanceCalculationController(data: AccordanceCalculationData): AccordanceCalculationHandler {
    return new AccordanceCalculationHandler(data);
}
