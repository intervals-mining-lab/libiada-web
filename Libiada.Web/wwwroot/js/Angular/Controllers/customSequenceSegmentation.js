import { MapModelFromJson } from "functions";
// Angular controller class
class CustomSequenceSegmentationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const customSequenceSegmentation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("CustomSequenceSegmentationCtrl", ["$scope", customSequenceSegmentation]);
    }
}
// Wrapper function for backwards compatibility
export default function CustomSequenceSegmentationController(data) {
    return new CustomSequenceSegmentationOperator(data);
}
//# sourceMappingURL=customSequenceSegmentation.js.map