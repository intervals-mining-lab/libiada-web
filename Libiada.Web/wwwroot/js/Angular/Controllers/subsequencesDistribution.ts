import type { SequenceType, SequenceGroup, Notation, Group, Feature, CharacterisrticType } from "viewDataTypes";
import { MapModelFromJson } from "functions";

// Interface for data passed to the controller from server on page load
interface SubsequencesDistributionData {

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
interface SubsequencesDistributionScope extends ng.IScope, SubsequencesDistributionData {
    selectedResearchObjectsCount: number;
}

// Main controller class
class SubsequencesDistributionManager {
    constructor(data: SubsequencesDistributionData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: SubsequencesDistributionData): void {
        // Define the controller function
        const subsequencesDistribution = ($scope: SubsequencesDistributionScope): void => {
            MapModelFromJson($scope, data);
        };

        // Register the controller in the Angular module
        angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}

// Export the constructor for use in _AngularControllerInitializer.cshtml
export default function SubsequencesDistributionController(data: SubsequencesDistributionData): SubsequencesDistributionManager {
    return new SubsequencesDistributionManager(data);
};
