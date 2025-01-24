function researchObjectsTable() {
    "use strict";

    function ResearchObjectsTableController($scope, filterFilter) {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.showRefSeqOnly = true;
            ctrl.checkboxes = ctrl.maximumSelectedResearchObjects > 1;
            ctrl.searchResearchObjectText = "";

            if (ctrl.groupAndTypeRequired) {
                const groupIndex = ctrl.initialGroupIndex ?? 0;
                ctrl.group = ctrl.groups[groupIndex];

                const sequenceTypeIndex = ctrl.initialSequenceTypeIndex ?? 0;
                ctrl.sequenceType = ctrl.sequenceTypes[sequenceTypeIndex];
            }

            if (ctrl.checkboxes) {
                ctrl.researchObjectsInputName = "researchObjectIds";
                ctrl.researchObjectsInputType = "checkbox";
            } else {
                ctrl.researchObjectsInputName = "researchObjectId";
                ctrl.researchObjectsInputType = "radio";
            }

            ctrl.selectedResearchObjectsCount = ctrl.researchObjects.filter(m => m.Selected).length;
            ctrl.visibleResearchObjects = [];
            ctrl.toogleResearchObjectsVisibility(false);

            // returning clear selection function to controller for callbacks
            ctrl.setUnselectAllResearchObjectsFunction({ func: ctrl.unselectAllResearchObjects });

            if (ctrl.displayMultisequenceNumber) {
                ctrl.multisequenceNumberCounter = 1;
                $("#researchObjectsSelectList").on("change", `input[name="${ctrl.researchObjectsInputName}"]`, ctrl.updateMultisequenceNumber);

                // if we are editing existing multisequence there are already selected research objects
                if (ctrl.multisequenceNumbers) {
                    ctrl.selectedResearchObjectsCount = ctrl.multisequenceNumbers.length;
                    ctrl.multisequenceNumberCounter += ctrl.multisequenceNumbers.length;

                    for (let i = 0; i < ctrl.multisequenceNumbers.length; i++) {
                        const researchObjectId = ctrl.multisequenceNumbers[i].Id;
                        const number = ctrl.multisequenceNumbers[i].MultisequenceNumber;
                        $(`#multisequenceNumberCell${researchObjectId}`).append(
                            `<span>${number}</span>
                             <input type="hidden"
                                    name="multisequenceNumbers"
                                    id="multisequenceNumber${researchObjectId}"
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

                ctrl.toogleResearchObjectsVisibility(true);
            }
        };

        ctrl.updateMultisequenceNumber = e => {
            const target = e.target;
            const researchObjectId = target.value;
            if (target.checked) {
                $(`#multisequenceNumberCell${researchObjectId}`).append(
                    `<span>${ctrl.multisequenceNumberCounter}</span>
                     <input type="hidden"
                            name="multisequenceNumbers"
                            id="multisequenceNumber${researchObjectId}"
                            value="${ctrl.multisequenceNumberCounter++}"/>`)

            } else {
                const numberToRemove = $(`#multisequenceNumberCell${researchObjectId}`).children()[1].value;

                //decrementing all numbers higher than removed one
                $("[id^=multisequenceNumberCell]:has(input)")
                    .filter((index, el) => +el.children[1].value > numberToRemove)
                    .each((index, el) => {
                        el.children[1].value--;
                        el.children[0].innerHTML = el.children[1].value;
                    });

                $(`#multisequenceNumberCell${researchObjectId}`).empty();
                ctrl.multisequenceNumberCounter--;
            }
        };

        ctrl.toogleResearchObjectsVisibility = (resetSelection, ignoreSearch) => {
            if (resetSelection) {
                ctrl.researchObjects.forEach(m => {
                    m.Selected = false;
                    $(`#researchObject{m.Value}`).prop("checked", false);
                });
                ctrl.selectedResearchObjectsCount = 0;
            }

            // Updating research objects visibility flag
            ctrl.researchObjects.forEach(m => m.Visible = ignoreSearch ? ctrl.isResearchObjectVisibleWithoutSearch(m) : ctrl.isResearchObjectVisible(m));
            const researchObjectsToHide = ctrl.visibleResearchObjects.filter(m => !m.Visible);
            const newVisibleResearchObjects = ctrl.researchObjects.filter(m => m.Visible);
            const researchObjectsToShow = newVisibleResearchObjects.filter(m => !ctrl.visibleResearchObjects.some(m2 => m2.Value === m.Value));
            ctrl.visibleResearchObjects = newVisibleResearchObjects;

            // removing not visible research objects from table
            $(researchObjectsToHide.map(m => `#researchObjectRow${m.Value}`).join(",")).remove();

            //adding newly visible research objects to table
            $("#researchObjectsSelectList").append(researchObjectsToShow.map(m =>
                `<tr id="researchObjectRow${m.Value}">
                    ${ctrl.displayMultisequenceNumber ?
                    `<td id="multisequenceNumberCell${m.Value}">
                    </td>` : ``}
                    <td>
                        <div class="form-check">
                            <input type="${ctrl.researchObjectsInputType}"
                                   class="form-check-input"
                                   name="${ctrl.researchObjectsInputName}"
                                   id="researchObject${m.Value}"
                                   value="${m.Value}" 
                                   ${m.Selected ? `checked` : ``} />
                            <label class="form-check-label" for="researchObject${m.Value}">${m.Text}</label>
                        </div>
                    </td>
                    <td>${m.Group}</td>
                    <td>${m.SequenceType}</td>
                </tr>`).join());

            // binding checkbox state change event
            researchObjectsToShow.forEach(m => $(`#researchObject${m.Value}`).change(
                () => {
                    ctrl.toggleResearchObjectSelection(m);
                    $scope.$apply();
                }
            ));
        };

        ctrl.isResearchObjectVisibleWithoutSearch = researchObject => researchObject.Selected || ctrl.checkResearchObjectVisibilitty(researchObject);

        ctrl.isResearchObjectVisible = researchObject => researchObject.Selected ||
            (ctrl.searchResearchObjectText.length >= 4
                && ctrl.checkResearchObjectVisibilitty(researchObject)
                && researchObject.Text.toUpperCase().includes(ctrl.searchResearchObjectText.toUpperCase()));

        ctrl.checkResearchObjectVisibilitty = researchObject => researchObject.Nature == ctrl.nature
            && (!ctrl.group || researchObject.Group.includes(ctrl.group.Text))
            && (!ctrl.sequenceType || researchObject.SequenceType.includes(ctrl.sequenceType.Text))
            && (ctrl.nature != ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(researchObject));

        // checks if genetic sequence is referense sequence 
        // (its genbank id contains "_")
        ctrl.isRefSeq = researchObject => researchObject.Text.split("|").slice(-1)[0].indexOf("_") !== -1;

        ctrl.selectAllVisibleResearchObjects = () =>
            ctrl.visibleResearchObjects.forEach(researchObject => {
                if (!researchObject.Selected && (ctrl.selectedResearchObjectsCount < ctrl.maximumSelectedResearchObjects)) {
                    $(`#researchObject${researchObject.Value}`).prop("checked", true);
                    researchObject.Selected = true;
                    ctrl.selectedResearchObjectsCount++;
                }
            });

        ctrl.unselectAllResearchObjects = () => ctrl.toogleResearchObjectsVisibility(true);

        ctrl.toggleResearchObjectSelection = researchObject => {
            if (researchObject.Selected) {
                researchObject.Selected = false;
                ctrl.selectedResearchObjectsCount--;
            } else {
                if (ctrl.maximumSelectedResearchObjects === 1) {
                    ctrl.researchObjects.forEach(m => m.Selected = false);
                    researchObject.Selected = true;
                    ctrl.selectedResearchObjectsCount = 1;
                } else {
                    if ((ctrl.selectedResearchObjectsCount < ctrl.maximumSelectedResearchObjects)) {
                        researchObject.Selected = true;
                        ctrl.selectedResearchObjectsCount++;
                    } else {
                        $(`#researchObject${researchObject.Value}`).prop("checked", false);
                        researchObject.Selected = false;
                    }
                }
            }
        };
    }

    angular.module("libiada").component("researchObjectsTable", {
        templateUrl: `${window.location.origin}/AngularTemplates/_ResearchObjectsTable`,
        controller: ["$scope", "filterFilter", ResearchObjectsTableController],
        bindings: {
            selectedResearchObjectsCount: "=",
            researchObjects: "<",
            nature: "<",
            groups: "<",
            sequenceTypes: "<",
            maximumSelectedResearchObjects: "<",
            displayMultisequenceNumber: "<",
            multisequenceNumbers: "<",
            initialSequenceTypeIndex: "<?",
            initialGroupIndex: "<?",
            groupAndTypeRequired: "<",
            setUnselectAllResearchObjectsFunction: "&"
        }
    });
}

researchObjectsTable();
