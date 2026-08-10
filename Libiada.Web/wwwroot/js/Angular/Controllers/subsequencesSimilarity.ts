import { MapModelFromJson } from "functions";
import type {
    Notation,
    Group,
    SequenceType,
    CharacteristicType,
    Feature
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SubsequencesSimilarityData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[];
    sequenceTypes: SequenceType[];
    features: Feature[];
}

/**
 * Interface for the angular controller's scope
 */
interface SubsequencesSimilarityScope extends ng.IScope, SubsequencesSimilarityData {
    selectedResearchObjectsCount: number;
}

// Controller class
class SubsequencesSimilarityOperator {
    constructor(data: SubsequencesSimilarityData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: SubsequencesSimilarityData): void {
        const subsequencesSimilarity = ($scope: SubsequencesSimilarityScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("SubsequencesSimilarityCtrl", ["$scope", subsequencesSimilarity]);
    }
}

// Wrapper function for backwards compatibility
export default function SubsequencesSimilarityController(data: SubsequencesSimilarityData): SubsequencesSimilarityOperator {
    return new SubsequencesSimilarityOperator(data);
}
