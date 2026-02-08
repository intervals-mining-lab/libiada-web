import { MapModelFromJson } from "functions";
import type {
    ImageTransformer,
    OrderTransformation
} from "viewDataTypes";

// Interface for the data object that is passed to the controller
interface CustomSequenceOrderTransformerData {
    orderTransformations: OrderTransformation[];
    imageTransformers: ImageTransformer[];
}

// Interface for the $scope controller
interface CustomSequenceOrderTransformerScope extends ng.IScope, CustomSequenceOrderTransformerData {
}

// Angular controller class
class CustomSequenceOrderTransformerOperator {
    constructor(data: CustomSequenceOrderTransformerData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CustomSequenceOrderTransformerData): void {
        const customSequenceOrderTransformer = ($scope: CustomSequenceOrderTransformerScope): void => {
            MapModelFromJson($scope, data);
        };

        angular.module("libiada").controller("CustomSequenceOrderTransformerCtrl", ["$scope", customSequenceOrderTransformer]);
    }
}

// Wrapper function for backwards compatibility
export default function CustomSequenceOrderTransformerController(data: CustomSequenceOrderTransformerData): CustomSequenceOrderTransformerOperator {
    return new CustomSequenceOrderTransformerOperator(data);
}
