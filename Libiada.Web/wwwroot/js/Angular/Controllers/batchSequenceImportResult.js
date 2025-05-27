/// <reference types="angular" />
/**
 * Контроллер для отображения результатов пакетного импорта последовательностей
 */
class BatchSequenceImportResultHandler {
    /**
     * Создает новый экземпляр контроллера
     */
    constructor() {
        this.initializeController();
    }
    /**
     * Инициализирует Angular контроллер
     */
    initializeController() {
        "use strict";
        const batchSequenceImportResult = ($scope, $http) => {
            // returns css class for given status
            function calculateStatusClass(status) {
                return status === "Success" ? "table-success"
                    : status === "Exists" ? "table-info"
                        : status === "Error" ? "table-danger" : "";
            }
            $scope.calculateStatusClass = calculateStatusClass;
            $scope.loadingScreenHeader = "Loading import results";
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            $scope.loading = true;
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading import results");
                $scope.loading = false;
            });
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportResultCtrl", ["$scope", "$http", batchSequenceImportResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
function BatchSequenceImportResultController() {
    return new BatchSequenceImportResultHandler();
}
//# sourceMappingURL=batchSequenceImportResult.js.map