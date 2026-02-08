import { MapModelFromJson } from "functions";
// Angular controller class
class CustomSequenceOrderTransformerOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const customSequenceOrderTransformer = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("CustomSequenceOrderTransformerCtrl", ["$scope", customSequenceOrderTransformer]);
    }
}
// Wrapper function for backwards compatibility
export default function CustomSequenceOrderTransformerController(data) {
    return new CustomSequenceOrderTransformerOperator(data);
}
//# sourceMappingURL=customSequenceOrderTransformer.js.map