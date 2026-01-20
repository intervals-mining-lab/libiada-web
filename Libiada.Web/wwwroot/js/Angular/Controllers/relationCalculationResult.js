import { MapModelFromJson } from "functions";
/**
 * Controller for relation calculation result view
 */
class RelationCalculationResultHandler {
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
        const relationCalculationResult = ($scope, $http) => {
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
        angular.module("libiada").controller("RelationCalculationResultCtrl", ["$scope", "$http", relationCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of relation calculation result handler
 */
export default function AccordanceResultController() {
    return new RelationCalculationResultHandler();
}
//# sourceMappingURL=relationCalculationResult.js.map