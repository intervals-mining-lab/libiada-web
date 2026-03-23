import { MapModelFromJson } from "functions";
// Controller class
class CongenericCalculationOperator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const congenericCalculation = ($scope, filterFilter) => {
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
        };
        angular.module("libiada").controller("CongenericCalculationCtrl", ["$scope", "filterFilter", congenericCalculation]);
    }
}
// Wrapper function for backwards compatibility
export default function CongenericCalculationController(data) {
    return new CongenericCalculationOperator(data);
}
//# sourceMappingURL=congenericCalculation.js.map