function LocalFileController() {
    const ctrl = this;

    ctrl.$onInit = () => {
        ctrl.localFile = false;
    };
}

angular.module("libiada").component("localFile", {
    templateUrl: `/AngularTemplates/_LocalFile`,
    controller: LocalFileController
});
