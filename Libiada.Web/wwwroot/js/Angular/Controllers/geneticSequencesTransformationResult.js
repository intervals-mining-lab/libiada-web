import { initScopeFromServer } from "functions";
class GeneticSequencesTransformationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const geneticSequencesTransformationResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("GeneticSequencesTransformationResultCtrl", ["$scope", "$http", geneticSequencesTransformationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of genetic sequences transformation result handler
 */
export default function GeneticSequencesTransformationResultController() {
    return new GeneticSequencesTransformationResultHandler();
}
//# sourceMappingURL=geneticSequencesTransformationResult.js.map