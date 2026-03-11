import { initScopeFromServer } from "functions";
/**
 * Angular controller class for poem segmentation result view
 */
class PoemSegmentationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const poemSegmentationResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading segmentation results", "Failed loading segmentation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("PoemSegmentationResultCtrl", ["$scope", "$http", poemSegmentationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function PoemSegmentationResultController() {
    return new PoemSegmentationResultHandler();
}
//# sourceMappingURL=poemSegmentationResult.js.map