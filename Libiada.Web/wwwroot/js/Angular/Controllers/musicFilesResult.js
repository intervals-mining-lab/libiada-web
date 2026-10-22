import { initScopeFromServer } from "functions";
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
    ngOnInit() {
        const musicFilesResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading music files data", "Failed loading music files data");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MusicFilesResultCtrl", ["$scope", "$http", musicFilesResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of MusicFilesResultHandler
 */
export default function MusicFilesResultController() {
    return new MusicFilesResultHandler();
}
//# sourceMappingURL=musicFilesResult.js.map