import { MapModelFromJson } from "functions";
class SequenceCheckResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const sequenceCheckResult = ($scope, $http) => {
            // returns css class for given status
            function calculateStatusClass(status) {
                return status === "Success" ? "text-success" : "text-danger";
            }
            $scope.calculateStatusClass = calculateStatusClass;
            // loading import results from the server
            $scope.loadingScreenHeader = "Loading import results";
            $scope.loading = true;
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
export default function SequenceCheckResultController() {
    return new SequenceCheckResultHandler();
}
//# sourceMappingURL=sequenceCheckResult.js.map