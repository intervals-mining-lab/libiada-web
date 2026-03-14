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
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SequencePredictionData {
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
interface SequencePredictionScope extends ng.IScope, SequencePredictionData {
    nature: string;
    selectedResearchObjectsCount: number;
}

// Controller class
class SequencePredictionOperator {
    constructor(data: SequencePredictionData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: SequencePredictionData): void {
        const sequencePrediction = ($scope: SequencePredictionScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("SequencePredictionCtrl", ["$scope", "filterFilter", sequencePrediction]);
    }
}

// Wrapper function for backwards compatibility
export default function SequencePredictionController(data: SequencePredictionData): SequencePredictionOperator {
    return new SequencePredictionOperator(data);
}
