import { MapModelFromJson } from "functions";
// Controller class
class GeneticSequencesTransformationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const geneticSequencesTransformation = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("GeneticSequencesTransformationCtrl", ["$scope", "filterFilter", geneticSequencesTransformation]);
    }
}
// Wrapper function for backwards compatibility
export default function GeneticSequencesTransformationController(data) {
    return new GeneticSequencesTransformationOperator(data);
}
//# sourceMappingURL=geneticSequencesTransformation.js.map