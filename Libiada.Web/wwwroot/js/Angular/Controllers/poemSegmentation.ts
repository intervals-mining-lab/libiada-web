import { MapModelFromJson } from "functions";
import type {
    Notation,
    Language,
    Translator,
    Group,
    SequenceType,
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface PoemSegmentationData {
    groups: Group[];
    languages: Language[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[];
    sequenceTypes: SequenceType[];
    translators: Translator[];
}

/**
 * Interface for the controller's scope
 */
interface PoemSegmentationScope extends ng.IScope, PoemSegmentationData {
    selectedResearchObjectsCount: number;
}

// Controller class
class PoemSegmentationOperator {
    constructor(data: PoemSegmentationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: PoemSegmentationData): void {
        const poemSegmentation = ($scope: PoemSegmentationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("PoemSegmentationCtrl", ["$scope", "filterFilter", poemSegmentation]);
    }
}

// Wrapper function for backwards compatibility
export default function PoemSegmentationController(data: PoemSegmentationData): PoemSegmentationOperator {
    return new PoemSegmentationOperator(data);
}
