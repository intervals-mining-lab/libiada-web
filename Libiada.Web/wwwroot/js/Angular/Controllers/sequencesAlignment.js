function SequencesAlignmentController(data) {
    "use strict";

    function sequencesAlignment($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
}
