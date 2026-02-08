import { MapModelFromJson } from "functions";
// Controller class
class CongenericCalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const congenericCalculation = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("CongenericCalculationCtrl", ["$scope", "filterFilter", congenericCalculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CongenericCalculationController(data) {
    return new CongenericCalculationOperator(data);
}
//# sourceMappingURL=congenericCalculation.js.map