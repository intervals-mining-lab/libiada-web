/// <reference types="angular" />

// Interface for the data object that is passed to the controller
interface ICalculationData {
    // Basic settings
    natures: INature[];
    nature?: number;

    // Notations
    notations: INotation[];
    hideNotation?: boolean;

    // Characteristics
    characteristicTypes: ICharacteristicType[];
    characteristicsDictionary: { [key: string]: string };
    characteristics?: ICharacteristic[];

    // Groups and sequence types
    groups: IGroup[];
    sequenceTypes: ISequenceType[];

    // Additional settings
    languages?: string[];
    translators?: string[];
    pauseTreatments?: IPauseTreatment[];
    trajectories?: ITrajectory[];

    // Limits for selecting research objects
    minimumSelectedResearchObjects: number;
    maximumSelectedResearchObjects?: number;

    // Options for clustering (if any)
    ClusterizatorsTypes?: IClusterizatorType[];

    // Other possible properties
    [key: string]: any;
}

// Interface for the $scope controller
interface ICalculationScope extends ng.IScope {
    // Properties related to nature and notation processing
    nature: number;
    natures: INature[];
    notation: INotation;
    notations: INotation[];
    hideNotation: boolean;

    // Characteristics
    characteristics: ICharacteristic[];
    characteristicTypes: ICharacteristicType[];
    characteristicsDictionary: { [key: string]: string };

    // Selecting research objects
    calculaionFor: string; // "researchObjects" or "sequenceGroups"
    selectedResearchObjectsCount?: number;
    selectedSequenceGroupsCount?: number;

    // Sequence groups and types
    groups: IGroup[];
    sequenceTypes: ISequenceType[];

    // Additional settings
    language: string;
    languages: string[];
    translator: string;
    translators: string[];
    pauseTreatment: IPauseTreatment;
    pauseTreatments: IPauseTreatment[];
    trajectories?: ITrajectory[];

    // Properties for controlling rotation and complementarity of sequences
    complementary?: boolean;
    rotate?: boolean;
    rotationLength?: number;

    // Clustering (if any)
    ClusterizatorsTypes?: IClusterizatorType[];
    ClusterizationType?: IClusterizatorType;

    // Methods 
    filterByNature: () => void;
    clearSelection: () => void;
    setUnselectAllResearchObjectsFunction: (func: Function) => void;
    setUnselectAllSequenceGroupsFunction: (func: Function) => void;
    unselectAllResearchObjects?: Function;
    unselectAllSequenceGroups?: Function;
}

// Auxiliary interfaces

interface INature {
    id: number;
    name: string;
}

interface INotation {
    id: number;
    name: string;
    Nature: number;
}

interface ICharacteristicType {
    id: number;
    name: string;
    description?: string;
    Links: ILink[];
    ArrangementTypes: IArrangementType[];
}

interface ICharacteristic {
    characteristicType: ICharacteristicType;
    notation: INotation;
    link?: ILink;
    arrangementType?: IArrangementType;
    language?: string;
    translator?: string;
    pauseTreatment?: IPauseTreatment;
    trajectory?: ITrajectory;
}

interface ILink {
    id: number;
    name: string;
}

interface IArrangementType {
    id: number;
    name: string;
}

interface IGroup {
    id: number;
    name: string;
}

interface ISequenceType {
    id: number;
    name: string;
}

interface IPauseTreatment {
    id: number;
    name: string;
}

interface ITrajectory {
    id: number;
    name: string;
}

interface IClusterizatorType {
    id: number;
    name: string;
}

// Updated controller class
class CalculationOperator {
    private data: ICalculationData;

    constructor(data: ICalculationData) {
        this.data = data;
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const calculation = ($scope: ICalculationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, this.data);

            function filterByNature(): void {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                    // if notation is not linked to characteristic 
                    angular.forEach($scope.characteristics, (characteristic: ICharacteristic) => {
                        characteristic.notation = $scope.notation;
                    });
                }
            }

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

            $scope.filterByNature = filterByNature;
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;

            // if notation is not linked to characteristic 
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            $scope.language = $scope.languages?.[0];
            $scope.translator = $scope.translators?.[0];
            $scope.pauseTreatment = $scope.pauseTreatment ?? $scope.pauseTreatments?.[0];
            $scope.calculaionFor = "researchObjects";

            // if we are in clusterization 
            if ($scope.ClusterizatorsTypes) {
                $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
            }
        };

        angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
    }
}

// Wrapper function for backwards compatibility
function CalculationController(data: ICalculationData): CalculationOperator {
    return new CalculationOperator(data);
}
