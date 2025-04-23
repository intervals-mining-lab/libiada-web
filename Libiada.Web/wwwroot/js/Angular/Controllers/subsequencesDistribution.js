// Объявление глобальных переменных и функций
/// <reference types="angular" />
// Теперь angular будет типизированным как IAngularStatic
/// <reference path="../functions.d.ts" />
// Основной класс контроллера
class SubsequencesDistributionControllerClass {
    constructor(data) {
        this.data = data;
        this.initialize();
    }
    initialize() {
        // Определение функции контроллера
        const subsequencesDistribution = ($scope) => {
            MapModelFromJson($scope, this.data);
        };
        // Регистрация контроллера в Angular модуле
        angular.module("libiada")
            .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}
// Экспорт конструктора для использования в _AngularControllerInitializer.cshtml
function SubsequencesDistributionController(data) {
    return new SubsequencesDistributionControllerClass(data);
}
;
//# sourceMappingURL=subsequencesDistribution.js.map