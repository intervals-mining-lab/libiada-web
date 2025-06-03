/// <reference types="angular" />

/**
 * Interface for intervals characteristics distribution data
 */
interface IIntervalsCharacteristicsDistributionData {
    // Данные могут содержать любые свойства из сервера
    [key: string]: any;
}

/**
 * Interface for controller scope
 */
interface IIntervalsCharacteristicsDistributionScope extends ng.IScope {
    // Свойства будут заполнены через MapModelFromJson
    [key: string]: any;
}

/**
 * Controller for intervals characteristics distribution
 */
class IntervalsCharacteristicsDistributionHandler {
    /**
     * Creates a new instance of the controller
     * @param data Data for controller initialization
     */
    constructor(private data: IIntervalsCharacteristicsDistributionData) {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        "use strict";

        const intervalsCharacteristicsDistribution = ($scope: IIntervalsCharacteristicsDistributionScope): void => {
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
function IntervalsCharacteristicsDistributionController(data: IIntervalsCharacteristicsDistributionData): IntervalsCharacteristicsDistributionHandler {
    return new IntervalsCharacteristicsDistributionHandler(data);
}
