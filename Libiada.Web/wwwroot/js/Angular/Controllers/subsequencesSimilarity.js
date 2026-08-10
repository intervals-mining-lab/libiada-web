import { MapModelFromJson } from "functions";
// Controller class
class SubsequencesSimilarityOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const subsequencesSimilarity = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("SubsequencesSimilarityCtrl", ["$scope", subsequencesSimilarity]);
    }
}
// Wrapper function for backwards compatibility
export default function SubsequencesSimilarityController(data) {
    return new SubsequencesSimilarityOperator(data);
}
//# sourceMappingURL=subsequencesSimilarity.js.map