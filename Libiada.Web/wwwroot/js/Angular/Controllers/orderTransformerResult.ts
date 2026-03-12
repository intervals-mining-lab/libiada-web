import { initScopeFromServer } from "functions";


/**
 * Interface for the data object fetched from the server
 */
interface OrderTransformerResultData {
    transformationsList: string[];
    iterationsCount: number;
    sequence: string;
}

/**
 * Interface for the controller's scope
 */
interface OrderTransformerResultScope extends ng.IScope, OrderTransformerResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

/**
 * Interface for the order transformation result scope
 */
interface OrderTransformerResultScope extends ng.IScope {
    // Loading state
    loading: boolean;
    loadingScreenHeader: string;
    
    // Task data
    taskId: string;
}

/**
 * Controller for order transformation result visualization
 */
class OrderTransformerResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const orderTransformerResult = ($scope: OrderTransformerResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<OrderTransformerResultData>(
                $http,
                $scope,
                "Loading order transformation results",
                "Failed loading order transformation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformerResultCtrl", ["$scope", "$http", orderTransformerResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
export default function OrderTransformerResultController(): OrderTransformerResultHandler {
    return new OrderTransformerResultHandler();
}
