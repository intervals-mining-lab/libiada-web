import { MapModelFromJson } from "functions";
class AccordanceCalculationHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        const accordanceCalculation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("AccordanceCalculationCtrl", ["$scope", accordanceCalculation]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance calculation handler
 */
export default function AccordanceCalculationController(data) {
    return new AccordanceCalculationHandler(data);
}
//# sourceMappingURL=accordanceCalculation.js.map