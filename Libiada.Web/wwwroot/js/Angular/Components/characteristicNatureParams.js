function characteristicNatureParams() {
    "use strict";

    function CharacteristicNatureParamsController(filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristic ??= {};
            ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            ctrl.characteristic.language = ctrl.languages?.[0];
            ctrl.characteristic.translator = ctrl.translators?.[0];
            ctrl.characteristic.pauseTreatment = ctrl.pauseTreatments?.[0];
            ctrl.characteristic.trajectory =  ctrl.trajectories?.[0];
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
        templateUrl: `${window.location.origin}/AngularTemplates/_CharacteristicNatureParams`,
        controller: ["filterFilter", CharacteristicNatureParamsController],
        bindings: {
            characteristic: "=?",
            nature: "<",
            notations: "<",
            languages: "<?",
            translators: "<?",
            pauseTreatments: "<?",
            trajectories: "<?"
        }
    });
}

characteristicNatureParams();
