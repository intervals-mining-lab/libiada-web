function MatterSequenceCreateController(data) {
    "use strict";

    function matterSequenceCreate($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            var arraysForFiltration = ["notations", "features", "remoteDbs", "matters"];

            arraysForFiltration.forEach(function (arrayName) {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                }
            });

            $scope.notationId = $scope.notationsFiltered[0].Value;
            $scope.featureId = $scope.featuresFiltered[0].Value;
            if (angular.isDefined($scope.mattersFiltered) && angular.isDefined($scope.mattersFiltered[0])) {
                $scope.matterId = $scope.mattersFiltered[0].Value;
            }
        };

        function featureChanged(featureId) {
            var featureName = "";
            for (var i = 0; i < $scope.features.length; i++) {
                if ($scope.features[i].Value === featureId) {
                    featureName = $scope.features[i].Text;
                    break;
                }
            }
            $scope.description = featureName;
        }

        function remoteIdChanged(remoteId) {
            var nameParts = $scope.name.split(" | ");
            if (nameParts.length <= 2) {
                    $scope.name = nameParts[0] + (remoteId ?  " | " + remoteId : "");
            }
        }

        function isRemoteDbDefined() {
            return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
        };

        $scope.filterByNature = filterByNature;
        $scope.isRemoteDbDefined = isRemoteDbDefined;
        $scope.featureChanged = featureChanged;
        $scope.remoteIdChanged = remoteIdChanged;

        $scope.original = false;
        $scope.localFile = false;
        $scope.languageId = $scope.languages[0].Value;
        $scope.nature = $scope.natures[0].Value;
        $scope.name = "";
    }

    angular.module("MatterSequenceCreate", []).controller("MatterSequenceCreateCtrl", ["$scope", "filterFilter", matterSequenceCreate]);
}
