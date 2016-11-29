function SubsequencesComparerResultController() {
    "use strict";

    function subsequencesComparerResult($scope, $http) {
        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        $scope.loading = true;
        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));

            $scope.loading = false;
        }).error(function (data) {
            alert("Failed loading characteristic data");
        });
    }

    angular.module("SubsequencesComparerResult", []).controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
