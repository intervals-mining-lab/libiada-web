import { initScopeFromServer } from "functions";

interface CombinedSequencesEntityResultData {
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

interface CombinedSequencesEntityResultScope extends ng.IScope, CombinedSequencesEntityResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class CombinedSequencesEntityResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const combinedSequencesEntityResult = ($scope: CombinedSequencesEntityResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<CombinedSequencesEntityResultData>($http, $scope, "Loading results", "Failed loading results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("CombinedSequencesEntityResultCtrl", ["$scope", "$http", combinedSequencesEntityResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of combined sequences entity creation result handler
 */
export default function CombinedSequencesEntityResultController(): CombinedSequencesEntityResultHandler {
    return new CombinedSequencesEntityResultHandler();
}
