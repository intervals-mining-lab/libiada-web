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
    SequenceGroup,
    CharacteristicType,
    DisplayedTable
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface CalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    groups: Group[];
    languages: Language[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    natures: Nature[];
    notations: Notation[];
    pauseTreatments: PauseTreatment[];
    sequenceGroups: SequenceGroup[];
    sequenceTypes: SequenceType[];
    trajectories: Trajectory[];
    translators: Translator[];
}

/**
 * Interface for the angular controller's scope
 */
interface CalculationScope extends ng.IScope, CalculationData {
    calculationFor: DisplayedTable;
    nature: string;
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;
    complementary: boolean;
    rotate: boolean;

    // Methods 
    clearSelection: () => void;
    setUnselectAllResearchObjectsFunction: (func: Function) => void;
    setUnselectAllSequenceGroupsFunction: (func: Function) => void;
    unselectAllResearchObjects: Function;
    unselectAllSequenceGroups: Function;
}



// Angular controller class
class CalculationOperator {
    constructor(data: CalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CalculationData): void {
        const calculation = ($scope: CalculationScope): void => {
            MapModelFromJson($scope, data);

            function setUnselectAllResearchObjectsFunction(func: Function): void {
                $scope.unselectAllResearchObjects = func;
            }

            function setUnselectAllSequenceGroupsFunction(func: Function): void {
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
            $scope.complementary = false;
            $scope.rotate = false;
        };

        angular.module("libiada").controller("CalculationCtrl", ["$scope", calculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CalculationController(data: CalculationData): CalculationOperator {
    return new CalculationOperator(data);
}
