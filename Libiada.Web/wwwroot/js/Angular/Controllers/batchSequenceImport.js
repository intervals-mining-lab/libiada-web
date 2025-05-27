/// <reference types="angular" />
/**
 * Контроллер для пакетного импорта последовательностей
 */
class BatchSequenceImportHandler {
    /**
     * Создает новый экземпляр контроллера
     * @param data Данные для инициализации контроллера
     */
    constructor(data) {
        this.initializeController(data);
    }
    /**
     * Инициализирует Angular контроллер
     * @param data Данные для инициализации контроллера
     */
    initializeController(data) {
        "use strict";
        const batchSequenceImport = ($scope) => {
            MapModelFromJson($scope, data);
            function parseIds() {
                let splitted = $scope.accessionsField.split(/[^\w.]/);
                for (let i = 0; i < splitted.length; i++) {
                    if (splitted[i]) {
                        $scope.accessions.push({ value: splitted[i] });
                    }
                }
                $scope.accessionsField = "";
            }
            function deleteId(accession) {
                $scope.accessions.splice($scope.accessions.indexOf(accession), 1);
            }
            $scope.parseIds = parseIds;
            $scope.deleteId = deleteId;
            $scope.accessions = [];
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportCtrl", ["$scope", batchSequenceImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Данные для инициализации контроллера
 * @returns Instance of batch sequence import handler
 */
function BatchSequenceImportController(data) {
    return new BatchSequenceImportHandler(data);
}
//# sourceMappingURL=batchSequenceImport.js.map