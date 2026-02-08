import { MapModelFromJson } from "functions";
// Controller class
class LocalCalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const localCalculation = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("LocalCalculationCtrl", ["$scope", "filterFilter", localCalculation]);
    }
}
// Wrapper function for backwards compatibility
export default function LocalCalculationController(data) {
    return new LocalCalculationOperator(data);
}
//# sourceMappingURL=localCalculation.js.map