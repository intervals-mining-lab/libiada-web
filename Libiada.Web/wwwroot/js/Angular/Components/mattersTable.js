﻿function mattersTable() {
    "use strict";

    function MattersTableController($scope, filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
            ctrl.searchMatterText = "";

            if (ctrl.groupAndTypeRequired) {
                const groupIndex = ctrl.initialGroupIndex ?? 0;
                ctrl.group = ctrl.groups[groupIndex];

                const sequenceTypeIndex = ctrl.initialSequenceTypeIndex ?? 0;
                ctrl.sequenceType = ctrl.sequenceTypes[sequenceTypeIndex];
            }

            if (ctrl.checkboxes) {
                ctrl.mattersInputName = "matterIds";
                ctrl.mattersInputType = "checkbox";
            } else {
                ctrl.mattersInputName = "matterId";
                ctrl.mattersInputType = "radio";
            }

            ctrl.selectedMattersCount = ctrl.matters.filter(m => m.Selected).length;
            ctrl.visibleMatters = [];
            ctrl.toogleMattersVisibility(false);

            // returning clear selection function to controller for callbacks
            ctrl.setUnselectAllMattersFunction({ func: ctrl.unselectAllMatters });

            if (ctrl.displayMultisequenceNumber) {
                ctrl.multisequenceNumberCounter = 1;
                $("#mattersSelectList").on("change", `input[name="${ctrl.mattersInputName}"]`, ctrl.updateMultisequenceNumber);

                // if we are editing existing multisequence there are already selected matters
                if (ctrl.multisequenceNumbers) {
                    ctrl.selectedMattersCount = ctrl.multisequenceNumbers.length;
                    ctrl.multisequenceNumberCounter += ctrl.multisequenceNumbers.length;

                    for (let i = 0; i < ctrl.multisequenceNumbers.length; i++) {
                        const matterId = ctrl.multisequenceNumbers[i].Id;
                        const number = ctrl.multisequenceNumbers[i].MultisequenceNumber;
                        $(`#multisequenceNumberCell${matterId}`).append(
                            `<span>${number}</span>
                             <input type="hidden"
                                    name="multisequenceNumbers"
                                    id="multisequenceNumber${matterId}"
                                    value="${number}"/>`)
                    }
                }
            }
        };

        ctrl.$onChanges = changes => {
            if (changes.nature && !changes.nature.isFirstChange()) {
                if (ctrl.groupAndTypeRequired) {
                    const nature = parseInt(ctrl.nature);
                    ctrl.group = ctrl.groups.filter(g => g.Nature === nature)[0];
                    ctrl.sequenceType = ctrl.sequenceTypes.filter(st => st.Nature === nature)[0];
                }

                ctrl.toogleMattersVisibility(true);
            }
        };

        ctrl.updateMultisequenceNumber = e => {
            const target = e.target;
            const matterId = target.value;
            if (target.checked) {
                $(`#multisequenceNumberCell${matterId}`).append(
                    `<span>${ctrl.multisequenceNumberCounter}</span>
                     <input type="hidden"
                            name="multisequenceNumbers"
                            id="multisequenceNumber${matterId}"
                            value="${ctrl.multisequenceNumberCounter++}"/>`)

            } else {
                const numberToRemove = $(`#multisequenceNumberCell${matterId}`).children()[1].value;

                //decrementing all numbers higher than removed one
                $("[id^=multisequenceNumberCell]:has(input)")
                    .filter((index, el) => +el.children[1].value > numberToRemove)
                    .each((index, el) => {
                        el.children[1].value--;
                        el.children[0].innerHTML = el.children[1].value;
                    });

                $(`#multisequenceNumberCell${matterId}`).empty();
                ctrl.multisequenceNumberCounter--;
            }
        };

        ctrl.toogleMattersVisibility = (resetSelection, ignoreSearch) => {
            if (resetSelection) {
                ctrl.matters.forEach(m => {
                    m.Selected = false;
                    $(`#matter${m.Value}`).prop("checked", false);
                });
                ctrl.selectedMattersCount = 0;
            }

            // Updating mattrs visibility flag
            ctrl.matters.forEach(m => m.Visible = ignoreSearch ? ctrl.isMatterVisibleWithoutSearch(m) : ctrl.isMatterVisible(m));
            const mattersToHide = ctrl.visibleMatters.filter(m => !m.Visible);
            const newVisibleMatters = ctrl.matters.filter(m => m.Visible);
            const mattersToShow = newVisibleMatters.filter(m => !ctrl.visibleMatters.some(m2 => m2.Value === m.Value));
            ctrl.visibleMatters = newVisibleMatters;

            // removing not visible matters from table 
            $(mattersToHide.map(m => `#matterRow${m.Value}`).join(",")).remove();

            //adding newly visible matters to table
            $("#mattersSelectList").append(mattersToShow.map(m =>
                `<tr id="matterRow${m.Value}">
                    ${ctrl.displayMultisequenceNumber ?
                    `<td id="multisequenceNumberCell${m.Value}">
                    </td>` : ``}
                    <td>
                        <div class="form-check">
                            <input type="${ctrl.mattersInputType}"
                                   class="form-check-input"
                                   name="${ctrl.mattersInputName}"
                                   id="matter${m.Value}"
                                   value="${m.Value}" 
                                   ${m.Selected ? `checked` : ``} />
                            <label class="form-check-label" for="matter${m.Value}">${m.Text}</label>
                        </div>
                    </td>
                    <td>${m.Group}</td>
                    <td>${m.SequenceType}</td>
                </tr>`).join());

            // binding checkbox state change event
            mattersToShow.forEach(m => $(`#matter${m.Value}`).change(
                () => {
                    ctrl.toggleMatterSelection(m);
                    $scope.$apply();
                }
            ));
        };

        ctrl.isMatterVisibleWithoutSearch = matter => matter.Selected || ctrl.checkMatterVisibilitty(matter);

        ctrl.isMatterVisible = matter => matter.Selected ||
            (ctrl.searchMatterText.length >= 4
                && ctrl.checkMatterVisibilitty(matter)
                && matter.Text.toUpperCase().includes(ctrl.searchMatterText.toUpperCase()));

        ctrl.checkMatterVisibilitty = matter => matter.Nature == ctrl.nature
            && (!ctrl.group || matter.Group.includes(ctrl.group.Text))
            && (!ctrl.sequenceType || matter.SequenceType.includes(ctrl.sequenceType.Text))
            && (ctrl.nature != ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter));

        // checks if genetic sequence is referense sequence 
        // (its genbank id contains "_")
        ctrl.isRefSeq = matter => matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;

        ctrl.selectAllVisibleMatters = () =>
            ctrl.visibleMatters.forEach(matter => {
                if (!matter.Selected && (ctrl.selectedMattersCount < ctrl.maximumSelectedMatters)) {
                    $(`#matter${matter.Value}`).prop("checked", true);
                    matter.Selected = true;
                    ctrl.selectedMattersCount++;
                }
            });

        ctrl.unselectAllMatters = () => ctrl.toogleMattersVisibility(true);

        ctrl.toggleMatterSelection = matter => {
            if (matter.Selected) {
                matter.Selected = false;
                ctrl.selectedMattersCount--;
            } else {
                if (ctrl.maximumSelectedMatters === 1) {
                    ctrl.matters.forEach(m => m.Selected = false);
                    matter.Selected = true;
                    ctrl.selectedMattersCount = 1;
                } else {
                    if ((ctrl.selectedMattersCount < ctrl.maximumSelectedMatters)) {
                        matter.Selected = true;
                        ctrl.selectedMattersCount++;
                    } else {
                        $(`#matter${matter.Value}`).prop("checked", false);
                        matter.Selected = false;
                    }
                }
            }
        };
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: `${window.location.origin}/AngularTemplates/_MattersTable`,
        controller: ["$scope", "filterFilter", MattersTableController],
        bindings: {
            selectedMattersCount: "=",
            matters: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedMatters: "<",
            displayMultisequenceNumber: "<",
            multisequenceNumbers: "<",
            initialSequenceTypeIndex: "<?",
            initialGroupIndex: "<?",
            groupAndTypeRequired: "<",
            setUnselectAllMattersFunction: "&"
        }
    });
}

mattersTable();
