import { initScopeFromServer } from "functions";

/**
 * Interface for the data object fetched from the server
 */
interface ResearchObjectCreateResultData {
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
interface ResearchObjectCreateResultScope extends ng.IScope, ResearchObjectCreateResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

/**
 * Angular controller class for research object creation result view
 */
class ResearchObjectCreateResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const researchObjectCreateResult = ($scope: ResearchObjectCreateResultScope, $http: ng.IHttpService): void => {

            initScopeFromServer<ResearchObjectCreateResultData>($http, $scope, "Loading sequence creation results", "Failed loading sequence creation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("ResearchObjectCreateResultCtrl", ["$scope", "$http", researchObjectCreateResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function ResearchObjectCreateResultController(): ResearchObjectCreateResultHandler {
    return new ResearchObjectCreateResultHandler();
}
