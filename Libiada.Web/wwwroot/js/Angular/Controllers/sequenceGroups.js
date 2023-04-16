function SequenceGroupsController(data) {
    "use strict";

    function sequenceGroups($scope) {
        MapModelFromJson($scope, data);

        function filterByNature() {
        }

        $scope.filterByNature = filterByNature;

        $scope.nature = $scope.natures[0].Value;
     //   $scope.sequenceGroupType = "";
    }

    angular.module("libiada").controller("sequenceGroupsCtrl", ["$scope", sequenceGroups]);
}
