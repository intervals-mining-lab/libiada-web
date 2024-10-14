function characteristics() {
    "use strict";

    function CharacteristicsController(filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristics = [];
            ctrl.addCharacteristic();
        };

        ctrl.$onChanges = changes => {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        };

        ctrl.filterByNature = () => {
            if (!ctrl.hideNotation) {
                let notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
                ctrl.notation = notation;

                // if notation is not linked to characteristic
                angular.forEach(ctrl.characteristics, characteristic => characteristic.notation = notation);
            }
        };

        ctrl.addCharacteristic = () => {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].Links[0],
                arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],

                // if notation is a part of characteristic
                notation: ctrl.hideNotation ? 0 : filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
                language: ctrl.languages?.[0],
                translator: ctrl.translators?.[0],
                pauseTreatment: ctrl.pauseTreatments?.[0],
                trajectory: ctrl.trajectories?.[0]
            });
        };

        ctrl.deleteCharacteristic = characteristic => {
            ctrl.characteristics.splice(ctrl.characteristics.indexOf(characteristic), 1);
        };

        ctrl.selectLink = characteristic => {
            characteristic.link = characteristic.characteristicType.Links[0];
            characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
        };
    }

    angular.module("libiada").component("characteristics", {
        templateUrl: `${window.location.origin}/AngularTemplates/_Characteristics`,
        controller: ["filterFilter", CharacteristicsController],
        bindings: {
            characteristicTypes: "<",
            nature: "<",
            notations: "<",
            languages: "<?",
            translators: "<?",
            pauseTreatments: "<?",
            trajectories: "<?",
            characteristicsDictionary: "<",
            percentageDifferenseNeeded: "<",
            hideNotation: "@"
        }
    });
}

characteristics();
