/// <reference types="angular" />

/**
 * Интерфейс для результата импорта последовательности
 */
interface IBatchImportResult {
    Matter: string;
    Name: string;
    Nature: string;
    Status: string;
    ErrorMessage?: string;
}

/**
 * Интерфейс для данных результата пакетного импорта
 */
interface IBatchSequenceImportResultData {
    Results: IBatchImportResult[];
}

/**
 * Интерфейс для области видимости контроллера
 */
interface IBatchSequenceImportResultScope extends ng.IScope {
    // Данные результата
    Results: IBatchImportResult[];

    // Состояния загрузки
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;

    // Методы
    calculateStatusClass: (status: string) => string;
}

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
    private initializeController(): void {
        "use strict";

        const batchSequenceImportResult = ($scope: IBatchSequenceImportResultScope, $http: ng.IHttpService): void => {
            // returns css class for given status
            function calculateStatusClass(status: string): string {
                return status === "Success" ? "table-success"
                    : status === "Exists" ? "table-info"
                        : status === "Error" ? "table-danger" : "";
            }

            $scope.calculateStatusClass = calculateStatusClass;

            $scope.loadingScreenHeader = "Loading import results";

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            $http.get < IBatchSequenceImportResultData > (`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
function BatchSequenceImportResultController(): BatchSequenceImportResultHandler {
    return new BatchSequenceImportResultHandler();
}
