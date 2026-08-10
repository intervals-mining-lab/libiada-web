import { MapModelFromJson } from "functions";
// Controller class
class PoemSegmentationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const poemSegmentation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("PoemSegmentationCtrl", ["$scope", poemSegmentation]);
    }
}
// Wrapper function for backwards compatibility
export default function PoemSegmentationController(data) {
    return new PoemSegmentationOperator(data);
}
//# sourceMappingURL=poemSegmentation.js.map