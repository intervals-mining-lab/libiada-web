import { initScopeFromServer, MapModelFromJson } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface MusicFilesResultData {
    data: {
        name: string;
        sampleRate: number;
        channels: number;
        audioFormat: number;
        sampleSize: number;
    }[]
}

/**
 * Interface for the angular controller's scope
 */
interface MusicFilesResultScope extends ng.IScope, MusicFilesResultData, ResultsLoadingScreenData {
}

/**
 * Controller for displaying music files processing results
 */
class MusicFilesResultHandler {
    /**
     * Creates a new instance of the controller
     */
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const musicFilesResult = ($scope: MusicFilesResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<MusicFilesResultData>(
                $http, $scope, "Loading music files data", "Failed loading music files data");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MusicFilesResultCtrl", ["$scope", "$http", musicFilesResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of MusicFilesResultHandler
 */
export default function MusicFilesResultController(): MusicFilesResultHandler {
    return new MusicFilesResultHandler();
}
