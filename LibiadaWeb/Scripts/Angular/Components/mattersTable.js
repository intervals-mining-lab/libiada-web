function mattersTable() {
    "use strict";

    function MattersTableController(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
            ctrl.selectedMatters = 0;

            if (ctrl.checkboxes) {
                ctrl.mattersInputName = "matterIds";
                ctrl.mattersInputType = "checkbox";
            } else {
                ctrl.mattersInputName = "matterId";
                ctrl.mattersInputType = "radio";
            }
        };

        ctrl.$onChanges = function (changes) {
            if (changes.nature) {
                ctrl.toogleMattersVisibility(true);
            }
        };

        ctrl.toogleMattersVisibility = function (isNewNature) {
            if (isNewNature) {
                ctrl.matters.forEach(m => m.Selected = false);
            }

            ctrl.matters.forEach(m => ctrl.setMatterVisibility(m));
        };

        ctrl.setMatterVisibility = function (matter) {
            ctrl.searchMatterText = ctrl.searchMatterText || "";
            matter.Visible = matter.Selected || (ctrl.searchMatterText.length >= 4
                          && matter.Nature == ctrl.nature
                          && matter.Group.includes(ctrl.group || "")
                          && matter.SequenceType.includes(ctrl.sequenceType || "")
                          && matter.Text.toUpperCase().includes(ctrl.searchMatterText.toUpperCase())
                          && (ctrl.nature != ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter)));
        };

        ctrl.isRefSeq = function (matter) {
            return matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;
        };

        ctrl.matterSelectChange = function (matter) {
            if (matter.Selected) {
                ctrl.selectedMatters++;
            } else {
                ctrl.selectedMatters--;
            }
        };

        ctrl.getVisibleMatters = function () {
            return ctrl.matters.filter(m => m.Visible);
        };

        ctrl.selectAllVisibleMatters = function () {
            ctrl.matters.filter(m => m.Visible).forEach(function (matter) {
                if (!matter.Selected && (ctrl.selectedMatters < ctrl.maximumSelectedMatters)) {
                    matter.Selected = true;
                    ctrl.selectedMatters++;
                }
            });
        };

        ctrl.unselectAllVisibleMatters = function () {
            ctrl.matters.filter(m => m.Selected).forEach(function (matter) {
                matter.Selected = false;

                ctrl.setMatterVisibility(matter);
            });

            ctrl.selectedMatters = 0;
        };
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: window.location.origin + "/Partial/_MattersTable",
        controller: ["filterFilter", MattersTableController],
        bindings: {
            matters: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedMatters: "<",
            selectedMatters: "="
        }
    });
}

mattersTable();
