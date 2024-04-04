function CalculationController(data) {
    "use strict";

    function calculation($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            if (!$scope.hideNotation) {
                $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                // if notation is not linked to characteristic
                angular.forEach($scope.characteristics, characteristic => {
                    characteristic.notation = notation;
                });
            }
        }

        function setUnselectAllMattersFunction(func) {
            $scope.unselectAllMatters = func;
        }

        function setUnselectAllSequenceGroupsFunction(func) {
            $scope.unselectAllSequenceGroups = func;
        }

        function clearSelection() {
            if ($scope.unselectAllMatters) $scope.unselectAllMatters();
            if ($scope.unselectAllSequenceGroups) $scope.unselectAllSequenceGroups();
        }

        $scope.filterByNature = filterByNature;
        $scope.setUnselectAllMattersFunction = setUnselectAllMattersFunction;
        $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
        $scope.clearSelection = clearSelection;

        // if notation is not linked to characteristic
        $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        $scope.language = $scope.languages[0];
        $scope.translator = $scope.translators[0];
        $scope.pauseTreatment ??= $scope.pauseTreatments[0];
        $scope.calculaionFor = "matters";

        // if we are in clusterization
        if ($scope.ClusterizatorsTypes) {
            $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
        }
    }

    angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
}
