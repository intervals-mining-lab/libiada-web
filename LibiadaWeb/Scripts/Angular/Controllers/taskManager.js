function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {

        var tasksHub = $.connection.tasksManagerHub;

        tasksHub.client.TaskEvent = function (event, data) {

            switch (event) {
                case "addTask":
                    $scope.tasks.push(data);
                    break;
                case "deleteTask":
                    var taskToDelete = $scope.tasks.find(function (t) { return t.Id == data.Id; });
                    $scope.tasks.splice($scope.tasks.indexOf(taskToDelete), 1);
                    break;
                case "changeStatus":
                    var taskToChange = $scope.tasks.find(function (t) { return t.Id == data.Id; });
                    if (taskToChange) {
                        taskToChange.Created = data.Created;
                        taskToChange.Started = data.Started;
                        taskToChange.Completed = data.Completed;
                        taskToChange.ExecutionTime = data.ExecutionTime;
                        taskToChange.TaskState = data.TaskState;
                        taskToChange.TaskStateName = data.TaskStateName;
                    }
                    else {
                        $scope.tasks.push(data);
                    }
                    break;
                default: console.log("Unknown task event");
                    break;
            }

            $scope.$apply();
        };


        tasksHub.client.onConnected = function (data) {
            alert(data);
        }

        $.connection.hub.start().done(function (data) {
            tasksHub.server.getAllTasks().done(function (tasksJson) {
                var tasks = JSON.parse(tasksJson);
                for (var i = 0; i < tasks.length; i++) {
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

        $scope.calculateStatusClass = calculateStatusClass;
        $scope.calculateStatusGlyphicon = calculateStatusGlyphicon;
        $scope.tasks = [];
    }

    angular.module("TaskManager", []).controller("TaskManagerCtrl", ["$scope", taskManager]);
}
