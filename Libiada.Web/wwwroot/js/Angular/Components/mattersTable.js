function mattersTable() {
    "use strict";

    function MattersTableController($scope, filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedMatters > 1;
            ctrl.searchMatterText = "";

            if (ctrl.groupAndTypeRequired) {
                let groupIndex = ctrl.initialGroupIndex || 0;
                ctrl.group = ctrl.groups[groupIndex];
                let sequenceTypeIndex = ctrl.initialSequenceTypeIndex || 0;
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

            ctrl.toogleMattersVisibility(false);
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
            if ((changes.nature && !changes.nature.isFirstChange())) {
                if (ctrl.groupAndTypeRequired) {
                    ctrl.group = ctrl.groups.filter(g => g.Nature === +ctrl.nature)[0];
                    ctrl.sequenceType = ctrl.sequenceTypes.filter(st => st.Nature === +ctrl.nature)[0];
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
        }

        ctrl.toogleMattersVisibility = (resetSelection, ignoreSearch) => {
            if (resetSelection) {
                ctrl.matters.forEach(m => {
                    m.Selected = false;
                    $(`#matter${m.Value}`).prop("checked", false);
                });
                ctrl.selectedMattersCount = 0;
            }

            const oldVisibleMatters = ctrl.getVisibleMatters();
            ctrl.matters.forEach(m => m.Visible = ignoreSearch ? ctrl.isMatterVisibleWithoutSearch(m) : ctrl.isMatterVisible(m));
            const visibleMatters = ctrl.getVisibleMatters();

            const mattersToHide = oldVisibleMatters.filter(m => !visibleMatters.includes(m));
            const mattersToShow = visibleMatters.filter(m => !oldVisibleMatters.includes(m));

            $(mattersToHide.map(m => `#matterRow${m.Value}`).join(",")).remove();

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
            && matter.Group.includes(ctrl.group.Text || "")
            && matter.SequenceType.includes(ctrl.sequenceType.Text || "")
            && (ctrl.nature != ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(matter));

        // checks if genetic sequence is referense sequence 
        // (its genbank id contains "_")
        ctrl.isRefSeq = matter => matter.Text.split("|").slice(-1)[0].indexOf("_") !== -1;

        ctrl.selectAllVisibleMatters = () => {
            ctrl.getVisibleMatters().forEach(matter => {
                if (!matter.Selected && (ctrl.selectedMattersCount < ctrl.maximumSelectedMatters)) {
                    $(`#matter${matter.Value}`).prop("checked", true);
                    matter.Selected = true;
                    ctrl.selectedMattersCount++;
                }
            });
        };

        ctrl.unselectAllVisibleMatters = () => {
            ctrl.toogleMattersVisibility(true);
        };

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

        ctrl.getVisibleMatters = () => ctrl.matters.filter(m => m.Visible);
    }

    angular.module("libiada").component("mattersTable", {
        templateUrl: window.location.origin + "/AngularTemplates/_MattersTable",
        controller: ["$scope", "filterFilter", MattersTableController],
        bindings: {
            matters: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedMatters: "<",
            selectedMattersCount: "=",
            displayMultisequenceNumber: "<",
            multisequenceNumbers: "<",
            groupAndTypeRequired: "<",
            initialSequenceTypeIndex: "<",
            initialGroupIndex: "<"
        }
    });
}

mattersTable();
