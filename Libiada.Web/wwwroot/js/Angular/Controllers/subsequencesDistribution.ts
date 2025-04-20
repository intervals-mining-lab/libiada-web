declare var angular: any;

// Объявляем существующую JavaScript функцию
declare function MapModelFromJson($scope: any, data: any): void;

function SubsequencesDistributionController(data) {
    "use strict";

    function subsequencesDistribution($scope) {
        MapModelFromJson($scope, data);
    }

    angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
}
