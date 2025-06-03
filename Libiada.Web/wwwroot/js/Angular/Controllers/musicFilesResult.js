/// <reference types="angular" />
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
    initializeController() {
        "use strict";
        const musicFilesResult = ($scope, $http) => {
            // Extract task ID from the URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Load data from the API
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
function MusicFilesResultController() {
    return new MusicFilesResultHandler();
}
//# sourceMappingURL=musicFilesResult.js.map