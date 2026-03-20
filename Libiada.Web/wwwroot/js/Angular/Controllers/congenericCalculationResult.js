import { initScopeFromServer } from "functions";
/**
 * Angular controller class for congeneric characteristics calculation results visualization
 */
class CongenericCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const congenericCalculationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();
            initScopeFromServer($http, $scope, "Loading characteristics calculation results", "Failed loading characteristics calculation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CongenericCalculationResultCtrl", ["$scope", "$http", congenericCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CongenericCalculationResultController() {
    return new CongenericCalculationResultHandler();
}
//# sourceMappingURL=congenericCalculationResult.js.map