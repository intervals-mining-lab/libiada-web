import { MapModelFromJson } from "functions";
// Updated controller class
class CalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const calculation = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            //function filterByNature(): void {
            //    if (!$scope.hideNotation) {
            //        const notation: Notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            //        // if notation is not linked to characteristic 
            //        angular.forEach($scope.characteristics, (characteristic: ICharacteristic) => {
            //            characteristic.notation = notation;
            //        });
            //    }
            //}
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
            //$scope.filterByNature = filterByNature;
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;
            // if notation is not linked to characteristic 
            //$scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            //$scope.language = $scope.languages?.[0];
            //$scope.translator = $scope.translators?.[0];
            //$scope.pauseTreatment = $scope.pauseTreatment ?? $scope.pauseTreatments?.[0];
            $scope.calculationFor = "researchObjects";
            $scope.complementary = false;
            $scope.rotate = false;
            // if we are in clusterization 
            //if ($scope.ClusterizatorsTypes) {
            //    $scope.ClusterizationType = $scope.ClusterizatorsTypes[0];
            //}
        };
        angular.module("libiada").controller("CalculationCtrl", ["$scope", "filterFilter", calculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CalculationController(data) {
    return new CalculationOperator(data);
}
//# sourceMappingURL=calculation.js.map