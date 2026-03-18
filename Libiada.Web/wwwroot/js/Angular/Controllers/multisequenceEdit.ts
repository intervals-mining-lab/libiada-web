import { MapModelFromJson } from "functions";
import type {
    Group,
    Nature,
    SequenceType,
    Language,
    Notation,
    PauseTreatment,
    Trajectory,
    Translator
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface MultisequenceEditData {
    groups: Group[];
    languages: Language[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    natures: Nature[];
    notations: Notation[];
    pauseTreatments: PauseTreatment[]
    sequenceTypes: SequenceType[];
    trajectories: Trajectory[];
    translators: Translator[];
}

/**
 * Interface for the angular controller's scope
 */
interface MultisequenceEditScope extends ng.IScope, MultisequenceEditData {    
    nature: string;
    name: string;
    displayMultisequenceNumber: boolean;

    // Optional properties that might be used
    selectedResearchObjectsCount: number;
}

/**
 * Angular controller class for multisequence creation page
 */
class MultisequenceEditHandler {
    constructor(data: MultisequenceEditData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: MultisequenceEditData): void {
        const multisequenceEdit = ($scope: MultisequenceEditScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceEditCtrl", ["$scope", "filterFilter", multisequenceEdit]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of MultisequenceEditHandler
 */
export default function MultisequenceEditController(data: MultisequenceEditData): MultisequenceEditHandler {
    return new MultisequenceEditHandler(data);
}
