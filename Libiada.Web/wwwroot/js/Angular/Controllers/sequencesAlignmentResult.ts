import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface SequencesAlignmentResultData {
    characteristicName: string;
    cyclicShift: boolean;
    distances: { Value: number }[];
    features: string[];
    firstSequenceName: string;
    secondSequenceName: string;
    optimalRotation: number;
    sort: boolean;
    validationType: string;
}

/**
 * Interface for the angular controller's scope
 */
interface SequencesAlignmentResultScope extends ng.IScope, SequencesAlignmentResultData, ResultsLoadingScreenData {
}

/**
 * Controller for sequences alignment result view
 */
class SequencesAlignmentResultHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes Angular controller
     */
    private ngOnInit(): void {
        const sequencesAlignmentResult = async ($scope: SequencesAlignmentResultScope, $http: ng.IHttpService): Promise<void> => {
            await initScopeFromServer<SequencesAlignmentResultData>(
                $http,
                $scope,
                "Loading alignment results",
                "Failed loading alignment results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SequencesAlignmentResultCtrl", ["$scope", "$http", sequencesAlignmentResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequences alignment result handler
 */
export default function SequencesAlignmentResultController(): SequencesAlignmentResultHandler {
    return new SequencesAlignmentResultHandler();
}
