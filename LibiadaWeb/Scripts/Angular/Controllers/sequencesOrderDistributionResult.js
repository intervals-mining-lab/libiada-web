function SequencesOrderDistributionResultController() {
    "use strict";

    function sequencesOrderDistributionResult($scope, $http) {

		$scope.loadingScreenHeader = "Loading Data";
		$scope.loading = true;

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
		
        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));
				$scope.loading = false;
                
            }, function () {
                alert("Failed loading sequences order distribution data");
				$scope.loading = false;
            });
    }

	angular.module("libiada").controller("SequencesOrderDistributionResultCtrl", ["$scope", "$http", sequencesOrderDistributionResult]);
	
}
