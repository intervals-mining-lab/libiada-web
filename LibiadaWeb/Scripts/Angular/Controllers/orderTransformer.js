function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope, filterFilter) {
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

        function addTransformation() {
            $scope.transformations.push({
                link: $scope.transformationLinks[0],
                operation: $scope.operations[0]
            });
        }

        function deleteTransformation(transformation) {
            $scope.transformations.splice($scope.transformations.indexOf(transformation), 1);
        }

        function filterByNature() {
            var notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

            angular.forEach($scope.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        }

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0],
                notation: filterFilter($scope.notations, { Nature: $scope.nature })[0],
                language: $scope.languages[0],
                translator: $scope.translators[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
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
        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;
        $scope.filterByNature = filterByNature;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.disableMattersSelect = FakeDisableMattersSelect;
        $scope.addTransformation = addTransformation;
        $scope.deleteTransformation = deleteTransformation;

        $scope.transformations = [];
        $scope.characteristics = [];
        $scope.flags = { showRefSeqOnly: true };
        $scope.selectedMatters = 0;
        $scope.nature = $scope.natures[0].Value;
    }

    angular.module("OrderTransformer", []).controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
}
