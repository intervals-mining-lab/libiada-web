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

interface CharacteristicsComponentController extends ng.IController {
    characteristicTypes: CharacteristicType[];
    nature: string;
    notations: Notation[];
    characteristicsDictionary: CharacteristicsDictionary;
    percentageDifferenceNeeded?: boolean;
    hideNotation?: string;
    languages?: Language[];
    translators?: Translator[];
    pauseTreatments?: PauseTreatment[];
    trajectories?: Trajectory[];
    characteristics: CharacteristicWithNatureParams[];

    $onInit(): void;
    $onChanges(changes: ng.IOnChangesObject): void;
    filterByNature: () => void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: CharacteristicWithNatureParams) => void;
    selectLink: (characteristic: SelectedCharacteristic) => void;
}

function CharacteristicsController(
    this: CharacteristicsComponentController,
    filterFilter: ng.IFilterFilter
): void {
    const ctrl: CharacteristicsComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.characteristics = [];
        ctrl.addCharacteristic();
    };

    ctrl.$onChanges = (changes: ng.IOnChangesObject): void => {
        if (changes["nature"]) {
            ctrl.filterByNature();
        }
    };

    ctrl.filterByNature = (): void => {
        const notation: Notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];

        // if notation is linked to characteristics
        angular.forEach(ctrl.characteristics, characteristic => characteristic.notation = notation);
    };

    ctrl.addCharacteristic = (): void => {
        ctrl.characteristics.push({
            characteristicType: ctrl.characteristicTypes[0],
            link: ctrl.characteristicTypes[0].Links[0],
            arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],

            // if notation is a part of characteristic
            notation: filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
            language: ctrl.languages?.[0],
            translator: ctrl.translators?.[0],
            trajectory: ctrl.trajectories?.[0],
            pauseTreatment: ctrl.pauseTreatments?.[0],
            sequentialTransfer: false
        });
    };

    ctrl.deleteCharacteristic = (characteristic: CharacteristicWithNatureParams): void => {
        ctrl.characteristics.splice(ctrl.characteristics.indexOf(characteristic), 1);
    };

    ctrl.selectLink = SelectLink;
}

angular.module("libiada").component("characteristics", {
    templateUrl: `/AngularTemplates/_Characteristics`,
    controller: ["filterFilter", CharacteristicsController],
    bindings: {
        characteristicTypes: "<",
        nature: "<",
        notations: "<",
        characteristicsDictionary: "<",
        percentageDifferenceNeeded: "<?",
        hideNotation: "@",
        languages: "<?",
        translators: "<?",
        pauseTreatments: "<?",
        trajectories: "<?",
    }
});
