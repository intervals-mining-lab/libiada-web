import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface CombinedSequencesEntityCreateResultData {
    Name: string;
    Nature: string;
    Notation: string;
    Group: string;
    SequenceType: string;
    Description: string;
    RemoteId?: string;
    Language?: string;
    Original?: boolean;
    Translator?: string;
    Partial?: boolean;
    Precision?: number;
    MultisequenceName?: string;
    MultisequenceNumber?: number;
    CollectionCountry?: string;
    CollectionDate?: string;
}

/**
 * Interface for the angular controller's scope
 */
interface CombinedSequencesEntityCreateResultScope 
    extends ng.IScope, CombinedSequencesEntityCreateResultData, ResultsLoadingScreenData {
}

/**
 * Angular controller class for combined sequence entity creation result view
 */
class CombinedSequencesEntityCreateResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const combinedSequencesEntityCreateResult = (
            $scope: CombinedSequencesEntityCreateResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<CombinedSequencesEntityCreateResultData>(
                $http, $scope, "Loading results", "Failed loading results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CombinedSequencesEntityCreateResultCtrl", 
            ["$scope", "$http", combinedSequencesEntityCreateResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of combined sequences entity creation result handler
 */
export default function CombinedSequencesEntityCreateResultController(): CombinedSequencesEntityCreateResultHandler {
    return new CombinedSequencesEntityCreateResultHandler();
}
