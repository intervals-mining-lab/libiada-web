import { initScopeFromServer } from "functions";

/**
 * Interface for sequence data in custom sequence segmentation
 */
interface SequenceData {
    name: string;
    value: string;
}

/**
 * Interface for the data object fetched from the server
 */
interface CustomSequenceSegmentationResultData {
    sequences: SequenceData[];
}



/**
 * Interface for the controller's scope
 */
interface CustomSequenceSegmentationResultScope extends ng.IScope, CustomSequenceSegmentationResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

/**
 * Angular controller class for custom sequence segmentation result visualization
 */
class CustomSequenceSegmentationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const customSequenceSegmentationResult = ($scope: CustomSequenceSegmentationResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<CustomSequenceSegmentationResultData>(
                $http,
                $scope,
                "Loading custom sequence segmentation results",
                "Failed loading custom sequence segmentation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SegmentationResultCtrl", ["$scope", "$http", customSequenceSegmentationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceSegmentationResultController(): CustomSequenceSegmentationResultHandler {
    return new CustomSequenceSegmentationResultHandler();
}
