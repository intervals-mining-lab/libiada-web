/// <reference types="angular" />

/**
 * Интерфейс для элемента списка идентификаторов последовательностей
 */
interface IAccession {
    value: string;
}

/**
 * Интерфейс для начальных данных контроллера
 */
interface IBatchSequenceImportData {
    // Свойства передаваемых данных
    // (при необходимости можно расширить в соответствии с реальными данными)
    matters?: { id: number; value: string }[];
    natures?: { id: number; value: string }[];
    // другие возможные свойства...
}

/**
 * Интерфейс для области видимости контроллера
 */
interface IBatchSequenceImportScope extends ng.IScope {
    // Входное поле и список идентификаторов
    accessionsField: string;
    accessions: IAccession[];

    // Возможные свойства, передаваемые через data
    matters?: { id: number; value: string }[];
    natures?: { id: number; value: string }[];

    // Методы
    parseIds: () => void;
    deleteId: (accession: IAccession) => void;
}

/**
 * Контроллер для пакетного импорта последовательностей
 */
class BatchSequenceImportHandler {
    /**
     * Создает новый экземпляр контроллера
     * @param data Данные для инициализации контроллера
     */
    constructor(data: IBatchSequenceImportData) {
        this.initializeController(data);
    }

    /**
     * Инициализирует Angular контроллер
     * @param data Данные для инициализации контроллера
     */
    private initializeController(data: IBatchSequenceImportData): void {
        "use strict";

        const batchSequenceImport = ($scope: IBatchSequenceImportScope): void => {
            MapModelFromJson($scope, data);

            function parseIds(): void {
                let splitted = $scope.accessionsField.split(/[^\w.]/);
                for (let i = 0; i < splitted.length; i++) {
                    if (splitted[i]) {
                        $scope.accessions.push({ value: splitted[i] });
                    }
                }
                $scope.accessionsField = "";
            }

            function deleteId(accession: IAccession): void {
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
function BatchSequenceImportController(data: IBatchSequenceImportData): BatchSequenceImportHandler {
    return new BatchSequenceImportHandler(data);
}
