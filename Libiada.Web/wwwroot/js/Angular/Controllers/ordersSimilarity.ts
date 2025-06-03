/**
* Interface for OrdersSimilarity controller input data
*/
interface IOrdersSimilarityData {
    // List of notations
    notations?: { Nature: number; Value: string; Text: string }[];
    // List of available nature types (genetic, literary, etc.)
    natures?: { Value: number; Text: string }[];
    // Other possible properties...
    [key: string]: any;
}

/**
* Interface for OrdersSimilarity external controller scope
*/
interface IOrdersSimilarityScope extends ng.IScope {
    // Selected nature (value)
    nature: number;
    // Selected notation
    notation: { Nature: number; Value: string; Text: string };
    // List of all notations
    notations: { Nature: number; Value: string; Text: string }[];
    // List of available nature types
    natures: { Value: number; Text: string }[];

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
    constructor(data: IOrdersSimilarityData) {
        this.initializeController(data);
    }

    /**
    * Initializes the Angular controller.
    */
    private initializeController(data: IOrdersSimilarityData): void {

        const ordersSimilarity = ($scope: IOrdersSimilarityScope, filterFilter: ng.IFilterFilter): void => {
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
function OrdersSimilarityController(data: IOrdersSimilarityData): OrdersSimilarityHandler {
    return new OrdersSimilarityHandler(data);
}