function MultisequenceGroupingController() {
    "use strict";

    function multisequenceGrouping($scope, $http) {

        function unbindResearchObject(multiSequence, researchObjectId) {
            multiSequence.researchObjectIds.splice(multiSequence.researchObjectIds.indexOf(researchObjectId), 1);
            $scope.ungroupedResearchObjects.push({ Id: researchObjectId, Name: $scope.researchObjects[researchObjectId] });
        }

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
            }, function () {
                alert("Failed loading grouping results");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("MultisequenceGroupingCtrl", ["$scope", "$http", multisequenceGrouping]);
}
