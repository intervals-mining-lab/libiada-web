interface LocalFileController extends ng.IController {
    localFile: boolean;

    $onInit(): void;
}

function LocalFileController(this: LocalFileController): void {
    const ctrl: LocalFileController = this;

    ctrl.$onInit = (): void => {
        ctrl.localFile = false;
    };
}

angular.module("libiada").component("localFile", {
    templateUrl: `/AngularTemplates/_LocalFile`,
    controller: LocalFileController
});
