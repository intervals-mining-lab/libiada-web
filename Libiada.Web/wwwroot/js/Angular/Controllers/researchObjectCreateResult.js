import { initScopeFromServer } from "functions";
/**
 * Angular controller class for research object creation result view
 */
class ResearchObjectCreateResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const researchObjectCreateResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading sequence creation results", "Failed loading sequence creation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("ResearchObjectCreateResultCtrl", ["$scope", "$http", researchObjectCreateResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function ResearchObjectCreateResultController() {
    return new ResearchObjectCreateResultHandler();
}
//# sourceMappingURL=researchObjectCreateResult.js.map