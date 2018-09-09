function characteristics() {
    "use strict";

    function Characteristics(filterFilter) {
        var ctrl = this;

        ctrl.characteristics = [];

        ctrl.$onInit = function() {
        };

        function filterByNature() {
            if (!ctrl.hideNotation) {
                var notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
                ctrl.notation = notation;
                // if notation is not linked to characteristic
                angular.forEach(ctrl.characteristics, function (characteristic) {
                    characteristic.notation = notation;
                });
            }
        }

        ctrl.$onChanges = function(changes) {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        };

        ctrl.addCharacteristic = function addCharacteristic() {
            ctrl.characteristics.push({
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].Links[0],
                arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],
                // if notation is part of characterisitcs
                notation: ctrl.hideNotation ? 0 : filterFilter(ctrl.notations, { Nature: ctrl.nature })[0],
                language: ctrl.languages ? ctrl.languages[0] : null,
                translator: ctrl.translators ? ctrl.translators[0] : null
            });
        };

        ctrl.deleteCharacteristic = function deleteCharacteristic(characteristic) {
            ctrl.characteristics.splice(ctrl.characteristics.indexOf(characteristic), 1);
        };

        ctrl.selectLink = function(characteristic) {
            "use strict";

            characteristic.link = characteristic.characteristicType.Links[0];
            characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
        };

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
            characteristicsDictionary: "<",
            hideNotation: "@"
        }
    });
}

characteristics();
