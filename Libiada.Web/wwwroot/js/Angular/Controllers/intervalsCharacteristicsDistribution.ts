/// <reference types="angular" />

/**
 * Interface for intervals characteristics distribution data
 */
interface IIntervalsCharacteristicsDistributionData {
}

/**
 * Interface for controller scope
 */
interface IIntervalsCharacteristicsDistributionScope extends ng.IScope {
}

/**
 * Controller for intervals characteristics distribution
 */
class IntervalsCharacteristicsDistributionHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
     */
    constructor(data: IIntervalsCharacteristicsDistributionData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: IIntervalsCharacteristicsDistributionData): void {
        const intervalsCharacteristicsDistribution = ($scope: IIntervalsCharacteristicsDistributionScope): void => {
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
function IntervalsCharacteristicsDistributionController(data: IIntervalsCharacteristicsDistributionData): IntervalsCharacteristicsDistributionHandler {
    return new IntervalsCharacteristicsDistributionHandler(data);
}
