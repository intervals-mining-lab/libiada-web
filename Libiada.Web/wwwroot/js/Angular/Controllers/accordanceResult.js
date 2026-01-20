import { MapModelFromJson } from "functions";
/**
 * Controller for accordance functionality
 */
class AccordanceResultHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor() {
        this.ngOnInit();
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit() {
        const accordanceResult = ($scope, $http) => {
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
        angular.module("libiada").controller("AccordanceResultCtrl", ["$scope", "$http", accordanceResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance handler
 */
export default function AccordanceResultController() {
    return new AccordanceResultHandler();
}
//# sourceMappingURL=accordanceResult.js.map