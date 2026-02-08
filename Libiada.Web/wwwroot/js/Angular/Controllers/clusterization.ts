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

// Interface for the data object that is passed to the controller
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

// Interface for the $scope controller
interface ClusterizationScope extends ng.IScope, ClusterizationData {
    nature: number;
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
        const clusterization = ($scope: ClusterizationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            $scope.clusterizationType = $scope.clusterizatorsTypes[0];
        };

        angular.module("libiada").controller("ClusterizationCtrl", ["$scope", "filterFilter", clusterization]);
    }
}

// Wrapper function for backwards compatibility
export default function ClusterizationController(data: ClusterizationData): ClusterizationOperator {
    return new ClusterizationOperator(data);
}
