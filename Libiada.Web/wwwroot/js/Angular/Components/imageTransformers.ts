import type { ImageTransformer } from "viewDataTypes";

interface ImageTransformersComponentController extends ng.IController {
    imageTransformers: ImageTransformer[];
    fileType: string;
    selectedImageTransformers: ImageTransformer[];

    $onInit(): void;
    addImageTransformation: () => void;
    deleteImageTransformation: (transformation: ImageTransformer) => void;
    
}

function ImageTransformersController(this: ImageTransformersComponentController): void {
    const ctrl: ImageTransformersComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.selectedImageTransformers = [];
    };

    ctrl.addImageTransformation = (): void => {
        ctrl.selectedImageTransformers.push(ctrl.imageTransformers[0]);
    };

    ctrl.deleteImageTransformation = (transformation: ImageTransformer): void => {
        const transformationIndex: number = ctrl.selectedImageTransformers.indexOf(transformation);
        ctrl.selectedImageTransformers.splice(transformationIndex, 1);
    };
}

angular.module("libiada").component("imageTransformers", {
    templateUrl: `/AngularTemplates/_ImageTransformers`,
    controller: ImageTransformersController,
    bindings: {
        imageTransformers: "<",
        fileType: "<"
    }
});
