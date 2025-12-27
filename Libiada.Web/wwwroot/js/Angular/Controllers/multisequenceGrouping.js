"use strict";
/// <reference types="angular" />
/**
 * Controller for multisequence grouping
 */
class MultisequenceGroupingHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        "use strict";
        const multisequenceGrouping = ($scope, $http) => {
            /**
             * Removes a research object from a multisequence and adds it to ungrouped list
             * @param multiSequence The multisequence to remove from
             * @param researchObjectId The ID of the research object to remove
             */
            function unbindResearchObject(multiSequence, researchObjectId) {
                multiSequence.researchObjectIds.splice(multiSequence.researchObjectIds.indexOf(researchObjectId), 1);
                $scope.ungroupedResearchObjects.push({
                    Id: researchObjectId,
                    Name: $scope.researchObjects[researchObjectId]
                });
            }
            /**
             * Adds a research object to a multisequence
             * @param multiSequence The multisequence to add to
             * @param index The index of the research object in ungrouped list
             */
            function bindResearchObject(multiSequence, index) {
                let researchObject = $scope.ungroupedResearchObjects[index];
                $scope.researchObjects[researchObject.Id] = researchObject.Name;
                multiSequence.researchObjectIds.push(researchObject.Id);
                $scope.ungroupedResearchObjects.splice(index, 1);
            }
            $scope.bindResearchObject = bindResearchObject;
            $scope.unbindResearchObject = unbindResearchObject;
            $scope.loadingScreenHeader = "Loading grouping results";
            $scope.loading = true;
            $http.get(`/Multisequence/GroupResearchObjectsIntoMultisequences/`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading grouping results");
                $scope.loading = false;
            });
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceGroupingCtrl", ["$scope", "$http", multisequenceGrouping]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of multisequence grouping handler
 */
function MultisequenceGroupingController() {
    return new MultisequenceGroupingHandler();
}
//# sourceMappingURL=multisequenceGrouping.js.map