/// <reference types="angular" />
/// <reference path="../functions.d.ts" />
// Updated controller class
class CalculationOperator {
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    initializeController() {
        "use strict";
        const calculation = ($scope, filterFilter) => {
            MapModelFromJson($scope, this.data);
            function filterByNature() {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
                    // if notation is not linked to characteristic 
                    angular.forEach($scope.characteristics, (characteristic) => {
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
// Wrapper function for backwards compatibility
function CalculationController(data) {
    return new CalculationOperator(data);
}
//# sourceMappingURL=calculation.js.map