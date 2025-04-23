//declare var angular: any;
//import angular from 'angular';
/// <reference types="angular" />
/// <reference path="../functions.d.ts" />


// Объявляем существующую JavaScript функцию
//declare function MapModelFromJson($scope: any, data: any): void;

class CalculationControllerClass {
    private data: any;

    constructor(data: any) {
        this.data = data;
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const calculation = ($scope: any, filterFilter: any): void => {
            MapModelFromJson($scope, this.data);

            function filterByNature(): void {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                    // if notation is not linked to characteristic
                    angular.forEach($scope.characteristics, (characteristic: any) => {
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
function CalculationController(data: any) {
    return new CalculationControllerClass(data);
}
