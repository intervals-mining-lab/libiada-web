/// <reference types="angular" />
/// <reference types="functions" />

/**
 * Interface for the order transformation result scope
 */
interface IOrderTransformationResultScope extends ng.IScope {
    // Loading state
    loading: boolean;
    loadingScreenHeader: string;
    
    // Task data
    taskId: string;
    
    // Result data (this can be expanded based on the actual data structure)
    [key: string]: any;
}

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
    private initializeController(): void {
        "use strict";

        const orderTransformationResult = ($scope: IOrderTransformationResultScope, $http: ng.IHttpService): void => {
            // Set loading message
            $scope.loadingScreenHeader = "Loading order transformation results";

            // Extract task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Set initial loading state
            $scope.loading = true;

            // Fetch data from server
            $http.get<any>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
        angular.module("libiada").controller("OrderTransformationResultCtrl", 
            ["$scope", "$http", orderTransformationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
function OrderTransformationResultController(): OrderTransformationResultHandler {
    return new OrderTransformationResultHandler();
}
