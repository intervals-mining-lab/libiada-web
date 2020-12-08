﻿function characteristic() {
    "use strict";

    function Characteristic(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.characteristicName = ctrl.characteristicName || "characteristicLinkId";
            ctrl.title = ctrl.title || "Characteristic";
            ctrl.characteristic = ctrl.characteristic || {};
            ctrl.characteristic.characteristicType = ctrl.characteristicTypes[0];
            ctrl.characteristic.link = ctrl.characteristicTypes[0].Links[0];
            ctrl.characteristic.arrangementType = ctrl.characteristicTypes[0].ArrangementTypes[0];
            ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            ctrl.characteristic.pauseTreatment = ctrl.pauseTreatments ? ctrl.pauseTreatments[0] : null;
        };

        ctrl.$onChanges = function (changes) {
            if (changes.nature) {
                ctrl.filterByNature();
            }
        };

        ctrl.filterByNature = function () {
            if (ctrl.characteristic) {
                ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            }
        };

        ctrl.selectLink = SelectLink;
    }

    angular.module("libiada").component("characteristic", {
        templateUrl: window.location.origin + "/Partial/_Characteristic",
        controller: ["filterFilter", Characteristic],
        bindings: {
            characteristic: "=?",
            characteristicTypes: "<",
            nature: "<",
            notations: "<",
            languages: "<",
            translators: "<",
            pauseTreatments: "<",
            characteristicsDictionary: "<",
            characteristicName: "@",
            title: "@"
        }
    });
}

characteristic();
