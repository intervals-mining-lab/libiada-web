function GenesImportController(data) {
    "use strict";

    function genesImport($scope) {
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

        $scope.matterCheckChanged = matterCheckChanged;
        $scope.disableSubmit = disableSubmit;

        $scope.disableMattersSelect = FakeDisableMattersSelect;

        $scope.localFile = false;
        $scope.flags = { showRefSeqOnly: true };
        $scope.selectedMatters = 0;
    }

    angular.module("GenesImport", []).controller("GenesImportCtrl", ["$scope", genesImport]);
}
