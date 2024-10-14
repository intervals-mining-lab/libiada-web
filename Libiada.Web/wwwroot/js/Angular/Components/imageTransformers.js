function imageTransformers() {
    "use strict";

    function ImageTransformersController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.selectedImageTransformers = [];
        };

        ctrl.addImageTransformation = () => ctrl.selectedImageTransformers.push(ctrl.imageTransformers[0]);

        ctrl.deleteImageTransformation = (transformation) => {
            const transformationIndex = ctrl.selectedImageTransformers.indexOf(transformation);
            ctrl.selectedImageTransformers.splice(transformationIndex, 1);
        };
    }

    angular.module("libiada").component("imageTransformers", {
        templateUrl: `${window.location.origin}/AngularTemplates/_ImageTransformers`,
        controller: ImageTransformersController,
        bindings: {
            imageTransformers: "<",
            fileType: "<"
        }
    });
}

imageTransformers();
