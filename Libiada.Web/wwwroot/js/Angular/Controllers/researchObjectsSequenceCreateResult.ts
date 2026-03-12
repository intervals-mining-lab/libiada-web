import { initScopeFromServer } from "functions";

interface ResearchObjectsSequenceCreateResultData {
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

interface ResearchObjectsSequenceCreateResultScope extends ng.IScope, ResearchObjectsSequenceCreateResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class ResearchObjectsSequenceCreateResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const researchObjectsSequenceCreateResult = ($scope: ResearchObjectsSequenceCreateResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<ResearchObjectsSequenceCreateResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("ResearchObjectsSequenceCreateResultCtrl", ["$scope", "$http", researchObjectsSequenceCreateResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function ResearchObjectsSequenceCreateResultController(): ResearchObjectsSequenceCreateResultHandler {
    return new ResearchObjectsSequenceCreateResultHandler();
}
