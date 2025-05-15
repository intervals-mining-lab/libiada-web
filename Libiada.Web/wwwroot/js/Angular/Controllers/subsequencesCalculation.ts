/// <reference types="angular" />

/**
* Interface for subsequence calculation controller data
*/
interface ISubsequencesCalculationData {
    // Basic data properties
    features?: IFeature[];
    attributeTypes?: string[];
    researchObjects?: IResearchObject[];
    characteristicTypes?: ICharacteristicType[];

    // Pre-selected values ​​(optional)
    selectedResearchObjects?: number[];
    selectedCharacteristics?: ICharacteristic[];
    featureId?: number;

    // Additional properties
    [key: string]: any;
}

interface IFeature {
    Text: string;
    Value: string | number;
    Selected?: boolean;
}

/**
* Interface for the subsequence calculation scope controller
*/
interface ISubsequencesCalculationScope extends ng.IScope {
    // Filtering parameters
    filters: any[];
    hideNotation: boolean;

    // Data for working with sequences (may be absent in the simplified controller)
    features?: IFeature[];
    attributeTypes?: string[];
    researchObjects?: IResearchObject[];
    characteristicTypes?: ICharacteristicType[];

    // Selected values
    featureId?: number;
    selectedResearchObjects?: number[];
    selectedCharacteristics?: ICharacteristic[];
    selectedAttributes?: string[];

    // Methods
    applyFilter: (filter: any) => void;
    toggleResearchObjectSelection?: (researchObject: IResearchObject) => void;
    addCharacteristic?: () => void;
    deleteCharacteristic?: (index: number) => void;
}

/**
* Controller for calculating subsequences
*/
class SubsequencesCalculationHandler {
    private data: ISubsequencesCalculationData;

    /**
    * Creates an instance of the subsequence calculation controller
    * @param data Data for initializing the controller
    */
    constructor(data: ISubsequencesCalculationData) {
        this.data = data;
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const subsequencesCalculation = ($scope: ISubsequencesCalculationScope): void => {
            // Initialize scope with data from parameter
            MapModelFromJson($scope, this.data);

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
function SubsequencesCalculationController(data: ISubsequencesCalculationData): SubsequencesCalculationHandler {
    return new SubsequencesCalculationHandler(data);
}