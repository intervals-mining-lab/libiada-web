import { initScopeFromServer } from "functions";

/**
 * Interface for sequence data in custom sequence order transformer
 */
interface SequenceData {
    name: string;
    value: string;
}

/**
 * Interface for the data object fetched from the server
 */
interface CustomSequenceOrderTransformerResultData {
    transformationsList: string[];
    iterationsCount: number;
    sequences: SequenceData[];
}

/**
 * Interface for the angular controller's scope
 */
interface CustomSequenceOrderTransformerResultScope extends ng.IScope, CustomSequenceOrderTransformerResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

/**
 * Angular controller class for custom sequence order transformer result visualization
 */
class CustomSequenceOrderTransformerResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const customSequenceOrderTransformerResult = ($scope: CustomSequenceOrderTransformerResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<CustomSequenceOrderTransformerResultData>(
                $http,
                $scope,
                "Loading order transformation results",
                "Failed loading order transformation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceOrderTransformerCtrl", ["$scope", "$http", customSequenceOrderTransformerResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceOrderTransformerResultController(): CustomSequenceOrderTransformerResultHandler {
    return new CustomSequenceOrderTransformerResultHandler();
}
