import { MapModelFromJson } from "functions";

/**
* Interface for OrdersSimilarity controller input data
*/
interface OrdersSimilarityData {
    // List of all notations
    notations: { Nature: number; Value: string; Text: string }[];
    // List of available nature types (genetic, literary, etc.)
    natures: { Value: number; Text: string }[];
}

/**
* Interface for OrdersSimilarity external controller scope
*/
interface OrdersSimilarityScope extends ng.IScope, OrdersSimilarityData {
    // Selected nature (value)
    nature: number;
    // Selected notation
    notation: { Nature: number; Value: string; Text: string };

    // function to filter notations by nature
    filterByNature: () => void;
}

/**
* Controller class for comparing orders
*/
class OrdersSimilarityHandler  {
    /**
    * Creates a new controller instance.
    * @param data Data to create the controller
    */
    constructor(data: OrdersSimilarityData) {
        this.ngOnInit(data);
    }

    /**
    * Initializes the Angular controller.
    */
    private ngOnInit(data: OrdersSimilarityData): void {

        const ordersSimilarity = ($scope: OrdersSimilarityScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            function filterByNature() {
                $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            }

            $scope.filterByNature = filterByNature;
        };

        angular.module("libiada").controller("OrdersSimilarityCtrl", ["$scope", "filterFilter", ordersSimilarity]);
    }
}

/**
* wrapper function for backward compatibility
* @param data Data to create controller
* @returns OrdersSimilarityHandler instance
*/
export default function OrdersSimilarityController(data: OrdersSimilarityData): OrdersSimilarityHandler {
    return new OrdersSimilarityHandler(data);
}
