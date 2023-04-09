function characteristic() {
    "use strict";

    function Characteristic(filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristicName = ctrl.characteristicName || "characteristicLinkId";
            ctrl.title = ctrl.title || "Characteristic";

            ctrl.characteristic = ctrl.characteristic || {};
            ctrl.characteristic.characteristicType = ctrl.characteristicTypes[0];
            ctrl.characteristic.link = ctrl.characteristicTypes[0].Links[0];
            ctrl.characteristic.arrangementType = ctrl.characteristicTypes[0].ArrangementTypes[0];
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
            trajectories: "<",
            characteristicsDictionary: "<",
            characteristicName: "@",
            title: "@"
        }
    });
}

characteristic();
