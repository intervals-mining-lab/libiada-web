function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {
        function onStateChange(change) {
            if (change.newState === $.signalR.connectionState.disconnected)
                if (confirm('Connection lost. Refresh page?')) {
                    location.reload(true);
                }
        };

        function onHubStart(data) {
            $scope.$apply();
            $scope.tasksHub.server.getAllTasks().done(function (tasksJson) {
                var tasks = JSON.parse(tasksJson);
                for (var i = 0; i < tasks.length; i++) {
                    $scope.tasks.push(tasks[i]);
                }
                $scope.loading = false;
                try {
                    $scope.$apply();
                } catch (e) { }
            });
        };

        function taskEvent(event, data) {
            switch (event) {
                case "AddTask":
                    $scope.tasks.push(data);
                    break;
                case "DeleteTask":
                    var taskToDelete = $scope.tasks.find(function (t) { return t.Id === data.Id; });
                    $scope.tasks.splice($scope.tasks.indexOf(taskToDelete), 1);
                    break;
                case "ChangeStatus":
                    var taskToChange = $scope.tasks.find(function (t) { return t.Id === data.Id; });
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
            try {
                $scope.$apply();
            } catch (e) { }
        };

        

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

        function deleteAllTasks() {
            if (confirm('Are you sure you want to delete all tasks?')) {
                $scope.tasksHub.server.deleteAllTasks();
            }
        }
        function deleteTask(id) {
            if (confirm('Are you sure you want to delete this task?')) {
                $scope.tasksHub.server.deleteTask(id);
            }
        }

        $scope.onStateChange = onStateChange;
        $scope.onHubStart = onHubStart;
        $scope.taskEvent = taskEvent;
        $scope.calculateStatusClass = calculateStatusClass;
        $scope.calculateStatusGlyphicon = calculateStatusGlyphicon;
        $scope.deleteAllTasks = deleteAllTasks;
        $scope.deleteTask = deleteTask;

        $scope.tasksHub = $.connection.tasksManagerHub;
        $scope.tasksHub.client.TaskEvent = taskEvent;

        $.connection.hub.stateChanged(onStateChange);
        $.connection.hub.start().done(onHubStart);

        $scope.loadingScreenHeader = "Loading tasks";
        $scope.loading = true;
        $scope.tasks = [];
        $scope.flags = { reconnecting: false };
    }

    angular.module("libiada").controller("TaskManagerCtrl", ["$scope", taskManager]);
    loadingWindow();
}
