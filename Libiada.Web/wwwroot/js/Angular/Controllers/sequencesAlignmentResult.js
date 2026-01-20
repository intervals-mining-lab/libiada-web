import { MapModelFromJson } from "functions";
/**
 * Controller for sequences alignment result view
 */
class SequencesAlignmentResultHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     */
    ngOnInit() {
        const sequencesAlignmentResult = ($scope, $http) => {
            $scope.loadingScreenHeader = "Loading data";
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            $scope.loading = true;
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
        angular.module("libiada").controller("SequencesAlignmentResultCtrl", ["$scope", "$http", sequencesAlignmentResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of sequences alignment result handler
 */
export default function SequencesAlignmentResultController() {
    return new SequencesAlignmentResultHandler();
}
//# sourceMappingURL=sequencesAlignmentResult.js.map