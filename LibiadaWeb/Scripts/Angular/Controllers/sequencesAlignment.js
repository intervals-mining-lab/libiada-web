function SequencesAlignmentController(data) {
    "use strict";

    function sequencesAlignment($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        }

        function disableMattersSelect(matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        }

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        }

        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });
                $scope.newFilter = "";
            }
        }

        function deleteFilter(filter) {
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
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
        $scope.disableMattersSelect = disableMattersSelect;
        $scope.disableSubmit = disableSubmit;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.flags = { showRefSeqOnly: true };
        $scope.selectedMatters = 0;
        $scope.filters = [];
        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };

        $scope.subsequencesCharacteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].CharacteristicLinks[0],
            notation: $scope.notations[0]
        };
    }

    angular.module("libiada", []).controller("SequencesAlignmentCtrl", ["$scope", "filterFilter", sequencesAlignment]);
}
