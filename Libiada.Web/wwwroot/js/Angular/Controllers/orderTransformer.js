/// <reference types="angular" />
/**
* Controller for order transformation
*/
class OrderTransformerHandler {
    /**
    * Creates an instance of the order transformation controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    /**
    * Initializes the Angular controller
    */
    initializeController() {
        "use strict";
        const orderTransformer = ($scope, filterFilter) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);
            /**
            * Filters notations by the selected nature
            */
            function filterByNature() {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
                    // If the notation is not associated with a characteristic
                    if ($scope.characteristics) {
                        angular.forEach($scope.characteristics, (characteristic) => {
                            characteristic.notation = $scope.notation;
                        });
                    }
                }
            }
            // Assign methods to $scope
            $scope.filterByNature = filterByNature;
            // Initialize the selected values
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            $scope.language = $scope.languages ? $scope.languages[0] : undefined;
            $scope.translator = $scope.translators ? $scope.translators[0] : undefined;
            $scope.pauseTreatment = $scope.pauseTreatments ? $scope.pauseTreatments[0] : undefined;
        };
        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation controller instance
*/
function OrderTransformerController(data) {
    return new OrderTransformerHandler(data);
}
//# sourceMappingURL=orderTransformer.js.map