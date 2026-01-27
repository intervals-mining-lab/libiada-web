import { MapModelFromJson } from "functions";
import { SequenceImportResult } from "viewDataTypes";

interface BatchSequenceImportResultData {
    Results: SequenceImportResult[];
}

interface BatchSequenceImportResultScope extends ng.IScope, BatchSequenceImportResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;

    calculateStatusClass: (status: string) => string;
}

class BatchSequenceImportResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const batchSequenceImportResult = ($scope: BatchSequenceImportResultScope, $http: ng.IHttpService): void => {
            // returns css class for given status
            function calculateStatusClass(status: string): string {
                return status === "Success" ? "table-success"
                     : status === "Exists" ? "table-info"
                     : status === "Error" ? "table-danger" : "";
            }

            $scope.calculateStatusClass = calculateStatusClass;

            // loading import results from the server
            $scope.loadingScreenHeader = "Loading import results";
            $scope.loading = true;

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $http.get<BatchSequenceImportResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data: { data: BatchSequenceImportResultData }) {
                    MapModelFromJson($scope, data.data);
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading import results");
                    $scope.loading = false;
                });
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportResultCtrl", ["$scope", "$http", batchSequenceImportResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import result handler
 */
export default function BatchSequenceImportResultController(): BatchSequenceImportResultHandler {
    return new BatchSequenceImportResultHandler();
}
