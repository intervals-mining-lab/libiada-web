function NatureSelectController() {
    const ctrl = this;

    ctrl.$onInit = () => {
        ctrl.nature ??= ctrl.natures[0].Value;
        ctrl.filterByNature ??= () => { };
        ctrl.filterByNature();
    };
}

angular.module("libiada").component("natureSelect", {
    templateUrl: `/AngularTemplates/_NatureSelect`,
    controller: NatureSelectController,
    bindings: {
        natures: "<",
        nature: "=",
        filterByNature: "&?"
    }
});
