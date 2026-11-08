import { MapModelFromJson } from "functions";
import { CharacteristicsDictionary, CharacteristicType, Feature, Group, Notation, SequenceType } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SequencesAlignmentData {
    characteristicTypes:CharacteristicType[];
    characteristicsDictionary: CharacteristicsDictionary;
    features: Feature[];
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[];
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the angular controller's scope
 */
interface SequencesAlignmentScope extends ng.IScope, SequencesAlignmentData {
}

/**
* Sequence alignment angular controller
*/
class SequencesAlignmentHandler {
    constructor(data: SequencesAlignmentData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: SequencesAlignmentData): void {
        const sequencesAlignment = ($scope: SequencesAlignmentScope): void => {
            // Initialize the scope with data from the parameter
            MapModelFromJson($scope, data);
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for initializing the controller
* @returns An instance of the sequence alignment controller
*/
export default function SequencesAlignmentController(data: SequencesAlignmentData): SequencesAlignmentHandler {
    return new SequencesAlignmentHandler(data);
}
