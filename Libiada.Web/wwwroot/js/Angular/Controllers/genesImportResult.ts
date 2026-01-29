import { initScopeFromServer } from "functions";
interface SubsequenceData {
    Id: number;
    Starts: number[];
    Lengths: number[];
    FeatureId: number;
    Partial: boolean;
    RemoteId?: string;
}

interface GenesImportResultData {
    researchObjectName: string;
    genes: SubsequenceData[];
    features: { [key: number]: string; };
}

interface GenesImportResultScope extends ng.IScope, GenesImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

class GenesImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const genesImportResult = ($scope: GenesImportResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<GenesImportResultData>($http, $scope, "Loading genes import results", "Failed loading genes import results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("GenesImportResultCtrl", ["$scope", "$http", genesImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of genes import result handler
 */
export default function GenesImportResultController(): GenesImportResultHandler {
    return new GenesImportResultHandler();
}
