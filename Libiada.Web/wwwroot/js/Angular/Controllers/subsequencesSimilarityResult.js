function SubsequencesSimilarityResultController() {
    "use strict";

    function subsequencesSimilarityResult($scope, $http) {


        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get(`/api/TaskManagerWebApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.loading = false;
            }, function () {
                alert("Failed loading similarity data");
                $scope.loading = false;
            });


    }

    angular.module("libiada").controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", subsequencesSimilarityResult]);
}
