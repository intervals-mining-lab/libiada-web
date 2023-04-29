function TaskManagerController() {
    "use strict";

    function taskManager($scope) {
        function onStateChange(change) {
            if (change.newState === $.signalR.connectionState.disconnected) {
                alertify.error('Connection lost', 5);
            }
        };

        function onHubStart() {
            $scope.$apply();
            $scope.tasksHub.server.getAllTasks().done(tasksJson => {
                let tasks = JSON.parse(tasksJson);
                for (let i = 0; i < tasks.length; i++) {
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
                    let taskToDelete = $scope.tasks.find(t => t.Id === data.Id);
                    $scope.tasks.splice($scope.tasks.indexOf(taskToDelete), 1);
                    break;
                case "ChangeStatus":
                    let taskToChange = $scope.tasks.find(t => t.Id === data.Id);
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

        function getStatusClass(status) {
            return status === "InProgress" ? "table-info"
                 : status === "Completed" ? "table-success"
                 : status === "Error" ? "table-danger" : "";
        }

        function getStatusIcon(status) {
            return status === "InProgress" ? "bi-play-circle-fill text-info"
                 : status === "Completed" ? "bi-check-circle-fill text-success"
                 : status === "Error" ? "bi-x-circle-fill text-danger"
                 : status === "InQueue" ? "bi-pause-circle-fill text-muted" : "";
        }

        function getTaskCountWithStatus(state) {
            let count = $scope.tasks.filter(task => task.TaskState === state).length;
            return count;
        }

        function deleteAllTasks() {
            alertify.confirm('Confirm action', 'Are you sure you want to delete all tasks?',
                () => {
                    $scope.tasksHub.server.deleteAllTasks();
                    alertify.success('All tasks have been deleted.');
                }, () => { });
        }

        function deleteTasksWithStatus(taskState) {
            alertify.confirm('Confirm action', 'Are you sure you want to delete all tasks with "' + taskState + '" status?',
                () => {
                    $scope.tasksHub.server.deleteTasksWithState(taskState);
                    alertify.success('All tasks with "' + taskState + '" status have been deleted.');
                }, () => { });
        }

        function deleteTask(id) {
            alertify.confirm('Confirm action', 'Are you sure you want to delete this task?',
                () => {
                    $scope.tasksHub.server.deleteTask(id);
                    alertify.success('The task has been deleted.');
                }, () => { });
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
        $scope.getStatusClass = getStatusClass;
        $scope.getStatusIcon = getStatusIcon;
        $scope.getTaskCountWithStatus = getTaskCountWithStatus;
        $scope.deleteAllTasks = deleteAllTasks;
        $scope.deleteTasksWithStatus = deleteTasksWithStatus;
        $scope.deleteTask = deleteTask;
        $scope.tryRedirectToResult = tryRedirectToResult;

        $scope.tasksHub = $.connection.tasksManagerHub;
        $scope.tasksHub.client.TaskEvent = taskEvent;

        $.connection.hub.stateChanged($scope.onStateChange);
        $.connection.hub.start().done($scope.onHubStart);

        let location = window.location.href.split("/");
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
