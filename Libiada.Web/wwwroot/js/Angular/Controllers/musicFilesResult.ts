/// <reference types="angular" />

/**
 * Interface for music files result data
 */
interface IMusicFilesResultData {
    // Add properties based on the actual data structure from the API
    // These will be populated from data.data in the HTTP response
    [key: string]: any;
}

/**
 * Interface for the controller scope
 */
interface IMusicFilesResultScope extends ng.IScope {
    // Task identifier for loading data
    taskId: string;

    // Properties mapped from the API response
    // Will be populated from data.data via MapModelFromJson
    [key: string]: any;
}

/**
 * Controller for displaying music files processing results
 */
class MusicFilesResultHandler {
    /**
     * Creates a new instance of the controller
     */
    constructor() {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        "use strict";

        const musicFilesResult = ($scope: IMusicFilesResultScope, $http: ng.IHttpService): void => {
            // Extract task ID from the URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Load data from the API
            $http.get < any > (`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);
                })
                .catch(function () {
                    alert("Failed loading characteristic data");
                });
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MusicFilesResultCtrl", ["$scope", "$http", musicFilesResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of MusicFilesResultHandler
 */
function MusicFilesResultController(): MusicFilesResultHandler {
    return new MusicFilesResultHandler();
}
