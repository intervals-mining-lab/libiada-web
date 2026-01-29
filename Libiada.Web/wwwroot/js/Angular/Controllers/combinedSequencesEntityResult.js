import { initScopeFromServer } from "functions";
class CombinedSequencesEntityResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const combinedSequencesEntityResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading results", "Failed loading results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CombinedSequencesEntityResultCtrl", ["$scope", "$http", combinedSequencesEntityResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of combined sequences entity creation result handler
 */
export default function CombinedSequencesEntityResultController() {
    return new CombinedSequencesEntityResultHandler();
}
//# sourceMappingURL=combinedSequencesEntityResult.js.map