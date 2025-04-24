/// <reference types="angular" />
/// <reference path="../functions.d.ts" />
// Updated controller class
class ResearchObjectEditControllerClass {
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    initializeController() {
        "use strict";
        const researchObjectEdit = ($scope, filterFilter) => {
            MapModelFromJson($scope, this.data);
            console.log('$scope after initialization:', $scope);
            console.log('\n');
            function filterByNature() {
                const arraysForFiltration = ["groups", "sequenceTypes"];
                arraysForFiltration.forEach((arrayName) => {
                    if (angular.isDefined($scope[arrayName])) {
                        $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                        console.log(`${arrayName}Filtered:`, $scope[`${arrayName}Filtered`]);
                        console.log('\n');
                    }
                });
                $scope.group = $scope.groupsFiltered[0].Value;
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
                console.log('Selected group:', $scope.group);
                console.log('Selected sequenceType:', $scope.sequenceType);
                console.log('\n');
                console.log('$scope after initialization:', $scope);
                console.log('\n');
            }
            $scope.filterByNature = filterByNature;
        };
        angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
    }
}
// Wrapper function for backwards compatibility
function ResearchObjectEditController(data) {
    return new ResearchObjectEditControllerClass(data);
}
//# sourceMappingURL=researchObjectEdit.js.map