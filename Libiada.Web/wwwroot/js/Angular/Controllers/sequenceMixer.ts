import { MapModelFromJson } from "functions";
import type {
    SequenceType,
    Notation,
    Group,
    Language,
    Nature,
    PauseTreatment,
    Trajectory,
    Translator
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface SequenceMixerData {
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
}

/**
 * Interface for the angular controller's scope
 */
interface SequenceMixerScope extends ng.IScope, SequenceMixerData {
    // Selected nature (value)
    nature: number;
    // Selected notation
    notation: { Nature: number; Value: string; Text: string };
}

/**
* Controller class for sequence mixer
*/
class SequenceMixerHandler  {
    /**
    * Creates a new controller instance.
    * @param data Data to create the controller
    */
    constructor(data: SequenceMixerData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller.
    */
    private ngOnInit(data: SequenceMixerData): void {

        const sequenceMixer = ($scope: SequenceMixerScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("SequenceMixerCtrl", ["$scope", sequenceMixer]);
    }
}

/**
* wrapper function for backward compatibility
* @param data Data to create controller
* @returns SequenceMixerHandler instance
*/
export default function SequenceMixerController(data: SequenceMixerData): SequenceMixerHandler {
    return new SequenceMixerHandler(data);
}
