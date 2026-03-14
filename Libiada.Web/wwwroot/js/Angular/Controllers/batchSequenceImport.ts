import { MapModelFromJson } from "functions";

/**
 * Интерфейс для элемента списка идентификаторов последовательностей
 */
interface IAccession {
    value: string;
}

/**
 * Interface for the data object passed from the server
 */
interface IBatchSequenceImportData {

    matters?: { id: number; value: string }[];
    natures?: { id: number; value: string }[];

}

/**
 * Interface for the angular controller's scope
 */
interface IBatchSequenceImportScope extends ng.IScope, IBatchSequenceImportData {

    accessionsField: string;
    accessions: IAccession[];

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
        this.ngOnInit(data);
    }

    /**
     * Инициализирует Angular контроллер
     * @param data Данные для инициализации контроллера
     */
    private ngOnInit(data: IBatchSequenceImportData): void {
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
export default function BatchSequenceImportController(data: IBatchSequenceImportData): BatchSequenceImportHandler {
    return new BatchSequenceImportHandler(data);
}
