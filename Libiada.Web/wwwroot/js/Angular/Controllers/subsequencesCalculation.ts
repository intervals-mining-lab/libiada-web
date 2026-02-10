import type { Feature, ResearchObject } from "viewDataTypes";
import { MapModelFromJson, ICharacteristicType, ICharacteristic } from "functions";

/**
* Interface for subsequence calculation controller data
*/
interface SubsequencesCalculationData {
    // Basic data properties
    features?: Feature[];
    attributeTypes?: string[];
    researchObjects?: ResearchObject[];
    characteristicTypes?: ICharacteristicType[];

    // Pre-selected values ​​(optional)
    selectedResearchObjects?: number[];
    selectedCharacteristics?: ICharacteristic[];
    featureId?: number;
}

/**
* Interface for the subsequence calculation scope controller
*/
interface SubsequencesCalculationScope extends ng.IScope {
    // Filtering parameters
    filters: any[];
    hideNotation: boolean;

    // Data for working with sequences (may be absent in the simplified controller)
    features?: Feature[];
    attributeTypes?: string[];
    researchObjects?: ResearchObject[];
    characteristicTypes?: ICharacteristicType[];

    // Selected values
    featureId?: number;
    selectedResearchObjects?: number[];
    selectedCharacteristics?: ICharacteristic[];
    selectedAttributes?: string[];

    // Methods
    applyFilter: (filter: any) => void;
    toggleResearchObjectSelection?: (researchObject: ResearchObject) => void;
    addCharacteristic?: () => void;
    deleteCharacteristic?: (index: number) => void;
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
            // Initialize scope with data from parameter
            MapModelFromJson($scope, data);

            /**
            * Apply filter to data
            * @param filter Filter to apply
            */
            function applyFilter(filter: any): void {
                // Implementation of filter application method
                // Empty function, as in original JavaScript code
            }

            // Assign methods to $scope
            $scope.applyFilter = applyFilter;

            // Initialize default properties
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
