/// <reference types="angular" />

interface SequenceType {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

interface SequenceGroup {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: string;
}

interface Notation {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

interface Group {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

interface Feature {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

interface Link {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

interface ArrangementType {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

interface CharacterisrticType {
    ArrangementTypes: ArrangementType[];
    Links: Link[];
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

// Interface for data passed to the controller from server on page load
interface ISubsequencesDistributionData {

    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    characteristicTypes: CharacterisrticType[];
    characteristicsDictionary: { [key: string]: number };
    features: Feature[];
    groups: Group[];
    nature: string;
    notations: Notation[];
    sequenceGroups: SequenceGroup[];
    sequenceTypes: SequenceType[];
}

// Interface for $scope in controller
interface ISubsequencesDistributionScope extends ng.IScope, ISubsequencesDistributionData {
    selectedResearchObjectsCount: number;
}

// Main controller class
class SubsequencesDistributionManager {
    constructor(data: ISubsequencesDistributionData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: ISubsequencesDistributionData): void {
        // Define the controller function
        const subsequencesDistribution = ($scope: ISubsequencesDistributionScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register the controller in the Angular module
        angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}

// Export the constructor for use in _AngularControllerInitializer.cshtml
function SubsequencesDistributionController(data: ISubsequencesDistributionData): SubsequencesDistributionManager {
    return new SubsequencesDistributionManager(data);
};
