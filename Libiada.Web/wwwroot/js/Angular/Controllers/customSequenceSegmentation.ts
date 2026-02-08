import { MapModelFromJson } from "functions";
import type {
    ImageTransformer,
    DeviationCalculationMethod,
    SegmentationCriterion,
    Threshold
} from "viewDataTypes";

// Interface for the data object that is passed to the controller
interface CustomSequenceSegmentationData {
    thresholds: Threshold[];
    segmentationCriteria: SegmentationCriterion[];
    deviationCalculationMethods: DeviationCalculationMethod[];
    imageTransformers: ImageTransformer[];
}

// Interface for the $scope controller
interface CustomSequenceSegmentationScope extends ng.IScope, CustomSequenceSegmentationData {
    leftBorder: number;
    rightBorder: number;
    step: number;
    precision: number;
    wordLengthDecrement: number;
    wordLength: number;
    balance: number;
    threshold: Threshold;
    segmentationCriterion: SegmentationCriterion;
    deviationCalculationMethod: DeviationCalculationMethod;

}

// Angular controller class
class CustomSequenceSegmentationOperator {
    constructor(data: CustomSequenceSegmentationData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CustomSequenceSegmentationData): void {
        const customSequenceSegmentation = ($scope: CustomSequenceSegmentationScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("CustomSequenceSegmentationCtrl", ["$scope", customSequenceSegmentation]);
    }
}

// Wrapper function for backwards compatibility
export default function CustomSequenceSegmentationController(data: CustomSequenceSegmentationData): CustomSequenceSegmentationOperator {
    return new CustomSequenceSegmentationOperator(data);
}
