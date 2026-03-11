import { initScopeFromServer } from "functions";
/**
 * Angular controller class for custom sequence order transformer result visualization
 */
class CustomSequenceOrderTransformerResultHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit() {
        const customSequenceOrderTransformerResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading order transformation results", "Failed loading order transformation results");
        };
        // Register controller in Angular module
        angular.module("libiada").controller("CustomSequenceOrderTransformerCtrl", ["$scope", "$http", customSequenceOrderTransformerResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
export default function CustomSequenceOrderTransformerResultController() {
    return new CustomSequenceOrderTransformerResultHandler();
}
//# sourceMappingURL=customSequenceOrderTransformerResult.js.map