"use strict";

var app = angular.module('MatterSequenceCreate', []);

app.controller('MatterSequenceCreateCtrl', ['$scope', 'filterFilter', function ($scope, filterFilter) {

    MapModelFromJson($scope, data);

    $scope.original = false;
    $scope.localFile = false;
    $scope.languageId = $scope.languages[0].Value;
    $scope.natureId = $scope.natures[0].Value;

    var filterByNature = function() {
        var arraysForFiltration = ["notations", "pieceTypes", "remoteDbs", "matters"];

        arraysForFiltration.forEach(function(array) {
            FilterOptionsByNature($scope, filterFilter, array);
        });

        $scope.notationId = $scope.notationsFiltered[0].Value;
        $scope.pieceTypeId = $scope.pieceTypesFiltered[0].Value;
        if (angular.isDefined($scope.mattersFiltered)) {
            $scope.matterId = $scope.mattersFiltered[0].Value;
        }
            
    };

    $scope.$watch('natureId', filterByNature, true);

    $scope.isRemoteDbDefined = function() {
        return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
    };
}
]);