function CharacteristicNatureParamsController(filterFilter) {
    const ctrl = this;
    ctrl.$onInit = () => {
        ctrl.characteristic ??= {
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
export {};
//# sourceMappingURL=characteristicNatureParams.js.map