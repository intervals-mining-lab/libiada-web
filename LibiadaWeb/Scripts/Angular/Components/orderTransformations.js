function orderTransformations() {
    "use strict";

    function OrderTransformationsController() {
        var ctrl = this;

        ctrl.$onInit = function () {
        }

        ctrl.$onChanges = function (changes) {
        }

        ctrl.addTransformation = function addTransformation() {
            ctrl.transformations.push({
                link: ctrl.transformationLinks[0],
                operation: ctrl.operations[0]
            });
        }

        ctrl.deleteTransformation = function deleteTransformation(transformation) {
            ctrl.transformations.splice(ctrl.transformations.indexOf(transformation), 1);
        }

    }

    angular.module("libiada").component("orderTransformations", {
        templateUrl: window.location.origin + "/Partial/_OrderTransformations",
        controller: [OrderTransformationsController],
        bindings: {
            transformations: "<",
            operations: "<",
            transformationLinks: "<"
        }
    });
}

orderTransformations();
