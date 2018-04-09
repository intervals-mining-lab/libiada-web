function SequenceGroupsController(data) {
    "use strict";

    function sequenceGroups($scope) {
        MapModelFromJson($scope, data);

        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("libiada").controller("sequenceGroupsCtrl", ["$scope", "$http", sequenceGroups]);
    mattersTable();
}
