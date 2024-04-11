function featuresSelect() {
    "use strict";

    function FeaturesSelectController() {
        let ctrl = this;

        ctrl.$onInit = () => { };

        ctrl.setCheckBoxesState = (state) => {
            // TODO: optimize this
            angular.forEach(ctrl.features, item => {
                item.Selected = state;
                ctrl.additionalAction({ feature: item });
            });
        };
    }

    angular.module("libiada").component("featuresSelect", {
        templateUrl: `${window.location.origin}/AngularTemplates/_FeaturesSelect`,
        controller: FeaturesSelectController,
        bindings: {
            features: "<",
            additionalAction: "&"
        }
    });
}

featuresSelect();
