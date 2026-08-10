import { MapModelFromJson } from "functions";
// Controller class
class SequencePredictionOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const sequencePrediction = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("SequencePredictionCtrl", ["$scope", sequencePrediction]);
    }
}
// Wrapper function for backwards compatibility
export default function SequencePredictionController(data) {
    return new SequencePredictionOperator(data);
}
//# sourceMappingURL=sequencePrediction.js.map