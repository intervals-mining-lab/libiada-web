/// <reference types="angular" />
/**
 * Controller for intervals characteristics distribution
 */
class IntervalsCharacteristicsDistributionHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    /**
     * Initializes the Angular controller
     */
    initializeController() {
        "use strict";
        const intervalsCharacteristicsDistribution = ($scope) => {
            MapModelFromJson($scope, this.data);
        };
        angular.module("libiada").controller("IntervalsCharacteristicsDistributionCtrl", ["$scope", intervalsCharacteristicsDistribution]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of IntervalsCharacteristicsDistributionHandler
 */
function IntervalsCharacteristicsDistributionController(data) {
    return new IntervalsCharacteristicsDistributionHandler(data);
}
//# sourceMappingURL=intervalsCharacteristicsDistribution.js.map