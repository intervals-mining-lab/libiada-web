import { initScopeFromServer, InitializeBootstrapTooltips } from "functions";
/**
 * Angular controller class for integral characteristics calculation results visualization
 */
class CalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const calculationResult = async ($scope, $http) => {
            $scope.characteristicsTableTabSelected = false;
            await initScopeFromServer($http, $scope, "Loading characteristics calculation results", "Failed loading characteristics calculation results");
            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CalculationResultCtrl", ["$scope", "$http", calculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CalculationResultController() {
    return new CalculationResultHandler();
}
//# sourceMappingURL=calculationResult.js.map