/// <reference types="angular" />

// Interface for SubsequenceData
interface ISubsequenceData {
    Attributes: number[],
    CharacteristicsValues: number[],
    FeatureId: number,
    Id: number,
    Lengths: number[],
    Starts: number[],
    Partial: boolean,
    RemoteId: string,
}

// Interface for data passed to the controller from server on page load
interface SubsequencesSimilarityResultData {
    firstSequenceName: string;
    secondSequenceName: string;
    characteristicName: string;
    similarity: number;
    firstSequenceSimilarity: number;
    secondSequenceSimilarity: number;
    similarSubsequences: { Item1: number, Item2: number }[];
    firstSequenceSubsequences: ISubsequenceData[];
    secondSequenceSubsequences: ISubsequenceData[];
    features: { [name: number]: string };
    attributes: { [name: number]: string };
    firstSequenceAttributes: { AttributeId: number, Value: string }[][];
    secondSequenceAttributes: { AttributeId: number, Value: string }[][];
}

// Interface for the controller scope
interface SubsequencesSimilarityResultScope extends ng.IScope, SubsequencesSimilarityResultData {
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
}

// Controller for subsequences similarity result visualization
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.ngOnInit();
    }

    // Initializes the Angular controller
    private ngOnInit(): void {
        const subsequencesSimilarityResult = ($scope: SubsequencesSimilarityResultScope, $http: ng.IHttpService): void => {

            // Get task ID from URL
            const location: string[] = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Initialize loading state
            $scope.loadingScreenHeader = "Loading subsequences data";
            $scope.loading = true;

            // Load data from server
            $http.get<SubsequencesSimilarityResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data: ng.IHttpResponse<SubsequencesSimilarityResultData>) {
                    MapModelFromJson($scope, data.data);
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading subsequences data");
                    $scope.loading = false;
                });
        };

        // Register controller with Angular
        angular.module("libiada").controller("SubsequencesSimilarityResultCtrl", ["$scope", "$http", subsequencesSimilarityResult]);
    }
}

// Wrapper function for backward compatibility
function SubsequencesSimilarityResultController(): SubsequencesSimilarityResultHandler {
    return new SubsequencesSimilarityResultHandler();
}
