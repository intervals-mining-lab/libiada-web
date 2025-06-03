/**
* Controller class for comparing orders
*/
class OrdersSimilarityHandler {
    /**
    * Creates a new controller instance.
    * @param data Data to create the controller
    */
    constructor(data) {
        this.initializeController(data);
    }
    /**
    * Initializes the Angular controller.
    */
    initializeController(data) {
        const ordersSimilarity = ($scope, filterFilter) => {
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
function OrdersSimilarityController(data) {
    return new OrdersSimilarityHandler(data);
}
//# sourceMappingURL=ordersSimilarity.js.map