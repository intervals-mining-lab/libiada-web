function BuildingsSimilarityController(data) {
    "use strict";

    function buildingsSimilarity($scope, filterFilter) {
        MapModelFromJson($scope, data);

        function filterByNature() {
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
        };

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        function disableMattersSelect(matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        };

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        $scope.filterByNature = filterByNature;
        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableMattersSelect = disableMattersSelect;
        $scope.disableSubmit = disableSubmit;

        $scope.nature = $scope.natures[0].Value;
        $scope.selectedMatters = 0;
    }

    angular.module("BuildingsSimilarity", []).controller("BuildingsSimilarityCtrl", ["$scope", "filterFilter", buildingsSimilarity]);
}
