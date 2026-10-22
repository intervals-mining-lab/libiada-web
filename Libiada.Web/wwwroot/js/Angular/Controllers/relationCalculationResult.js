import { initScopeFromServer } from "functions";
/**
 * Controller for relation calculation result view
 */
class RelationCalculationResultHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const relationCalculationResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading calculation results", "Failed loading calculation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("RelationCalculationResultCtrl", ["$scope", "$http", relationCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of relation calculation result handler
 */
export default function AccordanceResultController() {
    return new RelationCalculationResultHandler();
}
//# sourceMappingURL=relationCalculationResult.js.map