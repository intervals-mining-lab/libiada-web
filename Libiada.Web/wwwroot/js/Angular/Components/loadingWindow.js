function LoadingWindowController() {
    const ctrl = this;

    ctrl.loadingWindow = new bootstrap.Modal("#loadingDialog");

    ctrl.$onInit = () => { };

    ctrl.$onChanges = async changes => {
        if (changes.loading) {
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
    templateUrl: `${window.location.origin}/AngularTemplates/_LoadingWindow`,
    controller: LoadingWindowController,
    bindings: {
        loading: "<",
        loadingScreenHeader: "<"
    }
});
