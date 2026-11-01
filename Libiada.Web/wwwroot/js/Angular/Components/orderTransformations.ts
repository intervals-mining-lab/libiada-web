import type { OrderTransformation } from "viewDataTypes";

interface SelectedOrderTransformation {
    transformation: OrderTransformation;
}

interface OrderTransformationsController extends ng.IController {
    transformations: OrderTransformation[];
    transformationsSequence: SelectedOrderTransformation[];
    
    $onInit(): void;
    addTransformation: () => void;
    deleteTransformation: (transformation: SelectedOrderTransformation) => void;
}

function OrderTransformationsController(this: OrderTransformationsController): void {
    const ctrl: OrderTransformationsController = this;

    ctrl.$onInit = (): void => {
        ctrl.transformationsSequence = [];
        ctrl.addTransformation();
    };

    ctrl.addTransformation = (): void => {
        ctrl.transformationsSequence.push({
            transformation: ctrl.transformations[0]
        });
    };

    ctrl.deleteTransformation = (transformation: SelectedOrderTransformation): void => {
        const transformationIndex: number = ctrl.transformationsSequence.indexOf(transformation);
        ctrl.transformationsSequence.splice(transformationIndex, 1);
    };
}

angular.module("libiada").component("orderTransformations", {
    templateUrl: `/AngularTemplates/_OrderTransformations`,
    controller: OrderTransformationsController,
    bindings: {
        transformations: "<"
    }
});
