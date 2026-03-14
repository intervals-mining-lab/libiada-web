import { initScopeFromServer } from "functions";
/**
 * Angular controller class for combined sequence entity creation result view
 */
class CombinedSequencesEntityCreateResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const combinedSequencesEntityCreateResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading results", "Failed loading results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CombinedSequencesEntityCreateResultCtrl", ["$scope", "$http", combinedSequencesEntityCreateResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of combined sequences entity creation result handler
 */
export default function CombinedSequencesEntityCreateResultController() {
    return new CombinedSequencesEntityCreateResultHandler();
}
//# sourceMappingURL=combinedSequencesEntityCreateResult.js.map