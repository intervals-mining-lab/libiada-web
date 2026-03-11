import { initScopeFromServer } from "functions";

/**
 * Interface for the data object fetched from the server
 */
interface OrderTransformationConvergenceResultData {
    transformationsList: string[];
    transformationsResult: number[][];
    sequence: string;
    iterationsCount: number;
    loopIteration: number;
    lastIteration: number;
}

/**
 * Interface for the controller's scope
 */
interface OrderTransformationConvergenceResultScope extends ng.IScope, OrderTransformationConvergenceResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

/**
 *  Angular controller class for order transformation convergence result visualization
 */
class OrderTransformationConvergenceResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const orderTransformationConvergenceResult = ($scope: OrderTransformationConvergenceResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<OrderTransformationConvergenceResultData>($http, $scope, "Loading order transformation convergence results", "Failed loading order transformation convergence results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationConvergenceResultCtrl",
            ["$scope", "$http", orderTransformationConvergenceResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformationConvergenceResultController(): OrderTransformationConvergenceResultHandler {
    return new OrderTransformationConvergenceResultHandler();
}
