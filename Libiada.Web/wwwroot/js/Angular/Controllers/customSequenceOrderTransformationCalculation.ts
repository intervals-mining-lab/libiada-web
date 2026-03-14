import { MapModelFromJson } from "functions";
import type {
    CharacteristicType,
    ImageTransformer,
    OrderTransformation
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface CustomSequenceOrderTransformationCalculationData {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: { [key: string]: number };
    imageTransformers: ImageTransformer[];
    transformations: OrderTransformation[];
}

/**
 * Interface for the angular controller's scope
 */
interface CustomSequenceOrderTransformationCalculationScope extends ng.IScope, CustomSequenceOrderTransformationCalculationData {
    // No additional scope properties needed for this controller
}

// Controller class
class CustomSequenceOrderTransformationCalculationOperator {
    constructor(data: CustomSequenceOrderTransformationCalculationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CustomSequenceOrderTransformationCalculationData): void {
        const customSequenceOrderTransformationCalculation = ($scope: CustomSequenceOrderTransformationCalculationScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("CustomSequenceOrderTransformationCalculationCtrl", ["$scope", customSequenceOrderTransformationCalculation]);
    }
}

// Wrapper function for backwards compatibility
export default function CustomSequenceOrderTransformationCalculationController(data: CustomSequenceOrderTransformationCalculationData): CustomSequenceOrderTransformationCalculationOperator {
    return new CustomSequenceOrderTransformationCalculationOperator(data);
}
