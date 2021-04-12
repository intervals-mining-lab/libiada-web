function OrderTransformationResultController() {
    "use strict";

    function orderTransformationResult($scope, $http) {

        $scope.loadingScreenHeader = "Loading order transformation results";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get("/api/TaskManagerWebApi/", { params: { id: $scope.taskId } })
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));
                $scope.loading = false;
            }, function () {
                alert("Failed loading import results");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("OrderTransformationResultCtrl", ["$scope", "$http", orderTransformationResult]);
}
