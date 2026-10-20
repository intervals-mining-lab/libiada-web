import {
    InitializeBootstrapTooltips,
    initScopeFromServer
} from "functions";
import type {
    Aggregator,
    Aligner,
    Characteristic,
    DistanceCalculator,
    ResultsLoadingScreenData,
    LocalCharacteristicsData
} from "viewDataTypes";

/// <reference types="jquery" />

/**
 * Interface for the data object fetched from the server
 */
interface LocalCalculationResultData {
    aggregators: Aggregator[];
    aligners: Aligner[];
    characteristicNames: string[];
    characteristics: LocalCharacteristicsData[];
    characteristicsList: Characteristic[];
    distanceCalculators: DistanceCalculator[];
    lengths: number[][];
    notationName: string;
    starts: number[][];
}

/**
 * Interface for similarity matrix response
 */
interface LocalCharacteristicsSimilarityMatrixResponse {
    result: number[][];
    aligner: number;
    distanceCalculator: number;
    aggregator: number;
}

/**
 * Interface for the angular controller's scope
 */
interface LocalCalculationResultScope extends ng.IScope, LocalCalculationResultData, ResultsLoadingScreenData {
    // UI state
    isCharacteristicsTableVisible: boolean;

    // Comparison matrix data
    comparisonMatrix: number[][];
    usedAligner: string;
    usedDistanceCalculator: string;
    usedAggregator: string;

    // Dropdown selections
    aligner: Aligner;
    distanceCalculator: DistanceCalculator;
    aggregator: Aggregator;

    // Functions
    calculateLocalCharacteristicsSimilarityMatrix: () => Promise<void>;
}

/**
 * Controller for displaying local calculation results
 */
class LocalCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const localCalculationResult = async ($scope: LocalCalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            $scope.calculateLocalCharacteristicsSimilarityMatrix = async () => {
                const response = await $http.get<LocalCharacteristicsSimilarityMatrixResponse>("/api/LocalCalculationApi/CalculateLocalCharacteristicsSimilarityMatrix", {
                    params: {
                        taskId: $scope.taskId,
                        aligner: $scope.aligner.Value,
                        distanceCalculator: $scope.distanceCalculator.Value,
                        aggregator: $scope.aggregator.Value
                    }
                });

                const result: LocalCharacteristicsSimilarityMatrixResponse = response.data;
                $scope.comparisonMatrix = result.result;

                // TODO: fill this data from response
                $scope.usedAligner = $scope.aligners[result.aligner - 1].Text;
                $scope.usedDistanceCalculator = $scope.distanceCalculators[result.distanceCalculator - 1].Text;
                $scope.usedAggregator = $scope.aggregators[result.aggregator - 1].Text;
            };


            $scope.isCharacteristicsTableVisible = false;

            await initScopeFromServer<LocalCalculationResultData>(
                $http,
                $scope,
                "Loading local characteristics calculation results",
                "Failed loading local characteristics data");

            $scope.aligner = $scope.aligners[0];
            $scope.distanceCalculator = $scope.distanceCalculators[0];
            $scope.aggregator = $scope.aggregators[0];

            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
        }

        angular.module("libiada").controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
    }
}

export default function LocalCalculationResultController(): LocalCalculationResultHandler {
    return new LocalCalculationResultHandler();
}
