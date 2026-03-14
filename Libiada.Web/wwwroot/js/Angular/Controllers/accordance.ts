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
interface AccordanceData {
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
interface AccordanceScope extends angular.IScope, AccordanceData {
    nature: string;
    selectedResearchObjectsCount: number;
}


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
