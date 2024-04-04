function customSequences() {
    "use strict";

    function CustomSequencesController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.fileType = "genetic";
            ctrl.customSequences = [];

            ctrl.addSequence();
        };

        ctrl.addSequence = () => {
            ctrl.customSequences.push({});
        };

        ctrl.deleteSequence = (customSequence) => {
            const customSequenceIndex = ctrl.customSequences.indexOf(customSequence);
            ctrl.customSequences.splice(customSequenceIndex, 1);
        };
    }

    angular.module("libiada").component("customSequences", {
        templateUrl: `${window.location.origin}/AngularTemplates/_CustomSequences`,
        controller: CustomSequencesController,
        bindings: {
            imageTransformers: "<"
        }
    });
}

customSequences();
