import { MapModelFromJson } from "functions";
// Controller class
class SubsequencesComparerOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const subsequencesComparer = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("SubsequencesComparerCtrl", ["$scope", subsequencesComparer]);
    }
}
// Wrapper function for backwards compatibility
export default function SubsequencesComparerController(data) {
    return new SubsequencesComparerOperator(data);
}
//# sourceMappingURL=subsequencesComparer.js.map