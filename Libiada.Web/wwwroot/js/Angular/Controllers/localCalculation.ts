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
    CharacteristicType
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface LocalCalculationData {
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
}

/**
 * Interface for the controller's scope
 */
interface LocalCalculationScope extends ng.IScope, LocalCalculationData {
    nature: string;
    selectedResearchObjectsCount: number;
}

// Controller class
class LocalCalculationOperator {
    constructor(data: LocalCalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: LocalCalculationData): void {
        const localCalculation = ($scope: LocalCalculationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("LocalCalculationCtrl", ["$scope", "filterFilter", localCalculation]);
    }
}

// Wrapper function for backwards compatibility
export default function LocalCalculationController(data: LocalCalculationData): LocalCalculationOperator {
    return new LocalCalculationOperator(data);
}
