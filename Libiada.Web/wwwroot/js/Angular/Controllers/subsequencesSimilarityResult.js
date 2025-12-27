"use strict";
/// <reference types="angular" />
/**
 * Controller for subsequences similarity result visualization
 */
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const subsequencesSimilarityResult = ($scope, $http) => {
            // Get task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Initialize loading state
            $scope.loadingScreenHeader = "Loading subsequences data";
            $scope.loading = true;
            // Load data from server
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading subsequences data");
                $scope.loading = false;
            });
        };
        // Register controller with Angular
        angular.module("libiada").controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", subsequencesSimilarityResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
function SubsequencesSimilarityResultController() {
    return new SubsequencesSimilarityResultHandler();
}
//# sourceMappingURL=subsequencesSimilarityResult.js.map