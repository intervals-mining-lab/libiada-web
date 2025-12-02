/// <reference types="angular" />

/// <reference types="@signalr/src" />

// Interface for task data
interface ITask {
    Id: number;
    TaskType: string;
    UserId: string;
    Created: string;
    Started: string;
    Completed: string;
    ExecutionTime: string;
    TaskState: TaskState;
    TaskStateName: string;
    resultLink?: URL;
    Deleting?: boolean;
}

// Task state types
type TaskState = "InQueue" | "InProgress" | "Completed" | "Error";

// Interface for the task manager scope
interface ITaskManagerScope extends ng.IScope {
    // Tasks and loading state
    tasks: ITask[];
    loading: boolean;
    loadingScreenHeader: string;

    // SignalR hub connection
    tasksHub: signalR.HubConnection;
    flags: { reconnecting: boolean };

    // Redirection settings
    RedirectTaskId: number | null;
    autoRedirect: boolean;

    // Methods for hub events
    onCloseConnection: () => void;
    onHubStart: (tasks: ITask[]) => void;
    taskEvent: (event: string, taskData: ITask) => void;

    // Helper methods for the UI
    getStatusClass: (status: TaskState) => string;
    getStatusIcon: (status: TaskState) => string;
    getTaskCountWithStatus: (state: TaskState) => number;

    // Operations with tasks 
    deleteAllTasks: () => void;
    deleteTasksWithStatus: (taskStatus: TaskState) => void;
    deleteTask: (id: number) => void;
    tryRedirectToResult: (task: ITask) => void;
}

// Definition for alertify
declare const alertify: {
    success(message: string, timeout?: number): void;
    error(message: string, timeout?: number): void;
    confirm(title: string, message: string, onok: () => void, oncancel: () => void): void;
};

// Controller class for task manager
class TaskManagerControllerHandler {
    constructor() {
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const taskManager = ($scope: ITaskManagerScope): void => {
            function onCloseConnection(): void {
                alertify.error("Connection lost", 5);
            }

            function onHubStart(tasks: ITask[]): void {
                for (let i = 0; i < tasks.length; i++) {
                    let task = tasks[i];
                    task.resultLink = new URL(`${window.location.origin}/${task.TaskType}/Result/${task.Id}`);
                    $scope.tasks.push(task);
                    $scope.tryRedirectToResult(task);
                }

                $scope.loading = false;
                try {
                    $scope.$apply();
                } catch (e) { console.error(e instanceof Error ? e.message : String(e)); }
            }

            function taskEvent(event: string, taskData: ITask): void {
                switch (event) {
                    case "AddTask":
                        $scope.tasks.push(taskData);
                        break;
                    case "DeleteTask":
                        let taskToDelete = $scope.tasks.find(t => t.Id === taskData.Id);
                        if (taskToDelete) {
                            $scope.tasks.splice($scope.tasks.indexOf(taskToDelete), 1);
                        }
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
                            $scope.tasks.push(taskData);
                        }
                        break;
                    default:
                        console.error("Unknown task event");
                        break;
                }
                try {
                    $scope.$apply();
                } catch (e) { console.error(e.toString()); }
            }

            function getStatusClass(status: TaskState): string {
                return status === "InProgress" ? "table-info"
                    : status === "Completed" ? "table-success"
                        : status === "Error" ? "table-danger" : "";
            }

            function getStatusIcon(status: TaskState): string {
                return status === "InProgress" ? "bi-play-circle-fill text-info"
                    : status === "Completed" ? "bi-check-circle-fill text-success" : status === "Error" ? "bi-x-circle-fill text-danger"
                        : status === "InQueue" ? "bi-pause-circle-fill text-muted" : "";
            }

            function getTaskCountWithStatus(state: TaskState): number {
                return $scope.tasks.filter(task => task.TaskState === state).length;
            }

            function deleteAllTasks(): void {
                alertify.confirm("Confirm action", "Are you sure you want to delete all tasks?",
                    () => {
                        $scope.tasks.forEach(t => t.Deleting = true);
                        $scope.$apply();
                        $scope.tasksHub.invoke("deleteAllTasks")
                            .then(() => alertify.success("All tasks have been deleted."))
                            .catch(err => console.error(err));
                    },
                    () => { /* Cancel action */ });
            }

            function deleteTasksWithStatus(taskStatus: TaskState): void {
                alertify.confirm("Confirm action", `Are you sure you want to delete all tasks with "${taskStatus}" status?`,
                    () => {
                        $scope.tasks.filter(t => t.TaskState === taskStatus).forEach(t => t.Deleting = true);
                        $scope.$apply();
                        $scope.tasksHub.invoke("deleteTasksWithState", taskStatus)
                            .then(() => alertify.success(`All tasks with "${taskStatus}" status have been deleted.`))
                            .catch(err => console.error(err));
                    },
                    () => { /* Cancel action */ });
            }

            function deleteTask(id: number): void {
                alertify.confirm("Confirm action", "Are you sure you want to delete this task?",
                    () => {
                        let taskToDelete = $scope.tasks.find(t => t.Id === id);
                        if (taskToDelete) {
                            taskToDelete.Deleting = true;
                            $scope.$apply();
                            $scope.tasksHub.invoke("deleteTask", id)
                                .then(() => alertify.success("The task has been deleted."))
                                .catch(err => console.error(err));
                        }
                    },
                    () => { /* Cancel action */ });
            }

            //TODO Check List
            function tryRedirectToResult(task: ITask): void {
                if ($scope.autoRedirect && (task.Id === $scope.RedirectTaskId)
                    && (task.TaskState === "Completed" || task.TaskState === "Error")) {
                    document.location.href = task.resultLink.href; //`${window.location.origin}/${task.TaskType}/Result/${task.Id}`;
                }
            }

            // Assigning methods to $scope 
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

            // Initialize SignalR connection 
            $scope.tasksHub = new signalR.HubConnectionBuilder().withUrl("/TaskManagerHub").build();

            $scope.tasksHub.on("taskEvent", $scope.taskEvent);
            $scope.tasksHub.onclose($scope.onCloseConnection);

            $scope.tasksHub.start()
                .then(() => $scope.tasksHub.invoke("getAllTasks").then($scope.onHubStart))
                .catch((err) => console.error(err.toString()));

            // Initialize scope properties 
            let location = window.location.href.split("/");
            if (location[location.length - 1] !== "TaskManager") {
                $scope.RedirectTaskId = parseInt(location[location.length - 1]);
            } else {
                $scope.RedirectTaskId = null;
            }

            $scope.loadingScreenHeader = "Loading tasks";
            $scope.loading = true;
            $scope.tasks = [];
            $scope.flags = { reconnecting: false };
            $scope.autoRedirect = true;
        };

        angular.module("libiada").controller("TaskManagerCtrl", ["$scope", taskManager]);
    }
}

// Wrapper function for backward compatibility
function TaskManagerController(): TaskManagerControllerHandler {
    return new TaskManagerControllerHandler();
}