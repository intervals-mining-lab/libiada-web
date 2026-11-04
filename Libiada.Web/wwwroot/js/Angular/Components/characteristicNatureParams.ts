import type {
    Language,
    Notation,
    PauseTreatment,
    Trajectory,
    Translator,
    SelectedNatureParams
} from "viewDataTypes";

interface CharacteristicNatureParamsComponentController extends ng.IController {
    characteristic?: SelectedNatureParams;
    nature: string;
    notations: Notation[];
    languages?: Language[];
    translators?: Translator[];
    pauseTreatments?: PauseTreatment[];
    trajectories?: Trajectory[];
    filterByNature: () => void;

    $onInit(): void;
    $onChanges(changes: ng.IOnChangesObject): void;
}

function CharacteristicNatureParamsController(
    this: CharacteristicNatureParamsComponentController,
    filterFilter: ng.IFilterFilter
): void {
    const ctrl: CharacteristicNatureParamsComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.characteristic ??= {
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
}

angular.module("libiada").component("characteristicNatureParams", {
    templateUrl: `/AngularTemplates/_CharacteristicNatureParams`,
    controller: ["filterFilter", CharacteristicNatureParamsController],
    bindings: {
        characteristic: "=?",
        nature: "<",
        notations: "<",
        languages: "<?",
        translators: "<?",
        pauseTreatments: "<?",
        trajectories: "<?"
    }
});
