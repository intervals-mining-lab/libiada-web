function OrderTransformerController(data) {
    "use strict";

    function orderTransformer($scope) {
        MapModelFromJson($scope, data);

        function matterCheckChanged(matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        function disableSubmit() {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        function addTransformation() {
            $scope.transformations.push({
                link: $scope.links[0],
                operation: $scope.operations[0]
            });
        };

        function deleteTransformation(transformation) {
            $scope.transformations.splice($scope.transformations.indexOf(transformation), 1);
        };

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;

        $scope.disableMattersSelect = FakeDisableMattersSelect;
        $scope.addTransformation = addTransformation;
        $scope.deleteTransformation = deleteTransformation;

        $scope.transformations = [];
        $scope.selectedMatters = 0;
    }

    angular.module("OrderTransformer", []).controller("OrderTransformerCtrl", ["$scope", orderTransformer]);
}
