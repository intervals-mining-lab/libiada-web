function mattersTable() {
    "use strict";

    function MattersTableController(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.selectedMatters = 0;
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
        }

        ctrl.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                ctrl.selectedMatters++;
            } else {
                ctrl.selectedMatters--;
            }

            ctrl.onSelectedMattersCountChange(ctrl.selectedMatters);
        }

        ctrl.getVisibleMatters = function () {
            var visibleMatters = ctrl.matters;
            visibleMatters = filterFilter(visibleMatters, { Text: ctrl.searchMatter });
            visibleMatters = filterFilter(visibleMatters, { Description: ctrl.searchDescription });
            visibleMatters = filterFilter(visibleMatters, { Group: ctrl.group || "" });
            visibleMatters = filterFilter(visibleMatters, { SequenceType: ctrl.sequenceType || "" });
            visibleMatters = filterFilter(visibleMatters, function (value) {
                return !ctrl.showRefSeqOnly || ctrl.nature !== "1" || value.Text.split("|").slice(-1)[0].indexOf("_") !== -1;
            });

            return visibleMatters;
        }

        ctrl.selectAllVisibleMatters = function () {
            getVisibleMatters().forEach(function (matter) {
                matter.Selected = true;
            });
        }

        ctrl.unselectAllVisibleMatters = function () {
            getVisibleMatters().forEach(function (matter) {
                matter.Selected = false;
            });
        }
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: "Partial/_MattersTable",
        controller: ["filterFilter", MattersTableController],
        bindings: {
            matters: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedMatters: "<",
            onSelectedMattersCountChange: "&"
        }
    });
}
