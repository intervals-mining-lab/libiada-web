function MusicFilesResultController() {
    "use strict";

    function musicFilesResult($scope, $http) {
        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));
            }, function () {
                alert("Failed loading characteristic data");
            });
    }

    angular.module("libiada").controller("MusicFilesResultCtrl", ["$scope", "$http", musicFilesResult]);
}
