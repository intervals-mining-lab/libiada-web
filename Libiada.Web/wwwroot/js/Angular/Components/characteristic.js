import { SelectLink } from "functions";
function CharacteristicController(filterFilter) {
    const ctrl = this;
    ctrl.$onInit = () => {
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
    ctrl.$onChanges = (changes) => {
        if (changes["nature"]) {
            ctrl.filterByNature();
        }
    };
    ctrl.filterByNature = () => {
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
//# sourceMappingURL=characteristic.js.map