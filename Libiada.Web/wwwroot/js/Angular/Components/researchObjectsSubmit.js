"use strict";
function ResearchObjectsSubmitController() {
    const ctrl = this;
    ctrl.$onInit = () => {
        ctrl.submitName ??= "Calculate";
        // because this parameter is passed as string, converting it into number, 
        // and if it is not passed, then setting it to 0 by default
        ctrl.minimumSelectedResearchObjects = ctrl.minimumSelectedResearchObjects ? parseInt(String(ctrl.minimumSelectedResearchObjects)) : 0;
        ctrl.selectedResearchObjectsCount ??= 0;
        ctrl.selectedSequenceGroupsCount ??= 0;
    };
}
angular.module("libiada").component("researchObjectsSubmit", {
    templateUrl: `/AngularTemplates/_ResearchObjectsSubmit`,
    controller: ResearchObjectsSubmitController,
    bindings: {
        submitName: "@?",
        minimumSelectedResearchObjects: "@?",
        selectedResearchObjectsCount: "<?",
        selectedSequenceGroupsCount: "<?"
    }
});
//# sourceMappingURL=researchObjectsSubmit.js.map