function FmotifsDictionaryController() {
    "use strict";

    function fmotifsDictionary($scope) {
        MapModelFromJson($scope, data);

        $scope.notation = $scope.notations[0];
    }

    angular.module("libiada").controller("FmotifsDictionaryCtrl", ["$scope", fmotifsDictionary]);
}