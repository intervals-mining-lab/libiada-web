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
    DisplayedTable,
    CharacteristicsDictionary
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface CongenericCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: CharacteristicsDictionary;
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
 * Interface for the angular controller's scope
 */
interface CongenericCalculationScope extends ng.IScope, CongenericCalculationData {
    calculationFor: DisplayedTable;
    nature: string;
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;

    clearSelection: () => void;
    setUnselectAllResearchObjectsFunction: (func: () => Promise<void>) => void;
    setUnselectAllSequenceGroupsFunction: (func: () => Promise<void>) => void;
    unselectAllResearchObjects: Function;
    unselectAllSequenceGroups: Function;
}

// Controller class
class CongenericCalculationOperator {
    constructor(data: CongenericCalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CongenericCalculationData): void {
        const congenericCalculation = ($scope: CongenericCalculationScope): void => {
            MapModelFromJson($scope, data);

            function setUnselectAllResearchObjectsFunction(func: () => Promise<void>): void {
                $scope.unselectAllResearchObjects = func;
            }

            function setUnselectAllSequenceGroupsFunction(func: () => Promise<void>): void {
                $scope.unselectAllSequenceGroups = func;
            }

            function clearSelection(): void {
                if ($scope.unselectAllResearchObjects) $scope.unselectAllResearchObjects();

                if ($scope.unselectAllSequenceGroups) $scope.unselectAllSequenceGroups();
            }

            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;

            $scope.calculationFor = "researchObjects";
        };
        angular.module("libiada").controller("CongenericCalculationCtrl", ["$scope", congenericCalculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CongenericCalculationController(data: CongenericCalculationData): CongenericCalculationOperator {
    return new CongenericCalculationOperator(data);
}
