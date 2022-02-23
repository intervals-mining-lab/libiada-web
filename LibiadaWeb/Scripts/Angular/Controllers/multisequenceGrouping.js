function MultisequenceGroupingController() {
    "use strict";

    function multisequenceGrouping($scope, $http) {

        function unbindMatter(multiSequence, matterId) {
            multiSequence.matterIds.splice(multiSequence.matterIds.indexOf(matterId), 1);
            $scope.ungroupedMatters.push({ Id: matterId, Name: $scope.matters[matterId] });
        }

        function bindMatter(multiSequence, index) {
            var matter = $scope.ungroupedMatters[index];
            $scope.matters[matter.Id] = matter.Name;
            multiSequence.matterIds.push(matter.Id);
            $scope.ungroupedMatters.splice(index, 1);
        }

        $scope.bindMatter = bindMatter;
        $scope.unbindMatter = unbindMatter;

        $scope.loadingScreenHeader = "Loading grouping results";

        $scope.loading = true;

        $http.get(`/Multisequence/GroupMattersIntoMultisequences/`)
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
