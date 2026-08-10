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
interface MultisequenceCreateData {
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
interface MultisequenceCreateScope extends ng.IScope, MultisequenceCreateData {    
    nature: string;
    name: string;
    displayMultisequenceNumber: boolean;

    // Optional properties that might be used
    selectedResearchObjectsCount: number;
}

/**
 * Angular controller class for multisequence creation page
 */
class MultisequenceCreateHandler {
    constructor(data: MultisequenceCreateData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: MultisequenceCreateData): void {
        const multisequenceCreate = ($scope: MultisequenceCreateScope): void => {
            MapModelFromJson($scope, data);

            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceCreateCtrl", ["$scope", multisequenceCreate]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of MultisequenceCreateHandler
 */
export default function MultisequenceCreateController(data: MultisequenceCreateData): MultisequenceCreateHandler {
    return new MultisequenceCreateHandler(data);
}
