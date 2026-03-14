import { MapModelFromJson } from "functions";
import type {
    Notation,
    Group,
    SequenceType
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface GeneticSequencesTransformationData {
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[]
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the controller's scope
 */
interface GeneticSequencesTransformationScope extends ng.IScope, GeneticSequencesTransformationData {
    selectedResearchObjectsCount: number;
}

// Controller class
class GeneticSequencesTransformationOperator {
    constructor(data: GeneticSequencesTransformationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: GeneticSequencesTransformationData): void {
        const geneticSequencesTransformation = ($scope: GeneticSequencesTransformationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("GeneticSequencesTransformationCtrl", ["$scope", "filterFilter", geneticSequencesTransformation]);
    }
}

// Wrapper function for backwards compatibility
export default function GeneticSequencesTransformationController(data: GeneticSequencesTransformationData): GeneticSequencesTransformationOperator {
    return new GeneticSequencesTransformationOperator(data);
}
