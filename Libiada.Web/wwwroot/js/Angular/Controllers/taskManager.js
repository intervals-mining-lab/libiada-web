function TaskManagerController() {
    "use strict";

    function taskManager($scope) {
        function onCloseConnection() {
            alertify.error('Connection lost', 5);
        };

        function onHubStart() {
            $scope.$apply();
            $scope.tasksHub.invoke("getAllTasks").then(tasks => {
                for (let i = 0; i < tasks.length; i++) {
                    let task = tasks[i];
                    task.resultLink = `${window.location.origin}/${task.TaskType}/Result/${task.Id}`;
                    $scope.tasks.push(task);
                    $scope.tryRedirectToResult(task);
                }

                $scope.loading = false;
                try {
                    $scope.$apply();
                } catch (e) { console.error(e.toString()); }
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
            } catch (e) { console.error(e.toString()); }
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
            return $scope.tasks.filter(task => task.TaskState === state).length;
        }

        function deleteAllTasks() {
            alertify.confirm('Confirm action', 'Are you sure you want to delete all tasks?',
                () => {
                    $scope.tasks.forEach(t => t.Deleting = true);
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteAllTasks")
                        .then(alertify.success('All tasks have been deleted.'));
                }, () => { });
        }

        function deleteTasksWithStatus(taskState) {
            alertify.confirm('Confirm action', `Are you sure you want to delete all tasks with "${taskState}" status?`,
                () => {
                    $scope.tasks.filter(t => t.taskState == taskState).forEach(t => t.Deleting = true);
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteTasksWithState", taskState)
                        .then(() => alertify.success(`All tasks with "${taskState}" status have been deleted.`));
                }, () => { });
        }

        function deleteTask(id) {
            alertify.confirm('Confirm action', 'Are you sure you want to delete this task?',
                () => {
                    let taskToDelete = $scope.tasks.find(t => t.Id = id);
                    taskToDelete.Deleting = true;
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteTask", id)
                        .then(() => alertify.success('The task has been deleted.'));
                }, () => { });
        }

        function tryRedirectToResult(task) {
            if ($scope.autoRedirect &&
                (task.Id == $scope.RedirectTaskId) &&
                (task.TaskState === "Completed" || task.TaskState === "Error")) {
                document.location.href = `${window.location.origin}/${task.TaskType}/Result/${task.Id}`;
            }
        }

        $scope.onCloseConnection = onCloseConnection;
        $scope.onHubStart = onHubStart;
        $scope.taskEvent = taskEvent;
        $scope.getStatusClass = getStatusClass;
        $scope.getStatusIcon = getStatusIcon;
        $scope.getTaskCountWithStatus = getTaskCountWithStatus;
        $scope.deleteAllTasks = deleteAllTasks;
        $scope.deleteTasksWithStatus = deleteTasksWithStatus;
        $scope.deleteTask = deleteTask;
        $scope.tryRedirectToResult = tryRedirectToResult;


        //initializing signalr connection
        $scope.tasksHub = new signalR.HubConnectionBuilder().withUrl("/TaskManagerHub").build();

        $scope.tasksHub.on("taskEvent", $scope.taskEvent);
        $scope.tasksHub.onclose($scope.onCloseConnection);

        $scope.tasksHub.start().then($scope.onHubStart).catch(function (err) {
            return console.error(err.toString());
        });

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
