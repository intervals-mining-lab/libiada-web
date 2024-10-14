function mattersSubmit() {
    "use strict";

    function MattersSubmitController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.submitName ??= "Calculate";
            ctrl.minimumSelectedMatters = ctrl.minimumSelectedMatters ? parseInt(ctrl.minimumSelectedMatters) : 0;
            ctrl.selectedSequenceGroupsCount ??= 0;
            ctrl.selectedMattersCount ??= 0;
        };
    }

    angular.module("libiada").component("mattersSubmit", {
        templateUrl: `${window.location.origin}/AngularTemplates/_MattersSubmit`,
        controller: MattersSubmitController,
        bindings: {
            submitName: "@?",
            minimumSelectedMatters: "@?",
            selectedMattersCount: "<?",
            selectedSequenceGroupsCount: "<?"
        }
    });
}

mattersSubmit();
