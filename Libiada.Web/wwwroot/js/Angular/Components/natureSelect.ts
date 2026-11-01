import type { Nature } from "viewDataTypes";

interface NatureSelectComponentController extends ng.IController {
    natures: Nature[];
    nature: string;
    filterByNature?: (() => void);

    $onInit(): void;
}

function NatureSelectController(this: NatureSelectComponentController): void {
    const ctrl: NatureSelectComponentController = this;

    ctrl.$onInit = (): void => {
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
