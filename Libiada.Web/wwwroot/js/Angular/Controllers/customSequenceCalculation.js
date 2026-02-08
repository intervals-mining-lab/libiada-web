import { MapModelFromJson } from "functions";
// Controller class
class CustomSequenceCalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const customSequenceCalculation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("CustomSequenceCalculationCtrl", ["$scope", customSequenceCalculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CustomSequenceCalculationController(data) {
    return new CustomSequenceCalculationOperator(data);
}
//# sourceMappingURL=customSequenceCalculation.js.map