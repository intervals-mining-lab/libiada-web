import { SelectLink } from "functions";
import type {
    CharacteristicType,
    Language,
    Notation,
    PauseTreatment,
    SelectedCharacteristic,
    Trajectory,
    Translator,
    CharacteristicWithNatureParams,
    CharacteristicsDictionary
} from "viewDataTypes";

interface CharacteristicComponentController extends ng.IController {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: CharacteristicsDictionary;
    nature: string;
    notations: Notation[];
    characteristic?: CharacteristicWithNatureParams;
    languages?: Language[];
    translators?: Translator[];
    pauseTreatments?: PauseTreatment[];
    trajectories?: Trajectory[];
    characteristicName?: string;
    title?: string;
    characteristicLinkId?: number;
    hideNotation?: boolean;

    $onInit(): void;
    $onChanges(changes: ng.IOnChangesObject): void;
    filterByNature: () => void;
    selectLink: (characteristic: SelectedCharacteristic) => void;
}

function CharacteristicController(this: CharacteristicComponentController, filterFilter: ng.IFilterFilter): void {
    const ctrl: CharacteristicComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.characteristicName ??= "characteristicLinkId";
        ctrl.title ??= "Characteristic";

        ctrl.characteristic ??= {
            characteristicType: ctrl.characteristicTypes[0],
            link: ctrl.characteristicTypes[0].Links[0],
            arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],
            notation: filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
            language: ctrl.languages?.[0],
            translator: ctrl.translators?.[0],
            trajectory: ctrl.trajectories?.[0],
            pauseTreatment: ctrl.pauseTreatments?.[0],
            sequentialTransfer: false
        };
    };

    ctrl.$onChanges = (changes: ng.IOnChangesObject): void => {
        if (changes["nature"]) {
            ctrl.filterByNature();
        }
    };

    ctrl.filterByNature = (): void => {
        if (ctrl.characteristic) {
            ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
        }
    };

    ctrl.selectLink = SelectLink;
}

angular.module("libiada").component("characteristic", {
    templateUrl: `/AngularTemplates/_Characteristic`,
    controller: ["filterFilter", CharacteristicController],
    bindings: {
        characteristicTypes: "<",
        characteristicsDictionary: "<",
        nature: "<",
        notations: "<",
        characteristic: "=?",
        languages: "<?",
        translators: "<?",
        pauseTreatments: "<?",
        trajectories: "<?",
        characteristicName: "@?",
        title: "@?"
    }
});
