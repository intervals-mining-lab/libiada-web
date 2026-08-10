import { MapModelFromJson } from "functions";
// Controller class
class GeneticSequencesTransformationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const geneticSequencesTransformation = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("GeneticSequencesTransformationCtrl", ["$scope", geneticSequencesTransformation]);
    }
}
// Wrapper function for backwards compatibility
export default function GeneticSequencesTransformationController(data) {
    return new GeneticSequencesTransformationOperator(data);
}
//# sourceMappingURL=geneticSequencesTransformation.js.map