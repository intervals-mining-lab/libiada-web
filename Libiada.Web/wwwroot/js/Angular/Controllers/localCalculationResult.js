import { InitializeBootstrapTooltips, initScopeFromServer } from "functions";
/**
 * Controller for displaying local calculation results
 */
class LocalCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const localCalculationResult = async ($scope, $http) => {
            $scope.calculateLocalCharacteristicsSimilarityMatrix = async () => {
                const result = await $http.get("/api/LocalCalculationApi/CalculateLocalCharacteristicsSimilarityMatrix", {
                    params: {
                        taskId: $scope.taskId,
                        aligner: $scope.aligner.Value,
                        distanceCalculator: $scope.distanceCalculator.Value,
                        aggregator: $scope.aggregator.Value
                    }
                });
                const response = result.data;
                $scope.comparisonMatrix = response.result;
                // TODO: fill this data from response
                $scope.usedAligner = $scope.aligners[response.aligner - 1].Text;
                $scope.usedDistanceCalculator = $scope.distanceCalculators[response.distanceCalculator - 1].Text;
                $scope.usedAggregator = $scope.aggregators[response.aggregator - 1].Text;
            };
            $scope.isCharacteristicsTableVisible = false;
            await initScopeFromServer($http, $scope, "Loading local characteristics calculation results", "Failed loading local characteristics data");
            $scope.aligner = $scope.aligners[0];
            $scope.distanceCalculator = $scope.distanceCalculators[0];
            $scope.aggregator = $scope.aggregators[0];
            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
        };
        angular.module("libiada").controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
    }
}
export default function LocalCalculationResultController() {
    return new LocalCalculationResultHandler();
}
//# sourceMappingURL=localCalculationResult.js.map