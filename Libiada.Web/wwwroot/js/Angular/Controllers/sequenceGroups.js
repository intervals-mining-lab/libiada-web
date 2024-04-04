function SequenceGroupsController(data) {
    "use strict";

    function sequenceGroups($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("sequenceGroupsCtrl", ["$scope", sequenceGroups]);
}
