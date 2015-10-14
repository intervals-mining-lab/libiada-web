function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {
        MapModelFromJson($scope, data);

        function calculateStatusClass(status) {
            return status === "InProgress" ? "info"
                 : status === "Completed" ? "success"
                 : status === "Error" ? "danger" : "";
        }

        $scope.calculateStatusClass = calculateStatusClass;
    }

    angular.module("TaskManager", []).controller("TaskManagerCtrl", ["$scope", taskManager]);
}
