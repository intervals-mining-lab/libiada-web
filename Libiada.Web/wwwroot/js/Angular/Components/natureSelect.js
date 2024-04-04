function natureSelect() {
    "use strict";

    function NatureSelectController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.nature ??= ctrl.natures[0].Value;
            ctrl.filterByNature();
        };
    }

    angular.module("libiada").component("natureSelect", {
        templateUrl: `${window.location.origin}/AngularTemplates/_NatureSelect`,
        controller: NatureSelectController,
        bindings: {
            natures: "<",
            nature: "=",
            filterByNature: "&"
        }
    });
}

natureSelect();
