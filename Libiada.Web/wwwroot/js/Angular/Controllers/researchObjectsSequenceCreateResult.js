import { initScopeFromServer } from "functions";
class ResearchObjectsSequenceCreateResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const researchObjectsSequenceCreateResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading import results", "Failed loading import results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("ResearchObjectsSequenceCreateResultCtrl", ["$scope", "$http", researchObjectsSequenceCreateResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function ResearchObjectsSequenceCreateResultController() {
    return new ResearchObjectsSequenceCreateResultHandler();
}
//# sourceMappingURL=researchObjectsSequenceCreateResult.js.map