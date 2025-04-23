// Основной класс контроллера
var SubsequencesDistributionController = /** @class */ (function () {
    function SubsequencesDistributionController(data) {
        this.data = data;
        this.initialize();
    }
    SubsequencesDistributionController.prototype.initialize = function () {
        var _this = this;
        // Определение функции контроллера
        var subsequencesDistribution = function ($scope) {
            MapModelFromJson($scope, _this.data);
        };
        // Регистрация контроллера в Angular модуле
        angular.module("libiada")
            .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    };
    return SubsequencesDistributionController;
}());
// Экспорт конструктора для использования в _AngularControllerInitializer.cshtml
window.SubsequencesDistributionController = function (data) {
    "use strict";
    new SubsequencesDistributionController(data);
};
