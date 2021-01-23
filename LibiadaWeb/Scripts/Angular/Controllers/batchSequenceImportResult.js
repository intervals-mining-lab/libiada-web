function BatchSequenceImportResultController() {
    "use strict";

    function batchSequenceImportResult($scope, $http) {

        // returns css class for given status
        function calculateStatusClass(status) {
            return status === "Success" ? "success"
                : status === "Exists" ? "info"
                    : status === "Error" ? "danger" : "";
        }

        $scope.calculateStatusClass = calculateStatusClass;

        $scope.loadingScreenHeader = "Loading import results";

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

    angular.module("libiada").controller("BatchSequenceImportResultCtrl", ["$scope", "$http", batchSequenceImportResult]);
}