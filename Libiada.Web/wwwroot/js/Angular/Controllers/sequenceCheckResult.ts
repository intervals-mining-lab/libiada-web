import { MapModelFromJson } from "functions";

interface SequenceCheckResultData {
    dbSequenceName: string;
    fileSequenceName: string;
    message: string;
    status: string;
}

interface SequenceCheckResultScope extends ng.IScope, SequenceCheckResultData {

    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;

    calculateStatusClass: (status: string) => string;
}

class SequenceCheckResultHandler {

    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const sequenceCheckResult = ($scope: SequenceCheckResultScope, $http: ng.IHttpService): void => {
            // returns css class for given status
            function calculateStatusClass(status: string): string {
                return status === "Success" ? "text-success" : "text-danger";
            }

            $scope.calculateStatusClass = calculateStatusClass;

            // loading import results from the server
            $scope.loadingScreenHeader = "Loading import results";
            $scope.loading = true;

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $http.get<SequenceCheckResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading import results");
                    $scope.loading = false;
                });
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SequenceCheckResultCtrl", ["$scope", "$http", sequenceCheckResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequence check result handler
 */
export default function SequenceCheckResultController(): SequenceCheckResultHandler {
    return new SequenceCheckResultHandler();
}
