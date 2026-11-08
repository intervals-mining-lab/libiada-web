import { initScopeFromServer } from "functions";
/**
 * Angular controller for sequences alignment result view
 */
class SequencesAlignmentResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const sequencesAlignmentResult = async ($scope, $http) => {
            await initScopeFromServer($http, $scope, "Loading alignment results", "Failed loading alignment results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("SequencesAlignmentResultCtrl", ["$scope", "$http", sequencesAlignmentResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequences alignment result handler
 */
export default function SequencesAlignmentResultController() {
    return new SequencesAlignmentResultHandler();
}
//# sourceMappingURL=sequencesAlignmentResult.js.map