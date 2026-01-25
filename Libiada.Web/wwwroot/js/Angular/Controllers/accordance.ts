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
    CharacterisrticType
} from "viewDataTypes";

/**
 * Interface for accordion data
 */
interface AccordanceData {
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    characteristicTypes: CharacterisrticType[];
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
 * Interface for controller scope
 */
interface AccordanceScope extends angular.IScope, AccordanceData {
    nature: string;
    selectedResearchObjectsCount: number;
}

/**
 * Controller for accordance functionality
 */
class AccordanceHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: AccordanceData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: AccordanceData): void {
        const accordance = ($scope: AccordanceScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance handler
 */
export default function AccordanceController(data: AccordanceData): AccordanceHandler {
    return new AccordanceHandler(data);
}
