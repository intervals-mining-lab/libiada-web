function MatterEditController(data) {
    "use strict";

    function matterEdit($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            var arraysForFiltration = ["groups", "sequenceTypes"];

            arraysForFiltration.forEach(arrayName => {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                }
            });

            $scope.group = $scope.groupsFiltered[0].Value;
            $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
        }

        $scope.filterByNature = filterByNature;
    }

    angular.module("libiada").controller("MatterEditCtrl", ["$scope", "filterFilter", matterEdit]);
}
