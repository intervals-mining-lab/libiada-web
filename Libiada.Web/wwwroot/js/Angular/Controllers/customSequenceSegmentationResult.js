import { initScopeFromServer } from "functions";
/**
 * Angular controller class for custom sequence segmentation result visualization
 */
class CustomSequenceSegmentationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const customSequenceSegmentationResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading custom sequence segmentation results", "Failed loading custom sequence segmentation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("SegmentationResultCtrl", ["$scope", "$http", customSequenceSegmentationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceSegmentationResultController() {
    return new CustomSequenceSegmentationResultHandler();
}
//# sourceMappingURL=customSequenceSegmentationResult.js.map