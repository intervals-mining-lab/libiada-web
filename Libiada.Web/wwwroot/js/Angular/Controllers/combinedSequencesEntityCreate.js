import { MapModelFromJson } from "functions";
/**
 * Angular controller class for combined sequences entity creation view
 */
class CombinedSequencesEntityCreator {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const combinedSequencesEntityCreate = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            function filterByNatureIfExists(array) {
                if (angular.isDefined(array)) {
                    return filterFilter(array, { Nature: $scope.nature });
                }
            }
            function filterByNature() {
                $scope.notationsFiltered = $scope.filterByNatureIfExists($scope.notations) ?? [];
                $scope.notation = $scope.notationsFiltered[0].Value;
                $scope.groupsFiltered = $scope.filterByNatureIfExists($scope.groups) ?? [];
                $scope.group = $scope.groupsFiltered[0].Value;
                $scope.sequenceTypesFiltered = $scope.filterByNatureIfExists($scope.sequenceTypes) ?? [];
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
                $scope.remoteDbsFiltered = $scope.filterByNatureIfExists($scope.remoteDbs) ?? [];
                $scope.researchObjectsFiltered = $scope.filterByNatureIfExists($scope.researchObjects) ?? [];
                if (angular.isDefined($scope.researchObjectsFiltered) &&
                    angular.isDefined($scope.researchObjectsFiltered[0])) {
                    $scope.researchObjectId = $scope.researchObjectsFiltered[0].Value;
                }
            }
            function remoteIdChanged(remoteId) {
                const nameParts = $scope.name.split(" | ");
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
            $scope.filterByNatureIfExists = filterByNatureIfExists;
            $scope.original = false;
            $scope.languageId = $scope.languages[0].Value;
            $scope.translatorId = $scope.translators[0].Value;
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
        };
        angular.module("libiada").controller("CombinedSequencesEntityCreateCtrl", ["$scope", "filterFilter", combinedSequencesEntityCreate]);
    }
}
// Wrapper function for backwards compatibility
export default function CombinedSequencesEntityCreateController(data) {
    return new CombinedSequencesEntityCreator(data);
}
//# sourceMappingURL=combinedSequencesEntityCreate.js.map