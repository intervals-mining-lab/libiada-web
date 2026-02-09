import { MapModelFromJson } from "functions";
import type {
    Nature,
    Notation,
    Language,
    Translator,
    PauseTreatment,
    Trajectory,
    SequenceType,
    Group,
    CharacteristicType
} from "viewDataTypes";

// Interface for the data object that is passed to the controller
interface RelationCalculationData {
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
}

// Interface for the controller's scope
interface RelationCalculationScope extends angular.IScope, RelationCalculationData {
    nature: string;
    selectedResearchObjectsCount: number;
    showFilters: boolean;
    frequencyFilter: boolean;
}

class RelationCalculationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: RelationCalculationData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: RelationCalculationData): void {
        const relationCalculation = ($scope: RelationCalculationScope): void => {
            MapModelFromJson($scope, data);

            $scope.showFilters = false;
            $scope.frequencyFilter = false;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("RelationCalculationCtrl", ["$scope", relationCalculation]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of relation calculation handler
 */
export default function RelationCalculationController(data: RelationCalculationData): RelationCalculationHandler {
    return new RelationCalculationHandler(data);
}
