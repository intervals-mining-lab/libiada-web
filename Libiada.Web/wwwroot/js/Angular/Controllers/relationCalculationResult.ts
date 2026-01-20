import { MapModelFromJson } from "functions";

/**
 * Interface for accordion data
 */
interface RelationCalculationResultData {
    characteristicName: string;
    characteristics?: number[][];
    elements?: { Id: number, Name: string }[];
    isFilter: boolean;
    notationName: string;
    researchObjectName: string;
    filterSize?: number;
    filteredResult?: { FirstElementId: number, SecondElementId: number, Value: number }[][];
    firstElements?: string[];
    secondElements?: string[];
}
/**
 * Interface for controller scope
 */
interface RelationCalculationResultScope extends angular.IScope, RelationCalculationResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
}

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
    private ngOnInit(): void {
        const relationCalculationResult = ($scope: RelationCalculationResultScope, $http: ng.IHttpService): void => {
            $scope.loadingScreenHeader = "Loading data";

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            $http.get<RelationCalculationResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
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
export default function AccordanceResultController(): RelationCalculationResultHandler {
    return new RelationCalculationResultHandler();
}
