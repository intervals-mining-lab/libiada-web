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

// Interface for the data object that is passed to the controller
interface CongenericCalculationData {
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

// Interface for the $scope controller
interface CongenericCalculationScope extends ng.IScope, CongenericCalculationData {
    nature?: number;
    selectedResearchObjectsCount?: number;
    selectedSequenceGroupsCount?: number;

}

// Controller class
class CongenericCalculationOperator {
    constructor(data: CongenericCalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CongenericCalculationData): void {
        const congenericCalculation = ($scope: CongenericCalculationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("CongenericCalculationCtrl", ["$scope", "filterFilter", congenericCalculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CongenericCalculationController(data: CongenericCalculationData): CongenericCalculationOperator {
    return new CongenericCalculationOperator(data);
}
