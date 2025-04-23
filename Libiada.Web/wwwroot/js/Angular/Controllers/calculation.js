"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
//declare var angular: any;
//import * as angular from "../lib/angular.js/angular.js";
var angular_1 = require("angular");
var CalculationControllerClass = /** @class */ (function () {
    function CalculationControllerClass(data) {
        this.data = data;
        this.initializeController();
    }
    CalculationControllerClass.prototype.initializeController = function () {
        "use strict";
        var _this = this;
        var calculation = function ($scope, filterFilter) {
            var _a, _b, _c, _d;
            MapModelFromJson($scope, _this.data);
            function filterByNature() {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
                    // if notation is not linked to characteristic
                    angular_1.default.forEach($scope.characteristics, function (characteristic) {
                        characteristic.notation = $scope.notation;
                    });
                }
            }
            function setUnselectAllResearchObjectsFunction(func) {
                $scope.unselectAllResearchObjects = func;
            }
            function setUnselectAllSequenceGroupsFunction(func) {
                $scope.unselectAllSequenceGroups = func;
            }
            function clearSelection() {
                if ($scope.unselectAllResearchObjects)
                    $scope.unselectAllResearchObjects();
                if ($scope.unselectAllSequenceGroups)
                    $scope.unselectAllSequenceGroups();
            }
            $scope.filterByNature = filterByNature;
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;
            // if notation is not linked to characteristic
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            $scope.language = (_a = $scope.languages) === null || _a === void 0 ? void 0 : _a[0];
            $scope.translator = (_b = $scope.translators) === null || _b === void 0 ? void 0 : _b[0];
            $scope.pauseTreatment = (_c = $scope.pauseTreatment) !== null && _c !== void 0 ? _c : (_d = $scope.pauseTreatments) === null || _d === void 0 ? void 0 : _d[0];
            $scope.calculaionFor = "researchObjects";
            // if we are in clusterization
            if ($scope.ClusterizatorsTypes) {
                $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
            }
        };
        angular_1.default.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
    };
    return CalculationControllerClass;
}());
// Функция-обертка для обратной совместимости
function CalculationController(data) {
    return new CalculationControllerClass(data);
}
