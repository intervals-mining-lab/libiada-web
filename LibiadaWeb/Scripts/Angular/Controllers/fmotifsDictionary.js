function FmotifsDictionaryController(data) {
    "use strict";

    function fmotifsDictionary($scope) {
        MapModelFromJson($scope, data);

    }

    angular.module("libiada").controller("FmotifsDictionaryCtrl", ["$scope", fmotifsDictionary]);
}