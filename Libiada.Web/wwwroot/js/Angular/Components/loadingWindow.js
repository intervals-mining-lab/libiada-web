"use strict";
function LoadingWindowController() {
    const ctrl = this;
    ctrl.loadingWindow = new bootstrap.Modal("#loadingDialog");
    ctrl.$onChanges = (changes) => {
        if (changes["loading"]) {
            if (ctrl.loading) {
                ctrl.loadingWindow.show();
            }
            else {
                ctrl.loadingWindow.hide();
            }
        }
    };
}
angular.module("libiada").component("loadingWindow", {
    templateUrl: `/AngularTemplates/_LoadingWindow`,
    controller: LoadingWindowController,
    bindings: {
        loading: "<",
        loadingScreenHeader: "<"
    }
});
//# sourceMappingURL=loadingWindow.js.map