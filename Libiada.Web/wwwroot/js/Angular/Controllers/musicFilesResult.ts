/// <reference types="angular" />

/**
 * Interface for music files result data
 */
interface IMusicFilesResultData {

}

/**
 * Interface for the controller scope
 */
interface IMusicFilesResultScope extends ng.IScope {
    // Task identifier for loading data
    taskId: string;


}

/**
 * Controller for displaying music files processing results
 */
class MusicFilesResultHandler {
    /**
     * Creates a new instance of the controller
     */
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
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
