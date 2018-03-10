function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {

		$scope.tasks = [];
		$scope.loading = true;
        $scope.flags = { reconnecting: false };
        var tasksHub = $.connection.tasksManagerHub;

        tasksHub.client.TaskEvent = function (event, data) {

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
            } catch (e) {}
        };

        $.connection.hub.stateChanged(function (change) {
            if (change.newState === $.signalR.connectionState.connecting) {
                $scope.flags.reconnecting = false;

            }
            if (change.newState === $.signalR.connectionState.reconnecting) {
                $scope.flags.reconnecting = true;

                setInterval(function () {
                    if (change.newState === $.signalR.connectionState.reconnecting){
                        if (confirm('Connection lost. Refresh page?')) {
                            location.reload(true);
                        }
                    }
                }, 30000);
            }
            else if (change.newState === $.signalR.connectionState.connected) {
                $scope.flags.reconnecting = false;
            }
        });


        tasksHub.client.onConnected = function (data) {
            alert(data);
        };

        $.connection.hub.start().done(function (data) {
            tasksHub.server.getAllTasks().done(function (tasksJson) {
                var tasks = JSON.parse(tasksJson);
                for (var i = 0; i < tasks.length; i++) {
                    $scope.tasks.push(tasks[i]);
				}
				$scope.loading = false;
                try {
                    $scope.$apply();
                } catch (e) { }
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
    }

    angular.module("libiada", []).controller("TaskManagerCtrl", ["$scope", taskManager]);
}
