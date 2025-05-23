﻿function ResearchObjectSequenceCreateController(data) {
    "use strict";

    function researchObjectSequenceCreate($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            let arraysForFiltration = ["notations", "remoteDbs", "researchObjects", "groups", "sequenceTypes"];

            arraysForFiltration.forEach(arrayName => {
                if (angular.isDefined($scope[arrayName])) {
                    $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                }
            });

            $scope.notationId = $scope.notationsFiltered[0].Value;
            $scope.group = $scope.groupsFiltered[0].Value;
            $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
            if (angular.isDefined($scope.researchObjectsFiltered) && angular.isDefined($scope.researchObjectsFiltered[0])) {
                $scope.researchObjectId = $scope.researchObjectsFiltered[0].Value;
            }
        }

        function remoteIdChanged(remoteId) {
            let nameParts = $scope.name.split(" | ");
            if (nameParts.length <= 2) {
                $scope.name = `${nameParts[0]}${remoteId ? ` | ${remoteId}` : ""}`;
            }
        }

        function isRemoteDbDefined() {
            return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
        }

        $scope.filterByNature = filterByNature;
        $scope.isRemoteDbDefined = isRemoteDbDefined;
        $scope.remoteIdChanged = remoteIdChanged;

        $scope.original = false;
        $scope.languageId = $scope.languages[0].Value;
        $scope.translatorId = $scope.translators[0].Value;
        $scope.nature = $scope.natures[0].Value;
        $scope.name = "";
    }

    angular.module("libiada").controller("ResearchObjectSequenceCreateCtrl", ["$scope", "filterFilter", researchObjectSequenceCreate]);
}
