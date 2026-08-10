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
    ClusterizatorType
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface ClusterizationData {
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
    clusterizatorsTypes: ClusterizatorType[];
}

/**
 * Interface for the angular controller's scope
 */
interface ClusterizationScope extends ng.IScope, ClusterizationData {
    nature: string;
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;
    clusterizationType: ClusterizatorType;
}

// Controller class
class ClusterizationOperator {
    constructor(data: ClusterizationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: ClusterizationData): void {
        const clusterization = ($scope: ClusterizationScope): void => {
            MapModelFromJson($scope, data);

            $scope.clusterizationType = $scope.clusterizatorsTypes[0];
        };

        angular.module("libiada").controller("ClusterizationCtrl", ["$scope", clusterization]);
    }
}

// Wrapper function for backwards compatibility
export default function ClusterizationController(data: ClusterizationData): ClusterizationOperator {
    return new ClusterizationOperator(data);
}
