/// <reference types="angular" />
/**
 * Controller for multisequence creation
 */
class MultisequenceCreateHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.initializeController(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    initializeController(data) {
        "use strict";
        const multisequenceCreate = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            function filterByNature() {
                // Implementation left empty as in original
            }
            $scope.filterByNature = filterByNature;
            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceCreateCtrl", ["$scope", "filterFilter", multisequenceCreate]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of multisequence create handler
 */
function MultisequenceCreateController(data) {
    return new MultisequenceCreateHandler(data);
}
//# sourceMappingURL=multisequenceCreate.js.map