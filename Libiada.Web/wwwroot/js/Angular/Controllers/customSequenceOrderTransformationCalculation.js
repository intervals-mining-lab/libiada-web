import { MapModelFromJson } from "functions";
// Controller class
class CustomSequenceOrderTransformationCalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const customSequenceOrderTransformationCalculation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("CustomSequenceOrderTransformationCalculationCtrl", ["$scope", customSequenceOrderTransformationCalculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CustomSequenceOrderTransformationCalculationController(data) {
    return new CustomSequenceOrderTransformationCalculationOperator(data);
}
//# sourceMappingURL=customSequenceOrderTransformationCalculation.js.map