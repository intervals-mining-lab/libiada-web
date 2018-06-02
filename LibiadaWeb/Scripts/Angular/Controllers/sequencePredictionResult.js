function SequencePredictionResultController() {
    "use strict";

    function sequencePredictionResult($scope, $http) {


        $scope.loadingScreenHeader = "Loading data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

             
                $scope.firstCharacteristic = $scope.characteristicsList[0];
                $scope.secondCharacteristic = $scope.characteristicsList.length > 1 ? $scope.characteristicsList[1] : $scope.characteristicsList[0];

                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });


    }

    angular.module("libiada").controller("SequencePredictionResultCtrl", ["$scope", "$http", sequencePredictionResult]);
    
}
