﻿function MusicFilesResultController() {
    "use strict";

    function musicFilesResult($scope, $http) {
        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);
            }, function () {
                alert("Failed loading characteristic data");
            });
    }

    angular.module("libiada").controller("MusicFilesResultCtrl", ["$scope", "$http", musicFilesResult]);
}
