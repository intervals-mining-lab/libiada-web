/// <reference types="angular" />

/**
* Interface for the sequence prediction results scope controller
*/
interface ISequencePredictionResultScope extends ng.IScope {
    // Loading data
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;

    // Sequence prediction results
    originSequence?: string;
    predictedSequence?: string;
    predictionSequence?: string;
    predictionAccuracy?: number;

    // Graph settings
    legend?: { id: number, name: string, visible: boolean }[];
    legendHeight?: number;
    height?: number;

    // Additional properties
    [key: string]: any;
}

/**
* Controller for sequence prediction results
*/
class SequencePredictionResultHandler {
    /**
    * Creates an instance of the sequence prediction results controller
    */
    constructor() {
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const sequencePredictionResult = ($scope: ISequencePredictionResultScope, $http: ng.IHttpService): void => {
            // Initialize the loading screen header
            $scope.loadingScreenHeader = "Loading data";

            // Get the task ID from the URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Set the loading status
            $scope.loading = true;

            // Load task data
            $http.get < any > (`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    // Map data to the model
                    MapModelFromJson($scope, data.data);

                    // Set parameters for the legend and graph, if any
                    if ($scope.legend) {
                        $scope.legendHeight = $scope.legend.length *
                            20;
                        $scope.height = 800 + $scope.legendHeight;
                    }

                    // Reset loading status
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading characteristic data");
                    $scope.loading = false;
                });
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencePredictionResultCtrl", ["$scope", "$http", sequencePredictionResult]);
    }
}

/**
* Wrapper function for backward compatibility
* @returns Sequence prediction result controller instance
*/
function SequencePredictionResultController(): SequencePredictionResultHandler {
    return new SequencePredictionResultHandler();
}