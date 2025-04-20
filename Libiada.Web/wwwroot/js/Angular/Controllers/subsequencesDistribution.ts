// Объявление типов
interface IScope {
    [key: string]: any;
}

// Объявление глобальных переменных и функций
declare const angular: any;
declare function MapModelFromJson($scope: any, data: any): void;

// Основной класс контроллера
class SubsequencesDistributionController {
    constructor(private readonly data: any) {
        this.initialize();
    }

    private initialize(): void {
        // Определение функции контроллера
        const subsequencesDistribution = ($scope: IScope): void => {
            MapModelFromJson($scope, this.data);
        };

        // Регистрация контроллера в Angular модуле
        angular.module("libiada")
               .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}

// Экспорт конструктора для использования в _AngularControllerInitializer.cshtml
(window as any).SubsequencesDistributionController = function(data: any): void {
    "use strict";
    new SubsequencesDistributionController(data);
};
