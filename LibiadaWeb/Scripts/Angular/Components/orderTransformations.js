﻿function orderTransformations() {
    "use strict";

    function OrderTransformationsController() {
        var ctrl = this;

        ctrl.$onInit = () => {
            ctrl.transformationsSequence = [];
        };

        ctrl.addTransformation = () => {
            ctrl.transformationsSequence.push({
                transformation: ctrl.transformations[0]
            });
        };

        ctrl.deleteTransformation = transformation => 
            ctrl.transformations.splice(ctrl.transformationsSequence.indexOf(transformation), 1);
    }

    angular.module("libiada").component("orderTransformations", {
        templateUrl: window.location.origin + "/Partial/_OrderTransformations",
        controller: [OrderTransformationsController],
        bindings: {
            transformations: "<"
        }
    });
}

orderTransformations();
