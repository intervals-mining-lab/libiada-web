// Основной класс контроллера
class SubsequencesDistributionController {
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
window.SubsequencesDistributionController = function (data) {
    "use strict";
    new SubsequencesDistributionController(data);
};
//# sourceMappingURL=subsequencesDistribution.js.map