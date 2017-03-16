function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {
        //MapModelFromJson($scope, data);

        var tasksHub = $.connection.tasksManagerHub;

        tasksHub.client.TaskEvent = function (data) {
            alert(data);
        };


        tasksHub.client.onConnected = function (data) {
            alert(data);
        }

        $.connection.hub.start().done(function (data) {
            tasksHub.server.getAllTasks().done(function (tasksJson) {
                var tasks = JSON.parse(tasksJson);
                for(var i = 0; i < tasks.length; i++)
                {
                    $scope.tasks.push(tasks[i]);
                }
                $scope.$apply();
            });
        });

        function calculateStatusClass(status) {
            return status === "InProgress" ? "info"
                 : status === "Completed" ? "success"
                 : status === "Error" ? "danger" : "";
        }

        function calculateStatusGlyphicon(status) {
            var icon = status === "InProgress" ? "glyphicon-tasks text-info"
                     : status === "Completed" ? "glyphicon-ok-sign text-success"
                     : status === "Error" ? "glyphicon-alert text-danger"
                     : status === "InQueue" ? "glyphicon-hourglass text-muted" : "";

            return "glyphicon " + icon;
        }

        function autoRefresh () {
            //window.location.reload();
        }

        $scope.calculateStatusClass = calculateStatusClass;
        $scope.calculateStatusGlyphicon = calculateStatusGlyphicon;
        $scope.autoRefresh = autoRefresh;
        $scope.tasks = [];

        setInterval($scope.autoRefresh, 1 * 60 * 1000);
    }

    angular.module("TaskManager", []).controller("TaskManagerCtrl", ["$scope", taskManager]);
}
