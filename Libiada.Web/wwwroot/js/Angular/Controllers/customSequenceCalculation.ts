import { MapModelFromJson } from "functions";
import type {
    CharacteristicType,
    ImageTransformer
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface CustomSequenceCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    imageTransformers: ImageTransformer[];
}

/**
 * Interface for the angular controller's scope
 */
interface CustomSequenceCalculationScope extends ng.IScope, CustomSequenceCalculationData {
}

// Controller class
class CustomSequenceCalculationOperator {
    constructor(data: CustomSequenceCalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CustomSequenceCalculationData): void {
        const customSequenceCalculation = ($scope: CustomSequenceCalculationScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("CustomSequenceCalculationCtrl", ["$scope", customSequenceCalculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CustomSequenceCalculationController(data: CustomSequenceCalculationData): CustomSequenceCalculationOperator {
    return new CustomSequenceCalculationOperator(data);
}
