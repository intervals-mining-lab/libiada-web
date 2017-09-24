function GenesImportController(data) {
    "use strict";

    function genesImport($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        }

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        }

        function getVisibleMatters() {
            var visibleMatters = $scope.matters;
            visibleMatters = filterFilter(visibleMatters, { Text: $scope.searchMatter });
            visibleMatters = filterFilter(visibleMatters, { Description: $scope.searchDescription });
            visibleMatters = filterFilter(visibleMatters, { Group: $scope.group || "" });
            visibleMatters = filterFilter(visibleMatters, { SequenceType: $scope.sequenceType || "" });
            visibleMatters = filterFilter(visibleMatters, function (value) {
                return !$scope.flags.showRefSeqOnly || $scope.nature !== "1" || value.Text.split("|").slice(-1)[0].indexOf("_") !== -1;
            });

            return visibleMatters;
        }

        function selectAllVisibleMatters() {
            getVisibleMatters().forEach(function (matter) {
                matter.Selected = true;
            });
        }

        function unselectAllVisibleMatters() {
            getVisibleMatters().forEach(function (matter) {
                matter.Selected = false;
            });
        }

        $scope.getVisibleMatters = getVisibleMatters;
        $scope.selectAllVisibleMatters = selectAllVisibleMatters;
        $scope.unselectAllVisibleMatters = unselectAllVisibleMatters;
        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;

        $scope.disableMattersSelect = FakeDisableMattersSelect;

        $scope.localFile = false;
        $scope.flags = { showRefSeqOnly: true };
        $scope.selectedMatters = 0;
    }

    angular.module("libiada", []).controller("GenesImportCtrl", ["$scope", "filterFilter", genesImport]);
}
