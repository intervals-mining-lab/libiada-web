function researchObjectsSubmit() {
    "use strict";

    function ResearchObjectsSubmitController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.submitName ??= "Calculate";
            ctrl.minimumSelectedResearchObjects = ctrl.minimumSelectedResearchObjects ? parseInt(ctrl.minimumSelectedResearchObjects) : 0;
            ctrl.selectedSequenceGroupsCount ??= 0;
            ctrl.selectedResearchObjectsCount ??= 0;
        };
    }

    angular.module("libiada").component("researchObjectsSubmit", {
        templateUrl: `${window.location.origin}/AngularTemplates/_ResearchObjectsSubmit`,
        controller: ResearchObjectsSubmitController,
        bindings: {
            submitName: "@?",
            minimumSelectedResearchObjects: "@?",
            selectedResearchObjectsCount: "<?",
            selectedSequenceGroupsCount: "<?"
        }
    });
}

researchObjectsSubmit();
