function orderTransformations() {
    "use strict";

    function OrderTransformationsController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.transformationsSequence = [];

            ctrl.addTransformation()
        };

        ctrl.addTransformation = () => {
            ctrl.transformationsSequence.push({
                transformation: ctrl.transformations[0]
            });
        };

        ctrl.deleteTransformation = transformation => {
            const transformationIndex = ctrl.transformationsSequence.indexOf(transformation);
            ctrl.transformationsSequence.splice(transformationIndex, 1);
        };
    }

    angular.module("libiada").component("orderTransformations", {
        templateUrl: `${window.location.origin}/AngularTemplates/_OrderTransformations`,
        controller: OrderTransformationsController,
        bindings: {
            transformations: "<"
        }
    });
}

orderTransformations();
