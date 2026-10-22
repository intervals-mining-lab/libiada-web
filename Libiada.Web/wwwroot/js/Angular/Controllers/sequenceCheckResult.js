import { initScopeFromServer } from "functions";
class SequenceCheckResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const sequenceCheckResult = ($scope, $http) => {
            // returns css class for given status
            function calculateStatusClass(status) {
                return status === "Success" ? "text-success" : "text-danger";
            }
            $scope.calculateStatusClass = calculateStatusClass;
            initScopeFromServer($http, $scope, "Loading sequence check results", "Failed loading sequence check results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("SequenceCheckResultCtrl", ["$scope", "$http", sequenceCheckResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequence check result handler
 */
export default function SequenceCheckResultController() {
    return new SequenceCheckResultHandler();
}
//# sourceMappingURL=sequenceCheckResult.js.map