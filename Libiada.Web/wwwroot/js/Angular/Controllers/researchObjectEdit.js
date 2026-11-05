import { MapModelFromJson } from "functions";
// Angular controller class
class ResearchObjectEditor {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const researchObjectEdit = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            function filterByNature() {
                const arraysForFiltration = ["groups", "sequenceTypes"];
                if (angular.isDefined($scope.groups)) {
                    $scope.groupsFiltered = filterFilter($scope.groups, { Nature: $scope.nature });
                    $scope.group = $scope.groupsFiltered[0].Value;
                }
                if (angular.isDefined($scope.sequenceTypes)) {
                    $scope.sequenceTypesFiltered = filterFilter($scope.sequenceTypes, { Nature: $scope.nature });
                    $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
                }
            }
            $scope.filterByNature = filterByNature;
        };
        angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
    }
}
// Wrapper function for backwards compatibility
export default function ResearchObjectEditController(data) {
    return new ResearchObjectEditor(data);
}
//# sourceMappingURL=researchObjectEdit.js.map