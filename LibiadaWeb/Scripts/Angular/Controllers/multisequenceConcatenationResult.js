function MultiSequenceConcatenationResultController() {
    "use strict";

    function multiSequenceConcatenationResult($scope, $http) {

        function unboundMatter(multiSequence, matterId) {
            multiSequence.matterIds.splice(multiSequence.matterIds.indexOf(matterId), 1);
            $scope.ungroupedMatters.push($scope.matters[matterId]);
        }

        function distributeMatter(multiSequence, matterId) {
            multiSequence.matterIds.push(matterId);
            $scope.ungroupedMatters.splice($scope.ungroupedMatters.indexOf(matterId), 1);
        }

        $scope.distributeMatter = distributeMatter;
        $scope.unboundMatter = unboundMatter;

        $scope.loadingScreenHeader = "Loading grouping results";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function(data) {
                    MapModelFromJson($scope, JSON.parse(data.data));
                    $scope.loading = false;
                },
                function() {
                    alert("Failed loading grouping results");
                    $scope.loading = false;
                });
    }

    angular.module("libiada").controller("MultiSequenceConcatenationResultCtrl", ["$scope", "$http", multiSequenceConcatenationResult]);
}
