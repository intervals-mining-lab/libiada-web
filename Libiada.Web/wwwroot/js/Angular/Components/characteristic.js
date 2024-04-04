function characteristic() {
    "use strict";

    function CharacteristicController(filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.characteristicName ??= "characteristicLinkId";
            ctrl.title ??= "Characteristic";

            ctrl.characteristic ??= {};
            ctrl.characteristic.characteristicType = ctrl.characteristicTypes[0];
            ctrl.characteristic.link = ctrl.characteristicTypes[0].Links[0];
            ctrl.characteristic.arrangementType = ctrl.characteristicTypes[0].ArrangementTypes[0];
            ctrl.characteristic.notation = filterFilter(ctrl.notations, { Nature: ctrl.nature })[0];
            ctrl.characteristic.language =  ctrl.languages?.[0];
            ctrl.characteristic.translator = ctrl.translators?.[0];
            ctrl.characteristic.pauseTreatment = ctrl.pauseTreatments?.[0];
            ctrl.characteristic.trajectory = ctrl.trajectories?.[0];
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
        templateUrl: `${window.location.origin}/AngularTemplates/_Characteristic`,
        controller: ["filterFilter", CharacteristicController],
        bindings: {
            characteristic: "=?",
            characteristicTypes: "<",
            nature: "<",
            notations: "<",
            languages: "<?",
            translators: "<?",
            pauseTreatments: "<?",
            trajectories: "<?",
            characteristicsDictionary: "<",
            characteristicName: "@?",
            title: "@?"
        }
    });
}

characteristic();
