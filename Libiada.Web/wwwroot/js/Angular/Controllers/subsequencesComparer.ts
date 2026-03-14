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
interface SubsequencesComparerData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[];
    sequenceTypes: SequenceType[];
    percentageDifferenseNeeded: boolean;
    features: Feature[];
}

/**
 * Interface for the angular controller's scope
 */
interface SubsequencesComparerScope extends ng.IScope, SubsequencesComparerData {
    selectedResearchObjectsCount: number;
}

// Controller class
class SubsequencesComparerOperator {
    constructor(data: SubsequencesComparerData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: SubsequencesComparerData): void {
        const subsequencesComparer = ($scope: SubsequencesComparerScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("SubsequencesComparerCtrl", ["$scope", "filterFilter", subsequencesComparer]);
    }
}

// Wrapper function for backwards compatibility
export default function SubsequencesComparerController(data: SubsequencesComparerData): SubsequencesComparerOperator {
    return new SubsequencesComparerOperator(data);
}
