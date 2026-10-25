/**
 * Angular controller class
 */
class BatchSequenceImportHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const batchSequenceImport = ($scope) => {
            $scope.parseIds = function () {
                let splitted = $scope.accessionsField.split(/[^\w.]/);
                for (let i = 0; i < splitted.length; i++) {
                    if (splitted[i]) {
                        $scope.accessions.push({ value: splitted[i] });
                    }
                }
                $scope.accessionsField = "";
            };
            $scope.deleteId = function (accession) {
                $scope.accessions.splice($scope.accessions.indexOf(accession), 1);
            };
            $scope.accessions = [];
        };
        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportCtrl", ["$scope", batchSequenceImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import handler
 */
export default function BatchSequenceImportController() {
    return new BatchSequenceImportHandler();
}
//# sourceMappingURL=batchSequenceImport.js.map