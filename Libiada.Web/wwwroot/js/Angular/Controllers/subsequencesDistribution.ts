// Declaring global variables and functions
/// <reference types="angular" />
/// <reference path="../functions.d.ts" />

// Interface for data passed to the controller
interface ISubsequencesDistributionData {
    // Main parameters
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    groups: Array<{ id: number; name: string }>;
    sequenceTypes: Array<{ id: number; name: string }>;
    features: Array<{ id: number; name: string }>;

    // Characteristics
    characteristicTypes: Array<{ value: string; text: string }>;
    characteristicsDictionary: { [key: string]: any };
    notations: Array<{ value: number; text: string }>;
    languages: Array<{ value: number; text: string }>;
    translators: Array<{ value: number; text: string }>;
    pauseTreatments: Array<{ value: number; text: string }>;
    trajectories?: Array<{ value: number; text: string }>;
}

// Interface for $scope in controller
interface ISubsequencesDistributionScope extends ng.IScope {
    // Basic parameters from the data model 
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    groups: Array<{ id: number; name: string }>;
    sequenceTypes: Array<{ id: number; name: string }>;
    features: Array<{ id: number; name: string }>;

    // Characteristics 
    characteristicTypes: Array<{ value: string; text: string }>;
    characteristicsDictionary: { [key: string]: any };
    notations: Array<{ value: number; text: string }>;
    languages: Array<{ value: number; text: string }>;
    translators: Array<{ value: number; text: string }>;
    pauseTreatments: Array<{ value: number; text: string }>;
    trajectories?: Array<{ value: number; text: string }>;

    // Dynamic data 
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;
}



// Main controller class
class SubsequencesDistributionManager {
    constructor(private readonly data: ISubsequencesDistributionData) {
        this.initialize();
    }

    private initialize(): void {
        // Define the controller function
        const subsequencesDistribution = ($scope: ISubsequencesDistributionScope): void => {
            MapModelFromJson($scope, this.data);
        };

        // Register the controller in the Angular module
        angular.module("libiada")
            .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}

// Export the constructor for use in _AngularControllerInitializer.cshtml
function SubsequencesDistributionController(data: ISubsequencesDistributionData): SubsequencesDistributionManager {

    return new SubsequencesDistributionManager(data);
};