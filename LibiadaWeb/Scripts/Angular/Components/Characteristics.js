function characteristics() {
    "use strict";

    function Characteristics(filterFilter) {
        var ctrl = this;

        ctrl.characteristics = [];

        ctrl.$onInit = () => { };

        ctrl.filterByNature = () => {
            if (!ctrl.hideNotation) {
                var notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
                ctrl.notation = notation;

                // if notation is not linked to characteristic
                angular.forEach(ctrl.characteristics, characteristic => characteristic.notation = notation);
            }
        };

        ctrl.$onChanges = changes => {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        };

        ctrl.addCharacteristic = () => {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].Links[0],
                arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],

                // if notation is a part of characteristic
                notation: ctrl.hideNotation ? 0 : filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
                language: ctrl.languages ? ctrl.languages[0] : null,
                translator: ctrl.translators ? ctrl.translators[0] : null,
                pauseTreatment: ctrl.pauseTreatments ? ctrl.pauseTreatments[0] : null,
                trajectory: ctrl.trajectories ? ctrl.trajectories[0] : null
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
        templateUrl: window.location.origin + "/Partial/_Characteristics",
        controller: ["filterFilter", Characteristics],
        bindings: {
            characteristicTypes: "<",
            nature: "<",
            notations: "<",
            languages: "<",
            translators: "<",
            pauseTreatments: "<",
            trajectories: "<",
            characteristicsDictionary: "<",
            hideNotation: "@"
        }
    });
}

characteristics();
