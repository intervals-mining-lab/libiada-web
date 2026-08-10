import { MapModelFromJson } from "functions";
// Angular controller class
class CalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const calculation = ($scope) => {
            MapModelFromJson($scope, data);
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
            $scope.setUnselectAllResearchObjectsFunction = setUnselectAllResearchObjectsFunction;
            $scope.setUnselectAllSequenceGroupsFunction = setUnselectAllSequenceGroupsFunction;
            $scope.clearSelection = clearSelection;
            $scope.calculationFor = "researchObjects";
            $scope.complementary = false;
            $scope.rotate = false;
        };
        angular.module("libiada").controller("CalculationCtrl", ["$scope", calculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CalculationController(data) {
    return new CalculationOperator(data);
}
//# sourceMappingURL=calculation.js.map