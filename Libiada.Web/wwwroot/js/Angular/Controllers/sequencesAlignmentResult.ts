import { MapModelFromJson } from "functions";

/**
 * Interface for the data object fetched from the server
 */
interface SequencesAlignmentResultData {
    characteristicName: string;
    cyclicShift: boolean;
    distances: { Value: number }[];
    features: string[];
    firstSequenceName: string;
    secondSequenceName: string;
    optimalRotation: number;
    sort: boolean;
    validationType: string;
}

/**
 * Interface for the angular controller's scope
 */
interface SequencesAlignmentResultScope extends angular.IScope, SequencesAlignmentResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

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
    private ngOnInit(): void {
        const sequencesAlignmentResult = ($scope: SequencesAlignmentResultScope, $http: ng.IHttpService): void => {
            $scope.loadingScreenHeader = "Loading data";

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            $http.get<SequencesAlignmentResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
export default function SequencesAlignmentResultController(): SequencesAlignmentResultHandler {
    return new SequencesAlignmentResultHandler();
}
