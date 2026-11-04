import type { Group, SequenceGroup, SequenceType } from "viewDataTypes";

interface SequenceGroupsTableComponentController extends ng.IController {
    selectedSequenceGroupsCount: number;
    sequenceGroups: SequenceGroup[];
    nature: string;
    groups: Group[];
    sequenceTypes: SequenceType[];
    searchSequenceGroupText: string;
    visibleSequenceGroups: SequenceGroup[];
    group?: Group;
    sequenceType?: SequenceType;

    $onInit(): void;
    $onChanges(changes: ng.IOnChangesObject): void;
    toggleSequenceGroupsVisibility: (resetSelection: boolean, ignoreSearch?: boolean) => void;
    isSequenceGroupVisibleWithoutSearch: (sequenceGroup: SequenceGroup) => boolean;
    isSequenceGroupVisible: (sequenceGroup: SequenceGroup) => boolean;
    checkSequenceGroupVisibility: (sequenceGroup: SequenceGroup) => boolean;
    selectAllVisibleSequenceGroups: () => void;
    unselectAllSequenceGroups: () => void;
    toggleSequenceGroupSelection: (sequenceGroup: SequenceGroup) => void;

    // callback passed from the parent
    setUnselectAllSequenceGroupsFunction: (callbackWrapper: { func: () => void }) => void;
}

function SequenceGroupsTableController(this: SequenceGroupsTableComponentController, $scope: ng.IScope): void {
    const ctrl: SequenceGroupsTableComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.searchSequenceGroupText = "";

        const selectedSequenceGroups = ctrl.sequenceGroups.filter(sg => sg.Selected);
        ctrl.selectedSequenceGroupsCount = selectedSequenceGroups.length;
        ctrl.visibleSequenceGroups = selectedSequenceGroups;
        ctrl.visibleSequenceGroups.forEach(vsg => vsg.Visible = true);
        ctrl.toggleSequenceGroupsVisibility(false);

        // returning clearing selection function to controller for callbacks
        ctrl.setUnselectAllSequenceGroupsFunction({ func: ctrl.unselectAllSequenceGroups });
    };

    ctrl.$onChanges = (changes: ng.IOnChangesObject): void => {
        if (changes["nature"] && !changes["nature"].isFirstChange()) {
            ctrl.toggleSequenceGroupsVisibility(true);
        }
    };

    ctrl.toggleSequenceGroupsVisibility = (resetSelection: boolean, ignoreSearch?: boolean): void => {
        if (resetSelection) {
            ctrl.sequenceGroups.forEach(sg => {
                sg.Selected = false;
                $(`#sequenceGroup${sg.Value}`).prop("checked", false);
            });
            ctrl.selectedSequenceGroupsCount = 0;
        }

        // Updating sequence groups visibility flag
        ctrl.sequenceGroups.forEach(sg => 
            sg.Visible = ignoreSearch ? ctrl.isSequenceGroupVisibleWithoutSearch(sg) : ctrl.isSequenceGroupVisible(sg));
        const sequenceGroupsToHide: SequenceGroup[] = ctrl.visibleSequenceGroups.filter(sg => !sg.Visible);
        const newVisibleSequenceGroups: SequenceGroup[] = ctrl.sequenceGroups.filter(sg => sg.Visible);
        const sequenceGroupsToShow: SequenceGroup[] = newVisibleSequenceGroups
            .filter(sg => !ctrl.visibleSequenceGroups.some(sg2 => sg2.Value === sg.Value));
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

    ctrl.isSequenceGroupVisibleWithoutSearch = (sequenceGroup: SequenceGroup): boolean => 
        sequenceGroup.Selected || ctrl.checkSequenceGroupVisibility(sequenceGroup);

    ctrl.isSequenceGroupVisible = (sequenceGroup: SequenceGroup): boolean => sequenceGroup.Selected ||
        (ctrl.searchSequenceGroupText.length >= 4
            && ctrl.checkSequenceGroupVisibility(sequenceGroup)
            && sequenceGroup.Text.toUpperCase().includes(ctrl.searchSequenceGroupText.toUpperCase()));

    ctrl.checkSequenceGroupVisibility = (sequenceGroup: SequenceGroup): boolean => sequenceGroup.Nature == ctrl.nature
        && (!ctrl.group || sequenceGroup.Group.includes(ctrl.group.Text))
        && (!ctrl.sequenceType || sequenceGroup.SequenceType.includes(ctrl.sequenceType.Text));

    ctrl.selectAllVisibleSequenceGroups = (): void => {
        ctrl.visibleSequenceGroups.forEach(sg => {
            if (!sg.Selected) {
                $(`#sequenceGroup${sg.Value}`).prop("checked", true);
                sg.Selected = true;
                ctrl.selectedSequenceGroupsCount++;
            }
        });
    };

    ctrl.unselectAllSequenceGroups = (): void => ctrl.toggleSequenceGroupsVisibility(true);

    ctrl.toggleSequenceGroupSelection = (sequenceGroup: SequenceGroup): void => {
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
    templateUrl: `/AngularTemplates/_SequenceGroupsTable`,
    controller: ["$scope", SequenceGroupsTableController],
    bindings: {
        selectedSequenceGroupsCount: "=",
        sequenceGroups: "<",
        nature: "<",
        groups: "<",
        sequenceTypes: "<",
        setUnselectAllSequenceGroupsFunction: "&"
    }
});
