function TaskManagerController(data) {
    "use strict";

    function taskManager($scope) {
        MapModelFromJson($scope, data);

        var chat = $.connection.tasksManagerHub;

        chat.client.TaskEvent = function (data) {
            alert(data);
        };


        chat.client.onConnected = function (data) {
            alert(data);
        }

        $.connection.hub.start().done(function (data) {
            //alert(data);
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

        setInterval($scope.autoRefresh, 1 * 60 * 1000);
    }

    angular.module("TaskManager", []).controller("TaskManagerCtrl", ["$scope", taskManager]);
}
