/// <reference types="angular" />
/// <reference types="functions" />
/**
 * Controller for subsequences similarity result visualization
 */
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.initializeController();
    }
    /**
     * Initializes the Angular controller
     */
    initializeController() {
        "use strict";
        const subsequencesSimilarityResult = ($scope, $http, $sce) => {
            // Gets attributes text for given subsequence
            $scope.getAttributesText = (attributes) => {
                const attributesText = [];
                for (let i = 0; i < attributes.length; i++) {
                    const attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($sce.trustAsHtml($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`)));
                }
                return attributesText;
            };
            // Shows the position
            $scope.showPosition = (index) => {
                $scope.index = index;
            };
            // Get task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Initialize loading state
            $scope.loadingScreenHeader = "Loading subsequences similarity";
            $scope.loading = true;
            // Load data from server
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.index = 0;
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading subsequences similarity");
                $scope.loading = false;
            });
        };
        // Register controller with Angular
        angular.module("libiada").controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", "$sce", subsequencesSimilarityResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 */
function SubsequencesSimilarityResultController() {
    return new SubsequencesSimilarityResultHandler();
}
//# sourceMappingURL=subsequencesSimilarityResult.js.map