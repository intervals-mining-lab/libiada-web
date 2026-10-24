import { initScopeFromServer } from "functions";
// Controller for subsequences similarity result visualization
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.ngOnInit();
    }
    // Initializes the Angular controller
    ngOnInit() {
        const subsequencesSimilarityResult = ($scope, $http) => {
            initScopeFromServer($http, $scope, "Loading subsequences data", "Failed loading subsequences data");
        };
        // Register controller with Angular
        angular.module("libiada")
            .controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", subsequencesSimilarityResult]);
    }
}
// Wrapper function for backward compatibility
export default function SubsequencesSimilarityResultController() {
    return new SubsequencesSimilarityResultHandler();
}
//# sourceMappingURL=subsequencesSimilarityResult.js.map