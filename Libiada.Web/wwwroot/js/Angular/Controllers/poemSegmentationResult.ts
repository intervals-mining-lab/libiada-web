import { initScopeFromServer } from "functions";

/**
 * Interface for the data object fetched from the server
 */
interface PoemSegmentationResultData  {
    segmentedString: { [key: string]: number };
    segmentedText: string;
    poemSequence: string;
    poemName: string;
}

/**
 * Interface for the controller's scope
 */
interface PoemSegmentationResultScope extends ng.IScope, PoemSegmentationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

/**
 * Angular controller class for poem segmentation result view
 */
class PoemSegmentationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const poemSegmentationResult = ($scope: PoemSegmentationResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<PoemSegmentationResultData>($http, $scope, "Loading segmentation results", "Failed loading segmentation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("PoemSegmentationResultCtrl", ["$scope", "$http", poemSegmentationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function PoemSegmentationResultController(): PoemSegmentationResultHandler {
    return new PoemSegmentationResultHandler();
}
