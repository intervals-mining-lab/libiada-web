/// <reference types="angular" />
/// <reference types="functions" />
/**
 * Controller for order transformation result visualization
 */
class OrderTransformationResultHandler {
    constructor() {
        this.initializeController();
    }
    /**
     * Initializes the Angular controller
     */
    initializeController() {
        "use strict";
        const orderTransformationResult = ($scope, $http) => {
            // Set loading message
            $scope.loadingScreenHeader = "Loading order transformation results";
            // Extract task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Set initial loading state
            $scope.loading = true;
            // Fetch data from server
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading import results");
                $scope.loading = false;
            });
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationResultCtrl", ["$scope", "$http", orderTransformationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
function OrderTransformationResultController() {
    return new OrderTransformationResultHandler();
}
//# sourceMappingURL=orderTransformationResult.js.map