function characteristicsWithoutNotation() {
    "use strict";

    function CharacteristicsWithoutNotation() {
        let ctrl = this;

        ctrl.characteristics = [];

        ctrl.$onInit = () => { };

        ctrl.$onChanges = changes => { };

        ctrl.addCharacteristic = () => {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].Links[0],
                arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],
                language: ctrl.languages ? ctrl.languages[0] : null,
                translator: ctrl.translators ? ctrl.translators[0] : null,
                pauseTreatment: ctrl.pauseTreatments ? ctrl.pauseTreatments[0] : null
            });
        };

        ctrl.deleteCharacteristic = characteristic => {
            ctrl.characteristics.splice(ctrl.characteristics.indexOf(characteristic), 1);
        };

        ctrl.selectLink = characteristic => {
            "use strict";
            characteristic.link = characteristic.characteristicType.Links[0];
            characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
        };
    }

    angular.module("libiada").component("characteristicsWithoutNotation", {
        templateUrl: window.location.origin + "/Shared/_CharacteristicsWithoutNotation",
        controller: [CharacteristicsWithoutNotation],
        bindings: {
            characteristicTypes: "<",
            languages: "<",
            translators: "<",
            pauseTreatments: "<",
            characteristicsDictionary: "<",
            hideNotation: "@"
        }
    });
}

characteristicsWithoutNotation();
