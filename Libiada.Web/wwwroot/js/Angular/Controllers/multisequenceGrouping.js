import { MapModelFromJson } from "functions";
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
        const multisequenceGrouping = async ($scope, $http) => {
            /**
             * Removes a research object from a multisequence and adds it to ungrouped list
             * @param multiSequence The multisequence to remove from
             * @param researchObjectId The ID of the research object to remove
             */
            $scope.unbindResearchObject = function (multiSequence, researchObjectId) {
                multiSequence.researchObjectIds.splice(multiSequence.researchObjectIds.indexOf(researchObjectId), 1);
                $scope.ungroupedResearchObjects.push({
                    Id: researchObjectId,
                    Name: $scope.researchObjects[researchObjectId]
                });
            };
            /**
             * Adds a research object to a multisequence
             * @param multiSequence The multisequence to add to
             * @param index The index of the research object in ungrouped list
             */
            $scope.bindResearchObject = function (multiSequence, index) {
                let researchObject = $scope.ungroupedResearchObjects[index];
                $scope.researchObjects[researchObject.Id] = researchObject.Name;
                multiSequence.researchObjectIds.push(researchObject.Id);
                $scope.ungroupedResearchObjects.splice(index, 1);
            };
            $scope.loadingScreenHeader = "Loading grouping results";
            $scope.loading = true;
            try {
                const response = await $http.get(`/Multisequence/GroupResearchObjectsIntoMultisequences/`);
                MapModelFromJson($scope, response.data);
            }
            catch (error) {
                //TODO: change it to alertify
                alert("Failed loading grouping results");
            }
            finally {
                $scope.loading = false;
                $scope.$apply();
            }
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceGroupingCtrl", ["$scope", "$http", multisequenceGrouping]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of multisequence grouping handler
 */
export default function MultisequenceGroupingController() {
    return new MultisequenceGroupingHandler();
}
//# sourceMappingURL=multisequenceGrouping.js.map