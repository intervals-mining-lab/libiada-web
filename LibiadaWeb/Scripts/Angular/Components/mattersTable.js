function mattersTable() {
    "use strict";

    function MattersTableController(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
            ctrl.selectedMatters = 0;
        }

        ctrl.isRefSeq = function (matter) {
            return matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;
        }

        ctrl.matterSelectChange = function (matter) {
            if (matter.Selected) {
                ctrl.selectedMatters++;
            } else {
                ctrl.selectedMatters--;
            }
        }

        ctrl.getVisibleMatters = function () {
            var visibleMatters = ctrl.matters;
            visibleMatters = filterFilter(visibleMatters, { Text: ctrl.searchMatter });
            visibleMatters = filterFilter(visibleMatters, { Group: ctrl.group || "" });
            visibleMatters = filterFilter(visibleMatters, { SequenceType: ctrl.sequenceType || "" });
            visibleMatters = filterFilter(visibleMatters, function (matter) {
                return ctrl.nature !== "1" || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter);
            });

            return visibleMatters;
        }

        ctrl.selectAllVisibleMatters = function () {
            ctrl.getVisibleMatters().forEach(function (matter) {
                if (!matter.Selected && (ctrl.selectedMatters < ctrl.maximumSelectedMatters)) {
                    matter.Selected = true;
                    ctrl.selectedMatters++;
                }
            });
        }

        ctrl.unselectAllVisibleMatters = function () {
            ctrl.getVisibleMatters().forEach(function (matter) {
                if (matter.Selected) {
                    matter.Selected = false;
                    ctrl.selectedMatters--;
                }
            });
        }
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
