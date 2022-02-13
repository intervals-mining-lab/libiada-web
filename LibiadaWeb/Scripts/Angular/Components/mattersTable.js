function mattersTable() {
    "use strict";

    function MattersTableController($scope, filterFilter) {
        var ctrl = this;

        ctrl.$onInit = () => {
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

        ctrl.$onChanges = changes => {
            if (changes.nature) {
                ctrl.toogleMattersVisibility(true);
            }
        };

        ctrl.toogleMattersVisibility = isNewNature => {
            if (isNewNature) {
                ctrl.matters.forEach(m => m.Selected = false);
            }

            var oldVisibleMatters = ctrl.getVisibleMatters();
            ctrl.matters.forEach(m => ctrl.setMatterVisibility(m));
            var visibleMatters = ctrl.getVisibleMatters();

            var mattersToHide = oldVisibleMatters.filter(m => !visibleMatters.includes(m));
            var mattersToShow = visibleMatters.filter(m => !oldVisibleMatters.includes(m));

            $(mattersToHide.map(m => `#matterRow${m.Value}`).join(",")).remove();

            $("#mattersSelectList").append(mattersToShow.map(m =>
                `<tr id="matterRow${m.Value}">
                    <td>
                        <input type="${ctrl.mattersInputType}"
                               name="${ctrl.mattersInputName}"
                               id="matter${m.Value}"
                               value="${m.Value}" />
                        <label for="matter${m.Value}">${m.Text}</label>
                    </td>
                    <td>${m.Group}</td>
                    <td>${m.SequenceType}</td>
                </tr>`).join());

            mattersToShow.forEach(m => $(`#matter${m.Value}`).change(() => {
                ctrl.toggleMatterSelection(m);
                $scope.$apply();
            }
            ));

        };

        ctrl.setMatterVisibility = matter => {

            ctrl.searchMatterText = ctrl.searchMatterText || "";
            matter.Visible = matter.Selected || (ctrl.searchMatterText.length >= 4
                && matter.Nature == ctrl.nature
                && matter.Group.includes(ctrl.group || "")
                && matter.SequenceType.includes(ctrl.sequenceType || "")
                && matter.Text.toUpperCase().includes(ctrl.searchMatterText.toUpperCase())
                && (ctrl.nature != ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter)));
        };

        // checks if genetic sequence is referense sequence 
        // (its  genbank id contains "_")
        ctrl.isRefSeq = matter => matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;

        ctrl.selectAllVisibleMatters = () => {
            ctrl.getVisibleMatters().forEach(matter => {
                if (!matter.Selected && (ctrl.selectedMatters < ctrl.maximumSelectedMatters)) {
                    $(`#matter${matter.Value}`).prop("checked", true);
                    matter.Selected = true;
                    ctrl.selectedMatters++;
                }
            });
        };

        ctrl.unselectAllVisibleMatters = () => {
            ctrl.matters.filter(m => m.Selected).forEach(matter => {
                $(`#matter${matter.Value}`).prop("checked", false);
                matter.Selected = false;
                ctrl.setMatterVisibility(matter);
            });

            ctrl.selectedMatters = 0;
            ctrl.toogleMattersVisibility(false);
        };

        ctrl.toggleMatterSelection = matter => {
            if (matter.Selected) {
                matter.Selected = false;
                ctrl.selectedMatters--;
            } else {
                if (ctrl.maximumSelectedMatters == 1) {
                    ctrl.matters.forEach(m => m.Selected = false);
                    matter.Selected = true;
                    ctrl.selectedMatters = 1;
                } else {
                    if ((ctrl.selectedMatters < ctrl.maximumSelectedMatters)) {
                        matter.Selected = true;
                        ctrl.selectedMatters++;
                    } else {
                        $(`#matter${matter.Value}`).prop("checked", false);
                        matter.Selected = false;
                    }
                }
            }
        };

        ctrl.getVisibleMatters = () => ctrl.matters.filter(m => m.Visible);
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: window.location.origin + "/Partial/_MattersTable",
        controller: ["$scope", "filterFilter", MattersTableController],
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
