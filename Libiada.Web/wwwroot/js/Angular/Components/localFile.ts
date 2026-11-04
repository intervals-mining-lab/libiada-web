interface LocalFileComponentController extends ng.IController {
    localFile: boolean;

    $onInit(): void;
}

function LocalFileController(this: LocalFileComponentController): void {
    const ctrl: LocalFileComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.localFile = false;
    };
}

angular.module("libiada").component("localFile", {
    templateUrl: `/AngularTemplates/_LocalFile`,
    controller: LocalFileController
});
