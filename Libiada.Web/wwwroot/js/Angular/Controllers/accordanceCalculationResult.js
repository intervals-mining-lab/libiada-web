import { initScopeFromServer } from "functions";
/**
 * Angular controller class
 */
class AccordanceCalculationResultHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit() {
        const accordanceCalculationResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading accordance data", "Failed loading accordance data");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("AccordanceCalculationResultCtrl", ["$scope", "$http", accordanceCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance calculation result handler
 */
export default function AccordanceCalculationResultController() {
    return new AccordanceCalculationResultHandler();
}
//# sourceMappingURL=accordanceCalculationResult.js.map