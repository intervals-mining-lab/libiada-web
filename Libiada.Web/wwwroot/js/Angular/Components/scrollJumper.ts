interface ScrollJumperController extends ng.IController {
    scrolledUp: boolean;
    scrolledDown: boolean;
    savedPosition: number;

    $onInit(): void;
    isScrollable: () => boolean;
    scrollDown: () => void;
    scrollUp: () => void;
}

function ScrollJumperController(this: ScrollJumperController): void {
    const ctrl: ScrollJumperController = this;

    ctrl.$onInit = (): void => {
        ctrl.scrolledUp = false;
        ctrl.scrolledDown = false;
        ctrl.savedPosition = 0;
    };

    ctrl.isScrollable = (): boolean => {
        const documentHeight: number | undefined = $("html").height();
        return documentHeight !== undefined && window.innerHeight < documentHeight;
    };

    ctrl.scrollDown = (): void => {
        if (ctrl.scrolledDown) {
            $("html, body").animate({ scrollTop: ctrl.savedPosition }, 400);
            ctrl.scrolledDown = false;
            ctrl.scrolledUp = false;
        } else {
            ctrl.savedPosition = window.scrollY || document.documentElement.scrollTop;
            $("html, body").animate({ scrollTop: $("body").height() }, 400);
            ctrl.scrolledDown = true;
        }
    };

    ctrl.scrollUp = (): void => {
        if (ctrl.scrolledUp) {
            $("html, body").animate({ scrollTop: ctrl.savedPosition }, 400);
            ctrl.scrolledDown = false;
            ctrl.scrolledUp = false;
        } else {
            ctrl.savedPosition = window.scrollY || document.documentElement.scrollTop;
            $("html, body").animate({ scrollTop: "0px" }, 400);
            ctrl.scrolledUp = true;
        }
    };
}

angular.module("libiada").component("scrollJumper", {
    templateUrl: `/AngularTemplates/_ScrollJumper`,
    controller: ScrollJumperController
});
