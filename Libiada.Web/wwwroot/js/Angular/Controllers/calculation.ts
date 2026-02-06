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
    Link,
    ArrangementType
} from "viewDataTypes";

// Interface for the data object that is passed to the controller
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
    //ClusterizatorsTypes: ClusterizatorType[];
}

// Interface for the $scope controller
interface CalculationScope extends ng.IScope, CalculationData {
    calculationFor: displayedTable;
    nature: number;
    hideNotation?: boolean;
    //notation: Notation;
    //language: Language;
    //translator: Translator;
    //pauseTreatment: PauseTreatment;
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;
    complementary: boolean;
    rotate: boolean;

    //ClusterizationType: ClusterizatorType;

    // Methods 
    //filterByNature: () => void;
    clearSelection: () => void;
    setUnselectAllResearchObjectsFunction: (func: Function) => void;
    setUnselectAllSequenceGroupsFunction: (func: Function) => void;
    unselectAllResearchObjects: Function;
    unselectAllSequenceGroups: Function;
}

type displayedTable = "researchObjects" | "sequenceGroups";

// Auxiliary interfaces

interface ICharacteristic {
    characteristicType: CharacteristicType;
    notation: Notation;
    link?: Link;
    arrangementType?: ArrangementType;
    language?: string;
    translator?: string;
    pauseTreatment?: PauseTreatment;
    trajectory?: Trajectory;
}

// Updated controller class
class CalculationOperator {
    constructor(data: CalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CalculationData): void {
        const calculation = ($scope: CalculationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            //function filterByNature(): void {
            //    if (!$scope.hideNotation) {
            //        const notation: Notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

            //        // if notation is not linked to characteristic 
            //        angular.forEach($scope.characteristics, (characteristic: ICharacteristic) => {
            //            characteristic.notation = notation;
            //        });
            //    }
            //}

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

            //$scope.filterByNature = filterByNature;
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;

            // if notation is not linked to characteristic 
            //$scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            //$scope.language = $scope.languages?.[0];
            //$scope.translator = $scope.translators?.[0];
            //$scope.pauseTreatment = $scope.pauseTreatment ?? $scope.pauseTreatments?.[0];

            $scope.calculationFor = "researchObjects";
            $scope.complementary = false;
            $scope.rotate = false;

            // if we are in clusterization 
            //if ($scope.ClusterizatorsTypes) {
            //    $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
            //}
        };

        angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CalculationController(data: CalculationData): CalculationOperator {
    return new CalculationOperator(data);
}
