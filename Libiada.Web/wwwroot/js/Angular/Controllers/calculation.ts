/// <reference types="angular" />
/// <reference path="../functions.d.ts" />

// Интерфейс для объекта data, который передается в контроллер
interface ICalculationData {
    // Базовые настройки
    natures: INature[];
    nature?: number;

    // Нотации
    notations: INotation[];
    hideNotation?: boolean;

    // Характеристики
    characteristicTypes: ICharacteristicType[];
    characteristicsDictionary: { [key: string]: string };
    characteristics?: ICharacteristic[];

    // Группы и типы последовательностей
    groups: IGroup[];
    sequenceTypes: ISequenceType[];

    // Дополнительные настройки
    languages?: string[];
    translators?: string[];
    pauseTreatments?: IPauseTreatment[];
    trajectories?: ITrajectory[];

    // Лимиты для выбора объектов исследования
    minimumSelectedResearchObjects?: number;
    maximumSelectedResearchObjects?: number;

    // Опции для кластеризации (если есть)
    ClusterizatorsTypes?: IClusterizatorType[];

    // Другие возможные свойства
    [key: string]: any;
}

// Интерфейс для $scope контроллера
interface ICalculationScope extends ng.IScope {
    // Свойства, связанные с обработкой природы и нотации
    nature: number;
    natures: INature[];
    notation: INotation;
    notations: INotation[];
    hideNotation: boolean;

    // Характеристики
    characteristics: ICharacteristic[];
    characteristicTypes: ICharacteristicType[];
    characteristicsDictionary: { [key: string]: string };

    // Выбор объектов исследования
    calculaionFor: string; // "researchObjects" или "sequenceGroups"
    selectedResearchObjectsCount?: number;
    selectedSequenceGroupsCount?: number;

    // Группы и типы последовательностей
    groups: IGroup[];
    sequenceTypes: ISequenceType[];

    // Дополнительные настройки
    language: string;
    languages: string[];
    translator: string;
    translators: string[];
    pauseTreatment: IPauseTreatment;
    pauseTreatments: IPauseTreatment[];
    trajectories?: ITrajectory[];

    // Свойства для управления вращением и комплементарностью последовательностей
    complementary?: boolean;
    rotate?: boolean;
    rotationLength?: number;

    // Кластеризация (если есть)
    ClusterizatorsTypes?: IClusterizatorType[];
    ClusterizationType?: IClusterizatorType;

    // Методы
    filterByNature: () => void;
    clearSelection: () => void;
    setUnselectAllResearchObjectsFunction: (func: Function) => void;
    setUnselectAllSequenceGroupsFunction: (func: Function) => void;
    unselectAllResearchObjects?: Function;
    unselectAllSequenceGroups?: Function;
}

// Вспомогательные интерфейсы

interface INature {
    id: number;
    name: string;
}

interface INotation {
    id: number;
    name: string;
    Nature: number;
}

interface ICharacteristicType {
    id: number;
    name: string;
    description?: string;
    Links: ILink[];
    ArrangementTypes: IArrangementType[];
}

interface ICharacteristic {
    characteristicType: ICharacteristicType;
    notation: INotation;
    link?: ILink;
    arrangementType?: IArrangementType;
    language?: string;
    translator?: string;
    pauseTreatment?: IPauseTreatment;
    trajectory?: ITrajectory;
}

interface ILink {
    id: number;
    name: string;
}

interface IArrangementType {
    id: number;
    name: string;
}

interface IGroup {
    id: number;
    name: string;
}

interface ISequenceType {
    id: number;
    name: string;
}

interface IPauseTreatment {
    id: number;
    name: string;
}

interface ITrajectory {
    id: number;
    name: string;
}

interface IClusterizatorType {
    id: number;
    name: string;
}

// Обновленный класс контроллера
class CalculationControllerClass {
    private data: ICalculationData;

    constructor(data: ICalculationData) {
        this.data = data;
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const calculation = ($scope: ICalculationScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, this.data);

            function filterByNature(): void {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                    // if notation is not linked to characteristic
                    angular.forEach($scope.characteristics, (characteristic: ICharacteristic) => {
                        characteristic.notation = $scope.notation;
                    });
                }
            }

            function setUnselectAllResearchObjectsFunction(func: Function): void {
                $scope.unselectAllResearchObjects = func;
            }

            function setUnselectAllSequenceGroupsFunction(func: Function): void {
                $scope.unselectAllSequenceGroups = func;
            }

            function clearSelection(): void {
                if ($scope.unselectAllResearchObjects) $scope.unselectAllResearchObjects();
                if ($scope.unselectAllSequenceGroups) $scope.unselectAllSequenceGroups();
            }

            $scope.filterByNature = filterByNature;
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;

            // if notation is not linked to characteristic
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            $scope.language = $scope.languages?.[0];
            $scope.translator = $scope.translators?.[0];
            $scope.pauseTreatment = $scope.pauseTreatment ?? $scope.pauseTreatments?.[0];
            $scope.calculaionFor = "researchObjects";

            // if we are in clusterization
            if ($scope.ClusterizatorsTypes) {
                $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
            }
        };

        angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
    }
}

// Функция-обертка для обратной совместимости
function CalculationController(data: ICalculationData): CalculationControllerClass {
    return new CalculationControllerClass(data);
}

