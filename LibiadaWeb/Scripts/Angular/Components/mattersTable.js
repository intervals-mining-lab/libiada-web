function mattersTable() {
    "use strict";

    function MattersTableController(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.selectedMattersCount = 0;
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
        }

        ctrl.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                ctrl.selectedMattersCount++;
            } else {
                ctrl.selectedMattersCount--;
            }
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
            ctrl.getVisibleMatters().forEach(function (matter) {
                if ((ctrl.selectedMattersCount < ctrl.maximumSelectedMatters) && !matter.Selected) {
                    matter.Selected = true;
                    ctrl.matterCheckChanged(matter);
                }
            });
        }

        ctrl.unselectAllVisibleMatters = function () {
            ctrl.getVisibleMatters().forEach(function (matter) {
                if (matter.Selected) {
                    matter.Selected = false;
                    ctrl.matterCheckChanged(matter);
                }
            });
        }
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: "Partial/_MattersTable",
        controller: ["filterFilter", MattersTableController],
        bindings: {
            selectedMattersCount: "=",
            matters: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedMatters: "<"
        }
    });
}
