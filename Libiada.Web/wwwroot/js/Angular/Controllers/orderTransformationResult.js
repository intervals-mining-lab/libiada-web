﻿function OrderTransformationResultController() {
    "use strict";

    function orderTransformationResult($scope, $http) {

        $scope.loadingScreenHeader = "Loading order transformation results";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            }, function () {
                alert("Failed loading import results");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("OrderTransformationResultCtrl", ["$scope", "$http", orderTransformationResult]);
}
