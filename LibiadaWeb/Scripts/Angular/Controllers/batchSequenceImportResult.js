function BatchSequenceImportResultController() {
    "use strict";

    function batchSequenceImportResult($scope, $http) {

        // returns css class for given status
        function calculateStatusClass(status) {
            return status === "Success" ? "success"
                 : status === "Exist" ? "info"
                 : status === "Error" ? "danger" : "";
        }

        // shows modal window with progressbar and given text
        function showModalLoadingWindow(headerText) {
            $scope.loadingScreenHeader = headerText;
            $scope.loadingModalWindow.modal("show");
            $scope.loading = true;
        }

        // hides modal window
        function hideModalLoadingWindow() {
            $scope.loading = false;
            $scope.loadingModalWindow.modal("hide");
        }

        $scope.calculateStatusClass = calculateStatusClass;
        $scope.showModalLoadingWindow = showModalLoadingWindow;
        $scope.hideModalLoadingWindow = hideModalLoadingWindow;

        $scope.loadingModalWindow = $("#loadingDialog");

        $scope.showModalLoadingWindow("Loading import results");

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));
            $scope.hideModalLoadingWindow();
        }).error(function (data) {
            alert("Failed loading import results");
        });
    }

    angular.module("BatchSequenceImportResult", []).controller("BatchSequenceImportResultCtrl", ["$scope", "$http", batchSequenceImportResult]);
}