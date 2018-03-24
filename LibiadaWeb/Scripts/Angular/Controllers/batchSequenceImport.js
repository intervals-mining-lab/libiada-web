function BatchSequenceImportController(data) {
    "use strict";

    function batchSequenceImport($scope) {
        MapModelFromJson($scope, data);

        function parseIds() {
            var splitted = $scope.accessionsField.split(/[^\w.]/);
            for (var i = 0; i < splitted.length; i++) {
                if (splitted[i]) {
                    $scope.accessions.push({ value: splitted[i] });
                }
            }
            $scope.accessionsField = "";
        }

        function deleteId(accession) {
            $scope.accessions.splice($scope.accessions.indexOf(accession), 1);
        }

        $scope.parseIds = parseIds;
        $scope.deleteId = deleteId;

        $scope.accessions = [];
    }

    angular.module("libiada").controller("BatchSequenceImportCtrl", ["$scope", batchSequenceImport]);
}
