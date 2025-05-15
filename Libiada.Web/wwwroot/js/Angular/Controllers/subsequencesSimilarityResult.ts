/// <reference types="angular" />

/**
 * Interface for SubsequenceData
 */
interface ISubsequenceData {
    ID: number;
    Position: number;
    Length: number;
    FeatureId: number;
    Attributes: number[];
    Partial: boolean;
    DnaSequence: string;
    AminoAcidSequence: string;
}

/**
 * Interface for Similarity data
 */
interface ISimilarityData {
    ElementsIds: number[];
    Similarity: number[];
    SimilarityPercent: number[];
    Matrix: number[][];
}

/**
 * Interface for the controller scope
 */
interface ISubsequencesSimilarityResultScope extends ng.IScope {
    // Data properties
    loading: boolean;
    loadingScreenHeader: string;
    taskId: string;
    index: number;

    // Sequence and similarity data
    sequences: string[];
    subsequencesData: ISubsequenceData[][];
    similarities: ISimilarityData;
    attributes: string[];
    attributeValues: { attribute: number, value: string }[];
    features: { Text: string, Value: number }[];

    // Methods
    getAttributesText: (attributes: number[]) => any[];
    showPosition: (index: number) => void;
}

/**
 * Controller for subsequences similarity result visualization
 */
class SubsequencesSimilarityResultHandler {
    constructor() {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        "use strict";

        const subsequencesSimilarityResult = ($scope: ISubsequencesSimilarityResultScope, $http: ng.IHttpService, $sce: ng.ISCEService): void => {
            // Gets attributes text for given subsequence
            $scope.getAttributesText = (attributes: number[]): any[] => {
                const attributesText: any[] = [];
                for (let i = 0; i < attributes.length; i++) {
                    const attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($sce.trustAsHtml($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`)));
                }
                return attributesText;
            };

            // Shows the position
            $scope.showPosition = (index: number): void => {
                $scope.index = index;
            };

            // Get task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Initialize loading state
            $scope.loadingScreenHeader = "Loading subsequences similarity";
            $scope.loading = true;

            // Load data from server
            $http.get < any > (`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);
                    $scope.index = 0;
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading subsequences similarity");
                    $scope.loading = false;
                });
        };

        // Register controller with Angular
        angular.module("libiada").controller("SubsequencesSimilarityResultCtrl",
            ["$scope", "$http", "$sce", subsequencesSimilarityResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
function SubsequencesSimilarityResultController(): SubsequencesSimilarityResultHandler {
    return new SubsequencesSimilarityResultHandler();
}
