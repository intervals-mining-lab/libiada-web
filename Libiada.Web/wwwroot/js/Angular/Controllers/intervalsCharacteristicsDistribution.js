import { MapModelFromJson } from "functions";
/**
 * Controller for intervals characteristics distribution
 */
class IntervalsCharacteristicsDistributionHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit(data) {
        const intervalsCharacteristicsDistribution = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("IntervalsCharacteristicsDistributionCtrl", ["$scope", intervalsCharacteristicsDistribution]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of IntervalsCharacteristicsDistributionHandler
 */
export default function IntervalsCharacteristicsDistributionController(data) {
    return new IntervalsCharacteristicsDistributionHandler(data);
}
//# sourceMappingURL=intervalsCharacteristicsDistribution.js.map