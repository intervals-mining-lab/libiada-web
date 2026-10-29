import { MapModelFromJson } from "functions";
import { CharacteristicType } from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface IntervalsCharacteristicsDistributionData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
}

/**
 * Interface for the angular controller's scope
 */
interface IntervalsCharacteristicsDistributionScope extends ng.IScope, IntervalsCharacteristicsDistributionData {
}

/**
 * Controller for intervals characteristics distribution
 */
class IntervalsCharacteristicsDistributionHandler {
    constructor(data: IntervalsCharacteristicsDistributionData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(data: IntervalsCharacteristicsDistributionData): void {
        const intervalsCharacteristicsDistribution = ($scope: IntervalsCharacteristicsDistributionScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("IntervalsCharacteristicsDistributionCtrl", 
            ["$scope", intervalsCharacteristicsDistribution]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of IntervalsCharacteristicsDistributionHandler
 */
export default function IntervalsCharacteristicsDistributionController(
    data: IntervalsCharacteristicsDistributionData): IntervalsCharacteristicsDistributionHandler {
    return new IntervalsCharacteristicsDistributionHandler(data);
}
