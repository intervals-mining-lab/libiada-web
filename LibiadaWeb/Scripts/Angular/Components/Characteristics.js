function characteristics() {
    "use strict";

    function Characteristics(filterFilter) {
        var ctrl = this;

        ctrl.characteristics = [];

        ctrl.$onInit = function () {
        }

        function filterByNature() {
            var notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            ctrl.notation = notation;
            // if notation is not linked to characteristic
            angular.forEach(ctrl.characteristics, function (characteristic) {
                characteristic.notation = notation;
            });
        }

        ctrl.$onChanges = function (changes) {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        }

        ctrl.addCharacteristic = function addCharacteristic() {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].CharacteristicLinks[0],
                // if notation is part of characterisitcs
                notation: filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
                language: ctrl.languages ? ctrl.languages[0] : null,
                translator: ctrl.translators ? ctrl.translators[0] : null
            });
        }

        ctrl.deleteCharacteristic = function deleteCharacteristic(characteristic) {
            ctrl.characteristics.splice(ctrl.characteristics.indexOf(characteristic), 1);
        }

        ctrl.isLinkable = IsLinkable;
        ctrl.selectLink = SelectLink;

        ctrl.filterByNature = filterByNature;

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
            hideNotation: "<"
        }
    });
}

characteristics();
