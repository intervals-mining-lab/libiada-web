import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface SequenceCheckResultData {
    dbSequenceName: string;
    fileSequenceName: string;
    message: string;
    status: string;
}

/**
 * Interface for the controller's scope
 */
interface SequenceCheckResultScope extends ng.IScope, SequenceCheckResultData, ResultsLoadingScreenData {
    calculateStatusClass: (status: string) => string;
}

class SequenceCheckResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const sequenceCheckResult = ($scope: SequenceCheckResultScope, $http: ng.IHttpService): void => {
            // returns css class for given status
            function calculateStatusClass(status: string): string {
                return status === "Success" ? "text-success" : "text-danger";
            }

            $scope.calculateStatusClass = calculateStatusClass;
            initScopeFromServer<SequenceCheckResultData>(
                $http, $scope, "Loading sequence check results", "Failed loading sequence check results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SequenceCheckResultCtrl", ["$scope", "$http", sequenceCheckResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequence check result handler
 */
export default function SequenceCheckResultController(): SequenceCheckResultHandler {
    return new SequenceCheckResultHandler();
}
