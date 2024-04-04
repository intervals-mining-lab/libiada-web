function characteristicsWithoutNotation() {
    "use strict";

    function CharacteristicsWithoutNotationController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristics = [];
            ctrl.addCharacteristic();
        };

        ctrl.addCharacteristic = () => {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].Links[0],
                arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],
                language: ctrl.languages?.[0],
                translator: ctrl.translators?.[0],
                pauseTreatment: ctrl.pauseTreatments?.[0]
            });
        };

        ctrl.deleteCharacteristic = characteristic => {
            const characteristicIndex = ctrl.characteristics.indexOf(characteristic);
            ctrl.characteristics.splice(characteristicIndex, 1);
        };

        ctrl.selectLink = characteristic => {
            characteristic.link = characteristic.characteristicType.Links[0];
            characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
        };
    }

    angular.module("libiada").component("characteristicsWithoutNotation", {
        templateUrl: `${window.location.origin}/AngularTemplates/_CharacteristicsWithoutNotation`,
        controller: CharacteristicsWithoutNotationController,
        bindings: {
            characteristicTypes: "<",
            languages: "<?",
            translators: "<?",
            pauseTreatments: "<?",
            characteristicsDictionary: "<",
            hideNotation: "@"
        }
    });
}

characteristicsWithoutNotation();
