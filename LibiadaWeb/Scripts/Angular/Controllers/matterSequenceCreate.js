function MatterSequenceCreateController(data) {
    "use strict";

    function matterSequenceCreate($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            var arraysForFiltration = ["notations", "remoteDbs", "matters", "groups", "sequenceTypes"];

            arraysForFiltration.forEach(function (arrayName) {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                }
            });

            $scope.notationId = $scope.notationsFiltered[0].Value;
            $scope.group = $scope.groupsFiltered[0].Value;
            $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
            if (angular.isDefined($scope.mattersFiltered) && angular.isDefined($scope.mattersFiltered[0])) {
                $scope.matterId = $scope.mattersFiltered[0].Value;
            }
        }

        function remoteIdChanged(remoteId) {
            var nameParts = $scope.name.split(" | ");
            if (nameParts.length <= 2) {
                    $scope.name = nameParts[0] + (remoteId ?  " | " + remoteId : "");
            }
        }

        function isRemoteDbDefined() {
            return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
        }

        $scope.filterByNature = filterByNature;
        $scope.isRemoteDbDefined = isRemoteDbDefined;
        $scope.remoteIdChanged = remoteIdChanged;

        $scope.original = false;
        $scope.localFile = false;
        $scope.languageId = $scope.languages[0].Value;
        $scope.translatorId = $scope.translators[0].Value;
        $scope.nature = $scope.natures[0].Value;
        $scope.name = "";
    }

    angular.module("libiada").controller("MatterSequenceCreateCtrl", ["$scope", "filterFilter", matterSequenceCreate]);
}
