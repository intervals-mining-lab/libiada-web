function SequencesOrderDistributionResultController() {
    "use strict";

    function sequencesOrderDistributionResult($scope, $http) {

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

        

       
        $scope.showModalLoadingWindow = showModalLoadingWindow;
        $scope.hideModalLoadingWindow = hideModalLoadingWindow;

        $scope.loadingModalWindow = $("#loadingDialog");

        $scope.showModalLoadingWindow("Loading data");

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));
               
                $scope.hideModalLoadingWindow();
            }, function () {
                alert("Failed loading sequences order distribution data");
                $scope.hideModalLoadingWindow();
            });


    }

    angular.module("libiada").controller("SequencesOrderDistributionResultCtrl", ["$scope", "$http", sequencesOrderDistributionResult]);
}
