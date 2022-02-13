function characteristicNatureParams() {
    "use strict";

    function CharacteristicNatureParams(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristic = ctrl.characteristic || {};
            ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            ctrl.characteristic.language = ctrl.languages ? ctrl.languages[0] : null;
            ctrl.characteristic.translator = ctrl.translators ? ctrl.translators[0] : null;
            ctrl.characteristic.pauseTreatment = ctrl.pauseTreatments ? ctrl.pauseTreatments[0] : null;
            ctrl.characteristic.trajectory = ctrl.trajectories ? ctrl.trajectories[0] : null;
        };

        ctrl.$onChanges = changes => {
            if (changes.nature) {
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
        templateUrl: window.location.origin + "/Partial/_CharacteristicNatureParams",
        controller: ["filterFilter", CharacteristicNatureParams],
        bindings: {
            characteristic: "=?",
            nature: "<",
            notations: "<",
            languages: "<",
            translators: "<",
            pauseTreatments: "<",
            trajectories: "<"
        }
    });
}

characteristicNatureParams();
