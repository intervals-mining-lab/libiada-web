function SequencesAlignmentController(data) {
    "use strict";

    function sequencesAlignment($scope) {
        MapModelFromJson($scope, data);

        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });
                $scope.newFilter = "";
            }
        }

        function deleteFilter(filter) {
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
        }

        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;

        $scope.setCheckBoxesState = SetCheckBoxesState;
        $scope.filterByFeature = FakeFilterByFeature;

        $scope.filters = [];
    }

    angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
}
