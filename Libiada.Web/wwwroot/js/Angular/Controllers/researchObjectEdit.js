function ResearchObjectEditController(data) {
    "use strict";

    function researchObjectEdit($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            let arraysForFiltration = ["groups", "sequenceTypes"];

            arraysForFiltration.forEach(arrayName => {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                }
            });

            $scope.group = $scope.groupsFiltered[0].Value;
            $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
        }

        $scope.filterByNature = filterByNature;
    }

    angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
}
