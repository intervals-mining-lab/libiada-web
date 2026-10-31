import type { ImageTransformer } from "viewDataTypes";

type CustomSequenceFileType = "genetic" | "text" | "image" | "wavFile";

interface CustomSequence {
    sequence: string;
}

interface CustomSequencesComponentController extends ng.IController {
    imageTransformers: ImageTransformer[];
    fileType: CustomSequenceFileType;
    customSequences: CustomSequence[];
    localFile: boolean;
    toLower?: boolean;
    removePunctuation?: boolean;

    $onInit(): void;
    addSequence: () => void;
    deleteSequence: (customSequence: CustomSequence) => void;
}

function CustomSequencesController(this: CustomSequencesComponentController): void {
    const ctrl: CustomSequencesComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.fileType = "genetic";
        ctrl.customSequences = [];

        ctrl.addSequence();
    };

    ctrl.addSequence = (): void => {
        ctrl.customSequences.push({ sequence: "" });
    };

    ctrl.deleteSequence = (customSequence: CustomSequence): void => {
        const customSequenceIndex = ctrl.customSequences.indexOf(customSequence);
        ctrl.customSequences.splice(customSequenceIndex, 1);
    };
}

angular.module("libiada").component("customSequences", {
    templateUrl: `/AngularTemplates/_CustomSequences`,
    controller: CustomSequencesController,
    bindings: {
        imageTransformers: "<"
    }
});
