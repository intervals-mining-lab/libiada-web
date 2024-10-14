function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
}
