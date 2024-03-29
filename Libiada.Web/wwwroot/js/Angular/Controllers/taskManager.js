function TaskManagerController() {
    "use strict";

    function taskManager($scope) {
        function onCloseConnection() {
            alertify.error("Connection lost", 5);
        };

        function onHubStart(tasks) {
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
        };

        function taskEvent(event, taskData) {
            switch (event) {
                case "AddTask":
                    $scope.tasks.push(taskData);
                    break;
                case "DeleteTask":
                    let taskToDelete = $scope.tasks.find(t => t.Id === taskData.Id);
                    $scope.tasks.splice($scope.tasks.indexOf(taskToDelete), 1);
                    break;
                case "ChangeStatus":
                    let taskToChange = $scope.tasks.find(t => t.Id === taskData.Id);
                    if (taskToChange) {
                        taskToChange.Created = taskData.Created;
                        taskToChange.Started = taskData.Started;
                        taskToChange.Completed = taskData.Completed;
                        taskToChange.ExecutionTime = taskData.ExecutionTime;
                        taskToChange.TaskState = taskData.TaskState;
                        taskToChange.TaskStateName = taskData.TaskStateName;

                        $scope.tryRedirectToResult(taskToChange);
                    }
                    else {
                        $scope.tasks.push(data);
                    }
                    break;
                default: console.error("Unknown task event");
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
            alertify.confirm("Confirm action", "Are you sure you want to delete all tasks?",
                () => {
                    $scope.tasks.forEach(t => t.Deleting = true);
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteAllTasks")
                        .then(alertify.success("All tasks have been deleted."));
                }, () => { });
        }

        function deleteTasksWithStatus(taskStatus) {
            alertify.confirm("Confirm action", `Are you sure you want to delete all tasks with "${taskStatus}" status?`,
                () => {
                    $scope.tasks.filter(t => t.taskState === taskStatus).forEach(t => t.Deleting = true);
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteTasksWithState", taskStatus)
                        .then(() => alertify.success(`All tasks with "${taskStatus}" status have been deleted.`));
                }, () => { });
        }

        function deleteTask(id) {
            alertify.confirm("Confirm action", "Are you sure you want to delete this task?",
                () => {
                    let taskToDelete = $scope.tasks.find(t => t.Id === id);
                    taskToDelete.Deleting = true;
                    $scope.$apply();
                    $scope.tasksHub.invoke("deleteTask", id)
                        .then(() => alertify.success("The task has been deleted."))
                }, () => { });
        }

        function tryRedirectToResult(task) {
            if ($scope.autoRedirect &&
                (task.Id === $scope.RedirectTaskId) &&
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

        $scope.tasksHub.start()
            .then(() => $scope.tasksHub.invoke("getAllTasks").then($scope.onHubStart))
            .catch((err) => console.error(err.toString()));

        let location = window.location.href.split("/");
        if (location[location.length - 1] !== "TaskManager")
            $scope.RedirectTaskId = +location[location.length - 1];
        else $scope.RedirectTaskId = null;
        $scope.loadingScreenHeader = "Loading tasks";
        $scope.loading = true;
        $scope.tasks = [];
        $scope.flags = { reconnecting: false };
        $scope.autoRedirect = true;
    }

    angular.module("libiada").controller("TaskManagerCtrl", ["$scope", taskManager]);
}
