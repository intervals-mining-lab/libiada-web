import { MapModelFromJson } from "functions";

/**
 * Interface for nature object
 */
interface INature {
    Value: number;
    Text: string;
}

/**
 * Interface for multisequence create data
 */
interface IMultisequenceCreateData {
    // Natures list for selection
    natures: INature[];

    // Settings for multisequence display
    displayMultisequenceNumber: boolean;

    // Other properties that might be passed
    minimumSelectedResearchObjects?: number;
    maximumSelectedResearchObjects?: number;
    groups?: any[];
    sequenceTypes?: any[];

}

/**
 * Interface for controller scope
 */
interface IMultisequenceCreateScope extends ng.IScope {
    // Properties
    natures: INature[];
    nature: number;
    name: string;
    displayMultisequenceNumber: boolean;

    // Optional properties that might be used
    selectedResearchObjectsCount?: number;
    groups?: any[];
    sequenceTypes?: any[];

    // Methods
    filterByNature: () => void;
}

/**
 * Controller for multisequence creation
 */
class MultisequenceCreateHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IMultisequenceCreateData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: IMultisequenceCreateData): void {
        const multisequenceCreate = ($scope: IMultisequenceCreateScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            function filterByNature(): void {
                // Implementation left empty as in original
            }

            $scope.filterByNature = filterByNature;

            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceCreateCtrl", ["$scope", "filterFilter", multisequenceCreate]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of multisequence create handler
 */
export default function MultisequenceCreateController(data: IMultisequenceCreateData): MultisequenceCreateHandler {
    return new MultisequenceCreateHandler(data);
}
