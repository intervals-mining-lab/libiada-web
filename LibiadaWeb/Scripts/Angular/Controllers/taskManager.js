function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {
        function onStateChange(change) {
            if (change.newState === $.signalR.connectionState.disconnected)
                alertify.confirm('Connection lost', 'Connection lost. Refresh page?',
                    () => {
                        window.location.reload(true);
                    },
                    () => {
                    }
                );
            };

        function onHubStart(data) {
            $scope.$apply();
            $scope.tasksHub.server.getAllTasks().done(function (tasksJson) {
                var tasks = JSON.parse(tasksJson);
                for (var i = 0; i < tasks.length; i++) {
                    $scope.tasks.push(tasks[i]);

                    $scope.tryRedirectToResult(tasks[i]);
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

                        $scope.tryRedirectToResult(taskToChange);
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

        function calculateTaskCountWithState(state) {
            var count = $scope.tasks.filter(task => task.TaskState === state).length;
            return count;
        }

        function deleteAllTasks() {
            alertify.confirm('Confirm the action', 'Are you sure you want to delete all tasks?',
                () => {
                    $scope.tasksHub.server.deleteAllTasks();
                    alertify.success('All tasks have been deleted.');
                },
                () => {
                }
            );
        }

        function deleteTasksWithState(taskState) {
            alertify.confirm('Confirm the action', 'Are you sure you want to delete all tasks with state "' + taskState + '"?',
                () => {                    
                    $scope.tasksHub.server.deleteTasksWithState(taskState);
                    alertify.success('All tasks with state "' + taskState + '" have been deleted.');
                },
                () => {
                }
            );
        }

        function deleteTask(id)
        {
            alertify.confirm('Confirm the action', 'Are you sure you want to delete this task?',
                () => {
                    $scope.tasksHub.server.deleteTask(id);
                    alertify.success('The task has been deleted.');
                },
                () => {
                }
            );
        }

        function tryRedirectToResult(task) {
            if ($scope.autoRedirect &&
               (task.Id == $scope.RedirectTaskId) &&
               (task.TaskState === "Completed" || task.TaskState === "Error")) {
                document.location.href = window.location.origin + '/' + task.TaskType + '/Result/' + task.Id;
            }
        }

        $scope.onStateChange = onStateChange;
        $scope.onHubStart = onHubStart;
        $scope.taskEvent = taskEvent;
        $scope.calculateStatusClass = calculateStatusClass;
        $scope.calculateStatusGlyphicon = calculateStatusGlyphicon;
        $scope.calculateTaskCountWithState = calculateTaskCountWithState;
        $scope.deleteAllTasks = deleteAllTasks;
        $scope.deleteTasksWithState = deleteTasksWithState;
        $scope.deleteTask = deleteTask;
        $scope.tryRedirectToResult = tryRedirectToResult;

        $scope.tasksHub = $.connection.tasksManagerHub;
        $scope.tasksHub.client.TaskEvent = taskEvent;

        $.connection.hub.stateChanged(onStateChange);
        $.connection.hub.start().done(onHubStart);

        var location = window.location.href.split("/");
        if (location[location.length - 1] != "TaskManager")
            $scope.RedirectTaskId = location[location.length - 1];
        else $scope.RedirectTaskId = null;
        $scope.loadingScreenHeader = "Loading tasks";
        $scope.loading = true;
        $scope.tasks = [];
        $scope.flags = { reconnecting: false };
        $scope.autoRedirect = true;
    }

    angular.module("libiada").controller("TaskManagerCtrl", ["$scope", taskManager]);
}
