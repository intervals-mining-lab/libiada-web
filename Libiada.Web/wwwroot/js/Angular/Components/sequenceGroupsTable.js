function sequenceGroupsTable() {
    "use strict";

    function SequenceGroupsTableController($scope, filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.searchSequenceGroupText = "";

            const selectedSequenceGroups = ctrl.sequenceGroups.filter(sg => sg.Selected);
            ctrl.selectedSequenceGroupsCount = selectedSequenceGroups.length;
            ctrl.visibleSequenceGroups = selectedSequenceGroups;
            ctrl.visibleSequenceGroups.forEach(vsg => vsg.Visible = true);
            ctrl.toogleSequenceGroupsVisibility(false);

            // returning clear selection function to controller for callbacks
            ctrl.setUnselectAllSequenceGroupsFunction({ func: ctrl.unselectAllSequenceGroups });
        };

        ctrl.$onChanges = changes => {
            if (changes.nature && !changes.nature.isFirstChange()) {
                ctrl.toogleSequenceGroupsVisibility(true);
            }
        };

        ctrl.toogleSequenceGroupsVisibility = (resetSelection, ignoreSearch) => {
            if (resetSelection) {
                ctrl.sequenceGroups.forEach(sg => {
                    sg.Selected = false;
                    $(`#sequenceGroup${sg.Value}`).prop("checked", false);
                });
                ctrl.selectedSequenceGroupsCount = 0;
            }

            // Updating mattrs visibility flag
            ctrl.sequenceGroups.forEach(sg => sg.Visible = ignoreSearch ? ctrl.isSequenceGroupVisibleWithoutSearch(sg) : ctrl.isSequenceGroupVisible(sg));
            const sequenceGroupsToHide = ctrl.visibleSequenceGroups.filter(sg => !sg.Visible);
            const newVisibleSequenceGroups = ctrl.sequenceGroups.filter(sg => sg.Visible);
            const sequenceGroupsToShow = newVisibleSequenceGroups.filter(sg => !ctrl.visibleSequenceGroups.some(sg2 => sg2.Value === sg.Value));
            ctrl.visibleSequenceGroups = newVisibleSequenceGroups;

            // removing not visible sequence groups from table 
            $(sequenceGroupsToHide.map(sg => `#sequenceGroupRow${sg.Value}`).join(",")).remove();

            //adding newly visible sequence groups to table
            $("#sequenceGroupsSelectList").append(sequenceGroupsToShow.map(sg =>
                `<tr id="sequenceGroupRow${sg.Value}">
                    <td>
                        <div class="form-check">
                            <input type="checkbox"
                                   class="form-check-input"
                                   name="sequenceGroupIds"
                                   id="sequenceGroup${sg.Value}"
                                   value="${sg.Value}" 
                                   ${sg.Selected ? `checked` : ``} />
                            <label class="form-check-label" for="sequenceGroup${sg.Value}">${sg.Text}</label>
                        </div>
                    </td>
                    <td>${sg.Group}</td>
                    <td>${sg.SequenceType}</td>
                </tr>`).join());

            // binding checkbox state change event
            sequenceGroupsToShow.forEach(sg => $(`#sequenceGroup${sg.Value}`).change(
                () => {
                    ctrl.toggleSequenceGroupSelection(sg);
                    $scope.$apply();
                }
            ));
        };

        ctrl.isSequenceGroupVisibleWithoutSearch = sequenceGroup => sequenceGroup.Selected || ctrl.checkSequenceGroupVisibilitty(sequenceGroup);

        ctrl.isSequenceGroupVisible = sequenceGroup => sequenceGroup.Selected ||
            (ctrl.searchSequenceGroupText.length >= 4
                && ctrl.checkSequenceGroupVisibilitty(sequenceGroup)
                && sequenceGroup.Text.toUpperCase().includes(ctrl.searchSequenceGroupText.toUpperCase()));

        ctrl.checkSequenceGroupVisibilitty = sequenceGroup => sequenceGroup.Nature == ctrl.nature
            && (!ctrl.group || sequenceGroup.Group.includes(ctrl.group.Text))
            && (!ctrl.sequenceType || sequenceGroup.SequenceType.includes(ctrl.sequenceType.Text));

        ctrl.selectAllVisibleSequenceGroups = () => {
            ctrl.getVisibleSequenceGroups().forEach(sg => {
                if (!sg.Selected) {
                    $(`#sequenceGroup${sg.Value}`).prop("checked", true);
                    sg.Selected = true;
                    ctrl.selectedSequenceGroupsCount++;
                }
            });
        };

        ctrl.unselectAllSequenceGroups = () => ctrl.toogleSequenceGroupsVisibility(true);

        ctrl.toggleSequenceGroupSelection = sequenceGroup => {
            if (sequenceGroup.Selected) {
                sequenceGroup.Selected = false;
                ctrl.selectedSequenceGroupsCount--;
            } else {
                sequenceGroup.Selected = true;
                ctrl.selectedSequenceGroupsCount++;
            }
        };
    }

    angular.module("libiada").component("sequenceGroupsTable", {
        templateUrl: `${window.location.origin}/AngularTemplates/_SequenceGroupsTable`,
        controller: ["$scope", "filterFilter", SequenceGroupsTableController],
        bindings: {
            selectedSequenceGroupsCount: "=",
            sequenceGroups: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            setUnselectAllSequenceGroupsFunction: "&"
        }
    });
}

sequenceGroupsTable();
