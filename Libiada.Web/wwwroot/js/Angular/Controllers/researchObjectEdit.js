/// <reference types="angular" />
/// <reference types="functions" />
// Updated controller class
class ResearchObjectEditor {
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    initializeController() {
        "use strict";
        const researchObjectEdit = ($scope, filterFilter) => {
            MapModelFromJson($scope, this.data);
            function filterByNature() {
                const arraysForFiltration = ["groups", "sequenceTypes"];
                arraysForFiltration.forEach((arrayName) => {
                    if (angular.isDefined($scope[arrayName])) {
                        $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                    }
                });
                $scope.group = $scope.groupsFiltered[0].Value;
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
            }
            $scope.filterByNature = filterByNature;
        };
        angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
    }
}
// Wrapper function for backwards compatibility
function ResearchObjectEditController(data) {
    return new ResearchObjectEditor(data);
}
//# sourceMappingURL=researchObjectEdit.js.map