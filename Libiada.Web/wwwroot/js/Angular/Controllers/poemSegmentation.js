import { MapModelFromJson } from "functions";
// Controller class
class PoemSegmentationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const poemSegmentation = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("PoemSegmentationCtrl", ["$scope", "filterFilter", poemSegmentation]);
    }
}
// Wrapper function for backwards compatibility
export default function PoemSegmentationController(data) {
    return new PoemSegmentationOperator(data);
}
//# sourceMappingURL=poemSegmentation.js.map