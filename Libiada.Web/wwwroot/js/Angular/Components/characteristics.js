import { SelectLink } from "functions";
function CharacteristicsController(filterFilter) {
    const ctrl = this;
    ctrl.$onInit = () => {
        ctrl.characteristics = [];
        ctrl.addCharacteristic();
    };
    ctrl.$onChanges = (changes) => {
        if (changes["nature"]) {
            ctrl.filterByNature();
        }
    };
    ctrl.filterByNature = () => {
        const notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
        // if notation is linked to characteristics
        angular.forEach(ctrl.characteristics, characteristic => characteristic.notation = notation);
    };
    ctrl.addCharacteristic = () => {
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
    ctrl.deleteCharacteristic = (characteristic) => {
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
//# sourceMappingURL=characteristics.js.map