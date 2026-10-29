import type { Feature, CharacteristicType, Group, Notation, SequenceType } from "viewDataTypes";
import { MapModelFromJson } from "functions";

/**
 * Interface for the data object passed from the server
 */
interface SubsequencesCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    features: Feature[];
    groups: Group[];
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    notations: Notation[];
    sequenceTypes: SequenceType[];
}

/**
 * Interface for the controller's scope
 */
interface SubsequencesCalculationScope extends ng.IScope, SubsequencesCalculationData {
    selectedResearchObjectsCount: number;
    filters: string[];
    hideNotation: boolean;
}

/**
* Controller for calculating subsequences
*/
class SubsequencesCalculationHandler {

    /**
    * Creates an instance of the subsequence calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data: SubsequencesCalculationData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller
    */
    private ngOnInit(data: SubsequencesCalculationData): void {
        const subsequencesCalculation = ($scope: SubsequencesCalculationScope): void => {
            // Initialize scope with data from server
            MapModelFromJson($scope, data);

            // Initialize default properties values
            $scope.filters = [];
            $scope.hideNotation = true;
        };

        // Register controller in Angular
        angular.module("libiada").controller("SubsequencesCalculationCtrl", ["$scope", subsequencesCalculation]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Subsequence calculation controller instance
*/
export default function SubsequencesCalculationController(data: SubsequencesCalculationData): SubsequencesCalculationHandler {
    return new SubsequencesCalculationHandler(data);
}
