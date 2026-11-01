interface ResearchObjectsSubmitComponentController extends ng.IController {
    submitName: string;
    minimumSelectedResearchObjects: number | string;
    selectedResearchObjectsCount: number;
    selectedSequenceGroupsCount: number;

    $onInit(): void;
}

function ResearchObjectsSubmitController(this: ResearchObjectsSubmitComponentController): void {
    const ctrl: ResearchObjectsSubmitComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.submitName ??= "Calculate";
        ctrl.minimumSelectedResearchObjects = ctrl.minimumSelectedResearchObjects ? parseInt(String(ctrl.minimumSelectedResearchObjects)) : 0;
        ctrl.selectedSequenceGroupsCount ??= 0;
        ctrl.selectedResearchObjectsCount ??= 0;
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
