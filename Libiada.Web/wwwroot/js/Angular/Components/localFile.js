function localFile() {
    "use strict";

    function LocalFileController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.localFile = false;
        };
    }

    angular.module("libiada").component("localFile", {
        templateUrl: `${window.location.origin}/AngularTemplates/_LocalFile`,
        controller: LocalFileController
    });
}

localFile();
