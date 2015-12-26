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


        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;

        $scope.disableMattersSelect = fakeDisableMattersSelect;

        $scope.link = $scope.links[0];
        $scope.selectedMatters = 0;
    }

    angular.module("OrderTransformer", []).controller("OrderTransformerCtrl", ["$scope", orderTransformer]);
}
