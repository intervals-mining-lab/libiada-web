interface LoadingWindowComponentController extends ng.IController {
    loading: boolean;
    loadingScreenHeader: string;
    loadingWindow: bootstrap.Modal;

    $onChanges(changes: ng.IOnChangesObject): void;
}

function LoadingWindowController(this: LoadingWindowComponentController): void {
    const ctrl: LoadingWindowComponentController = this;

    ctrl.loadingWindow = new bootstrap.Modal("#loadingDialog");

    ctrl.$onChanges = (changes: ng.IOnChangesObject): void => {
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
