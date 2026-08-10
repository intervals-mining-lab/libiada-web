import { MapModelFromJson } from "functions";

/**
 * Interface for the data object fetched from the server
 */
interface AccordanceCalculationResultData {
    alphabet?: string[];
    calculationType: string;
    characteristicName: string;
    characteristics: number[][];
    researchObjectNames: number[];
    firstAlphabet?: string[];
    secondAlphabet?: string[];
}

/**
 * Interface for the angular controller's scope
 */
interface AccordanceCalculationResultScope extends angular.IScope, AccordanceCalculationResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

/**
 * Controller for accordance functionality
 */
class AccordanceCalculationResultHandler {
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
    private ngOnInit(): void {
        const accordanceCalculationResult = ($scope: AccordanceCalculationResultScope, $http: ng.IHttpService): void => {
            $scope.loadingScreenHeader = "Loading accordance data";

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            $http.get<AccordanceCalculationResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading accordance data");
                    $scope.loading = false;
                });
        };

        // Register controller in Angular module
        angular.module("libiada").controller("AccordanceCalculationResultCtrl", ["$scope", "$http", accordanceCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance calculation result handler
 */
export default function AccordanceCalculationResultController(): AccordanceCalculationResultHandler {
    return new AccordanceCalculationResultHandler();
}
