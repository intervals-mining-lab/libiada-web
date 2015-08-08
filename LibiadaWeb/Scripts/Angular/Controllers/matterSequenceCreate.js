function matterSequenceCreateController(data) {
    "use strict";

    function matterSequenceCreate($scope, filterFilter) {

        MapModelFromJson($scope, data);

        $scope.original = false;
        $scope.localFile = false;
        $scope.languageId = $scope.languages[0].Value;
        $scope.natureId = $scope.natures[0].Value;

        $scope.filterByNature = function () {
            var arraysForFiltration = ["notations", "features", "remoteDbs", "matters"];

            arraysForFiltration.forEach(function (arrayName) {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.natureId });
                }
            });

            $scope.notationId = $scope.notationsFiltered[0].Value;
            $scope.featureId = $scope.featuresFiltered[0].Value;
            if (angular.isDefined($scope.mattersFiltered) && angular.isDefined($scope.mattersFiltered[0])) {
                $scope.matterId = $scope.mattersFiltered[0].Value;
            }
        };

        $scope.isRemoteDbDefined = function () {
            return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
        };
    }

    angular.module("MatterSequenceCreate", []).controller("MatterSequenceCreateCtrl", ["$scope", "filterFilter", matterSequenceCreate]);
}
