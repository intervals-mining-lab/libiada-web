function characteristic() {
    "use strict";

    function Characteristic(filterFilter) {
        var ctrl = this;

        function filterByNature() {
            if (ctrl.characteristic)
                ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
        }

        ctrl.$onInit = function () {
            ctrl.characteristic = {
                characteristicType: ctrl.characteristicTypes[0],
                link: ctrl.characteristicTypes[0].CharacteristicLinks[0],
                notation: filterFilter(ctrl.notations, { Nature: ctrl.nature })[0]
            };
        }

        ctrl.$onChanges = function (changes) {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        }

        ctrl.isLinkable = IsLinkable;
        ctrl.selectLink = SelectLink;

        ctrl.filterByNature = filterByNature;

    }

    angular.module("libiada").component("characteristic", {
        templateUrl: window.location.origin + "/Partial/_Characteristic",
        controller: ["filterFilter", Characteristic],
        bindings: {
            characteristicTypes: "<",
            nature: "<",
            notations: "<",
            languages: "<",
            translators: "<"
        }
    });
}

characteristic();
