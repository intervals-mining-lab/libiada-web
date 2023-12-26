function scrollJumper() {
    "use strict";

    function ScrollJumperController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.scrolledUp = false;
            ctrl.scrolledDown = false;
            ctrl.savedPosition = 0;
        };

        ctrl.isScrollable = () => document.body.clientHeight - document.documentElement.clientHeight > 0;

        ctrl.scrollDown = () => {
            if (ctrl.scrolledDown) {
                $('html, body').animate({ scrollTop: ctrl.savedPosition }, 400);
                ctrl.scrolledDown = false;
                ctrl.scrolledUp = false;
            } else {
                ctrl.savedPosition = window.pageYOffset || document.documentElement.scrollTop;
                $('html, body').animate({ scrollTop: $('body').height() }, 400);
                ctrl.scrolledDown = true;
            }
        };

        ctrl.scrollUp = () => {
            if (ctrl.scrolledUp) {
                $('html, body').animate({ scrollTop: ctrl.savedPosition }, 400);
                ctrl.scrolledDown = false;
                ctrl.scrolledUp = false;
            } else {
                ctrl.savedPosition = window.pageYOffset || document.documentElement.scrollTop;
                $('html, body').animate({ scrollTop: '0px' }, 400);
                ctrl.scrolledUp = true;
            }
        };
    }

    angular.module("libiada", []).component("scrollJumper", {
        templateUrl: window.location.origin + "/AngularTemplates/_ScrollJumper",
        controller: [ScrollJumperController]
    });
}

scrollJumper();
