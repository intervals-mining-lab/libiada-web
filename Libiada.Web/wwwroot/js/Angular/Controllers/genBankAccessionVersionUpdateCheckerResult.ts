import { initScopeFromServer } from "functions";

interface GenBankSearchResult {
    Updated: boolean;
    NameUpdated: boolean
    Name: string;
    RemoteName?: string;
    RemoteOrganism?: string;
    LocalAccession: string;
    LocalVersion: number;
    RemoteVersion: number;
    LocalUpdateDateTime: string;
    RemoteUpdateDate?: string;
}

interface GenBankAccessionVersionUpdateCheckerResultData {
    Results: GenBankSearchResult[];
}

interface GenBankAccessionVersionUpdateCheckerResultScope extends ng.IScope, GenBankAccessionVersionUpdateCheckerResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;

    calculateStatusClass(result: GenBankSearchResult): string;
}

class GenBankAccessionVersionUpdateCheckerResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const genBankAccessionVersionUpdateCheckerResult = ($scope: GenBankAccessionVersionUpdateCheckerResultScope, $http: ng.IHttpService): void => {
            $scope.calculateStatusClass = (result: GenBankSearchResult): string => {
                return result.Updated ? result.NameUpdated ? "table-warning" : "table-danger" : result.NameUpdated ? "" : "table-info";
            };

            initScopeFromServer<GenBankAccessionVersionUpdateCheckerResultData>($http, $scope, "Loading import results", "Failed loading import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("GenBankAccessionVersionUpdateCheckerResultCtrl", ["$scope", "$http", genBankAccessionVersionUpdateCheckerResult]);
    }

    
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of GenBank accession version update check result handler
 */
export default function GenBankAccessionVersionUpdateCheckerResultController(): GenBankAccessionVersionUpdateCheckerResultHandler {
    return new GenBankAccessionVersionUpdateCheckerResultHandler();
}
