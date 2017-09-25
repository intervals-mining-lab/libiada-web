function mattersTable() {
    "use strict";

    function MattersTableController(filterFilter) {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
        }

        ctrl.isRefSeq = function (matter) {
            return matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;
        }

        ctrl.getVisibleMatters = function () {
            var visibleMatters = ctrl.matters;
            visibleMatters = filterFilter(visibleMatters, { Text: ctrl.searchMatter });
            visibleMatters = filterFilter(visibleMatters, { Description: ctrl.searchDescription });
            visibleMatters = filterFilter(visibleMatters, { Group: ctrl.group || "" });
            visibleMatters = filterFilter(visibleMatters, { SequenceType: ctrl.sequenceType || "" });
            visibleMatters = filterFilter(visibleMatters, function (matter) {
                return ctrl.nature !== "1" || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter);
            });

            return visibleMatters;
        }

        ctrl.selectAllVisibleMatters = function () {
            ctrl.getVisibleMatters().forEach(function (matter) {
                if (filterFilter(ctrl.matters, { Selected: true }).length < ctrl.maximumSelectedMatters) {
                    matter.Selected = true;
                }
            });
        }

        ctrl.unselectAllVisibleMatters = function () {
            ctrl.getVisibleMatters().forEach(function (matter) {
                if (matter.Selected) {
                    matter.Selected = false;
                }
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
            maximumSelectedMatters: "<"
        }
    });
}
