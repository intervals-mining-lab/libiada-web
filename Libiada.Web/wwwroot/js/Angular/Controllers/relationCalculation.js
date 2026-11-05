import { MapModelFromJson } from "functions";
class RelationCalculationHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        const relationCalculation = ($scope) => {
            MapModelFromJson($scope, data);
            $scope.showFilters = false;
            $scope.frequencyFilter = false;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("RelationCalculationCtrl", ["$scope", relationCalculation]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of relation calculation handler
 */
export default function RelationCalculationController(data) {
    return new RelationCalculationHandler(data);
}
//# sourceMappingURL=relationCalculation.js.map