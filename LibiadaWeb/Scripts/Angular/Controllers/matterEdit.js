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

        $scope.name = $scope.matter.Name;
        $scope.description = $scope.matter.Description;
        $scope.nature = $scope.matter.Nature;
        $scope.group = $scope.matter.Group;
        $scope.sequenceType = $scope.matter.SequenceType;
    }

    angular.module("libiada").controller("MatterEditCtrl", ["$scope", "filterFilter", matterEdit]);
}
