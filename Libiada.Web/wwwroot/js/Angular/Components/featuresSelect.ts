import type { Feature } from "viewDataTypes";

interface FeaturesSelectController extends ng.IController {
    features: Feature[];

    $onInit(): void;
    setCheckBoxesState: (state: boolean) => void;

    // callback function passed from the parent
    additionalAction: (dataWrapper: { feature: Feature }) => void;
}

function FeaturesSelectController(this: FeaturesSelectController): void {
    const ctrl: FeaturesSelectController = this;

    ctrl.$onInit = (): void => {
        ctrl.additionalAction ??= () => { };
     };

    ctrl.setCheckBoxesState = (state: boolean): void => {
        // TODO: optimize this
        angular.forEach(ctrl.features, (item: Feature): void => {
            item.Selected = state;
            ctrl.additionalAction({ feature: item });
        });
    };
}

angular.module("libiada").component("featuresSelect", {
    templateUrl: `/AngularTemplates/_FeaturesSelect`,
    controller: FeaturesSelectController,
    bindings: {
        features: "<",
        additionalAction: "&?"
    }
});
