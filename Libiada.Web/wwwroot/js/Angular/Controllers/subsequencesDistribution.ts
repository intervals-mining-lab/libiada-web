// Интерфейс для $scope в AngularJS
interface IScope {
    // Стандартные методы Angular $scope
    $apply(exp?: string | Function): any;
    $watch(watchExpression: string | Function,
        listener?: string | Function,
        objectEquality?: boolean): Function;

    // Свойство для доступа к другим свойствам по строковому ключу
    [key: string]: any;
}

// Интерфейс для данных, передаваемых в контроллер
interface ISubsequencesDistributionData {
    // Основные параметры
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    groups: Array<{ id: number; name: string }>;
    sequenceTypes: Array<{ id: number; name: string }>;
    features: Array<{ id: number; name: string }>;

    // Характеристики
    characteristicTypes: Array<{ value: string; text: string }>;
    characteristicsDictionary: { [key: string]: any };
    notations: Array<{ value: number; text: string }>;
    languages: Array<{ value: number; text: string }>;
    translators: Array<{ value: number; text: string }>;
    pauseTreatments: Array<{ value: number; text: string }>;
    trajectories?: Array<{ value: number; text: string }>;
}

// Интерфейс для $scope в контроллере
interface ISubsequencesDistributionScope extends IScope {
    // Базовые параметры из модели данных
    maximumSelectedResearchObjects: number;
    minimumSelectedResearchObjects: number;
    nature: string;
    groups: Array<{ id: number; name: string }>;
    sequenceTypes: Array<{ id: number; name: string }>;
    features: Array<{ id: number; name: string }>;

    // Характеристики
    characteristicTypes: Array<{ value: string; text: string }>;
    characteristicsDictionary: { [key: string]: any };
    notations: Array<{ value: number; text: string }>;
    languages: Array<{ value: number; text: string }>;
    translators: Array<{ value: number; text: string }>;
    pauseTreatments: Array<{ value: number; text: string }>;
    trajectories?: Array<{ value: number; text: string }>;

    // Динамические данные
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;
}

// Объявление глобальных переменных и функций
declare const angular: any;
declare function MapModelFromJson($scope: ISubsequencesDistributionScope, data: ISubsequencesDistributionData): void;

// Основной класс контроллера
class SubsequencesDistributionController {
    constructor(private readonly data: ISubsequencesDistributionData) {
        this.initialize();
    }

    private initialize(): void {
        // Определение функции контроллера
        const subsequencesDistribution = ($scope: ISubsequencesDistributionScope): void => {
            MapModelFromJson($scope, this.data);
        };

        // Регистрация контроллера в Angular модуле
        angular.module("libiada")
            .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}

// Экспорт конструктора для использования в _AngularControllerInitializer.cshtml
(window as any).SubsequencesDistributionController = function (data: ISubsequencesDistributionData): void {
    "use strict";
    new SubsequencesDistributionController(data);
};