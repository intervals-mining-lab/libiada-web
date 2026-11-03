import type { Group, ResearchObject, SequenceType } from "viewDataTypes";

interface MultisequenceNumber {
    Id: number;
    MultisequenceNumber: number;
}

interface ResearchObjectsSearchParams {
    nature: string;
    refSeqOnly: boolean;
    searchQuery?: string;
    group?: string;
    sequenceType?: string;
}

interface ResearchObjectsTableComponentController extends ng.IController {
    selectedResearchObjectsCount: number;
    researchObjects: ResearchObject[];
    nature: string;
    groups: Group[];
    sequenceTypes: SequenceType[];
    maximumSelectedResearchObjects: number;
    displayMultisequenceNumber: boolean;
    multisequenceNumbers?: MultisequenceNumber[];
    initialSequenceTypeIndex?: number;
    initialGroupIndex?: number;
    groupAndTypeRequired: boolean;
    researchObjectsEndpoint: string;
    showRefSeqOnly: boolean;
    checkboxes: boolean;
    searchResearchObjectText: string;
    group?: Group;
    sequenceType?: SequenceType;
    researchObjectsInputName: "researchObjectIds" | "researchObjectId";
    researchObjectsInputType: "checkbox" | "radio";
    researchObjectsProvided: boolean;
    visibleResearchObjects: ResearchObject[];
    multisequenceNumberCounter: number;
    geneticNature: string;

    $onInit(): void;
    $onChanges(changes: ng.IOnChangesObject): void;
    unselectAllResearchObjects: () => Promise<void>;
    updateMultisequenceNumber: (event: Event) => void;
    toggleResearchObjectsVisibility: (resetSelection: boolean, ignoreSearch?: boolean) => Promise<void>;
    FillResearchObjectsSearchParams: () => ResearchObjectsSearchParams;
    isResearchObjectVisibleWithoutSearch: (researchObject: ResearchObject) => boolean;
    isResearchObjectVisible: (researchObject: ResearchObject) => boolean;
    checkResearchObjectVisibility: (researchObject: ResearchObject) => boolean;
    isRefSeq: (researchObject: ResearchObject) => boolean;
    selectAllVisibleResearchObjects: () => void;
    toggleResearchObjectSelection: (researchObject: ResearchObject) => void;

    // callback passed from the parent
    setUnselectAllResearchObjectsFunction: (callbackWrapper: { func: () => Promise<void> }) => void;
}

function ResearchObjectsTableController(
    this: ResearchObjectsTableComponentController,
    $scope: ng.IScope,
    $http: ng.IHttpService
): void {
    const ctrl: ResearchObjectsTableComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.researchObjectsEndpoint ??= "GetAllResearchObjects";
        ctrl.showRefSeqOnly = true;
        ctrl.checkboxes = ctrl.maximumSelectedResearchObjects > 1;
        ctrl.searchResearchObjectText = "";

        if (ctrl.groupAndTypeRequired) {
            const groupIndex: number = ctrl.initialGroupIndex ?? 0;
            ctrl.group = ctrl.groups[groupIndex];

            const sequenceTypeIndex: number = ctrl.initialSequenceTypeIndex ?? 0;
            ctrl.sequenceType = ctrl.sequenceTypes[sequenceTypeIndex];
        }

        if (ctrl.checkboxes) {
            ctrl.researchObjectsInputName = "researchObjectIds";
            ctrl.researchObjectsInputType = "checkbox";
        } else {
            ctrl.researchObjectsInputName = "researchObjectId";
            ctrl.researchObjectsInputType = "radio";
        }

        ctrl.researchObjectsProvided = ctrl.researchObjects !== undefined;
        ctrl.researchObjects ??= [];
        ctrl.selectedResearchObjectsCount = ctrl.researchObjectsProvided ? ctrl.researchObjects.filter(m => m.Selected).length : 0;


        ctrl.visibleResearchObjects = [];
        ctrl.toggleResearchObjectsVisibility(false);

        // returning clear selection function to controller for callbacks
        ctrl.setUnselectAllResearchObjectsFunction({ func: ctrl.unselectAllResearchObjects });

        if (ctrl.displayMultisequenceNumber) {
            ctrl.multisequenceNumberCounter = 1;
            $("#researchObjectsSelectList").on("change",
                `input[name="${ctrl.researchObjectsInputName}"]`,
                ctrl.updateMultisequenceNumber);

            // if we are editing existing multisequence there are already selected research objects
            if (ctrl.multisequenceNumbers) {
                ctrl.selectedResearchObjectsCount = ctrl.multisequenceNumbers.length;
                ctrl.multisequenceNumberCounter += ctrl.multisequenceNumbers.length;

                for (let i = 0; i < ctrl.multisequenceNumbers.length; i++) {
                    const researchObjectId: number = ctrl.multisequenceNumbers[i].Id;
                    const number: number = ctrl.multisequenceNumbers[i].MultisequenceNumber;
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

    ctrl.$onChanges = (changes: ng.IOnChangesObject): void => {
        if (changes["nature"] && !changes["nature"].isFirstChange()) {
            if (ctrl.groupAndTypeRequired) {
                ctrl.group = ctrl.groups.filter(g => g.Nature === ctrl.nature)[0];
                ctrl.sequenceType = ctrl.sequenceTypes.filter(st => st.Nature === ctrl.nature)[0];
            }

            ctrl.toggleResearchObjectsVisibility(true);
        }
    };

    ctrl.updateMultisequenceNumber = (event: Event): void => {
        const target: HTMLInputElement = event.target as HTMLInputElement;
        const researchObjectId: string = target.value;
        if (target.checked) {
            $(`#multisequenceNumberCell${researchObjectId}`).append(
                `<span>${ctrl.multisequenceNumberCounter}</span>
                     <input type="hidden"
                            name="multisequenceNumbers"
                            id="multisequenceNumber${researchObjectId}"
                            value="${ctrl.multisequenceNumberCounter++}"/>`)

        } else {
            const numberInput: HTMLInputElement = $(`#multisequenceNumberCell${researchObjectId}`).children()[1] as HTMLInputElement;
            const numberToRemove: number = Number(numberInput.value);

            //decrementing all numbers higher than removed one
            $("[id^=multisequenceNumberCell]:has(input)")
                .filter((_index, el) => Number((el.children[1] as HTMLInputElement).value) > numberToRemove)
                .each((_index, el) => {
                    const input: HTMLInputElement = el.children[1] as HTMLInputElement;
                    input.value = String(Number(input.value) - 1);
                    el.children[0].innerHTML = input.value;
                });

            $(`#multisequenceNumberCell${researchObjectId}`).empty();
            ctrl.multisequenceNumberCounter--;
        }
    };

    ctrl.toggleResearchObjectsVisibility = async (resetSelection: boolean, ignoreSearch?: boolean): Promise<void> => {

        if (resetSelection) {
            ctrl.visibleResearchObjects.forEach((m: ResearchObject) => {
                m.Selected = false;
                $(`#researchObject${m.Value}`).prop("checked", false);
            });
            ctrl.selectedResearchObjectsCount = 0;
        }
        let researchObjectsToHide: ResearchObject[];
        if (ctrl.researchObjectsProvided) {
            // Updating research objects visibility flag
            ctrl.researchObjects.forEach(m =>
                m.Visible = ignoreSearch ? ctrl.isResearchObjectVisibleWithoutSearch(m) : ctrl.isResearchObjectVisible(m));

            researchObjectsToHide = ctrl.visibleResearchObjects.filter(m => !m.Visible);

        } else {
            if (ignoreSearch || ctrl.searchResearchObjectText.length > 3) {
                const requestParams = ctrl.FillResearchObjectsSearchParams();

                const result: ng.IHttpResponse<ResearchObject[]> = await $http.get<ResearchObject[]>(
                    `/api/ResearchObjectApi/${ctrl.researchObjectsEndpoint}`,
                    { params: requestParams });

                ctrl.researchObjects = result.data;
                ctrl.researchObjects.forEach(m => m.Visible = true);
            } else {
                ctrl.researchObjects = [];
            }

            researchObjectsToHide = ctrl.visibleResearchObjects
                .filter(m => !m.Selected && !ctrl.researchObjects.some(m2 => m2.Value === m.Value));

        }

        const researchObjectsToShow: ResearchObject[] = ctrl.researchObjects
            .filter(m => m.Visible && !ctrl.visibleResearchObjects.some(m2 => m2.Value === m.Value));

        ctrl.visibleResearchObjects = ctrl.visibleResearchObjects
            .filter(m => !researchObjectsToHide.some(m2 => m2.Value === m.Value))
            .concat(researchObjectsToShow);

        // removing not visible research objects from table
        $(researchObjectsToHide.map(m => `#researchObjectRow${m.Value}`).join(",")).remove();

        //adding newly visible research objects to table
        $("#researchObjectsSelectList").append(researchObjectsToShow.map((m: ResearchObject) =>
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
        researchObjectsToShow.forEach((m: ResearchObject) => $(`#researchObject${m.Value}`)
            .change(() => {
                ctrl.toggleResearchObjectSelection(m);
                $scope.$apply();
            }
            ));
    };

    ctrl.FillResearchObjectsSearchParams = (): ResearchObjectsSearchParams => {
        const params: ResearchObjectsSearchParams = {
            nature: ctrl.nature,
            refSeqOnly: ctrl.showRefSeqOnly
        };

        if (ctrl.searchResearchObjectText) params.searchQuery = ctrl.searchResearchObjectText;
        if (ctrl.group) params.group = ctrl.group.Value;
        if (ctrl.sequenceType) params.sequenceType = ctrl.sequenceType.Value;

        return params;
    }

    ctrl.isResearchObjectVisibleWithoutSearch = (researchObject: ResearchObject): boolean => researchObject.Selected || ctrl.checkResearchObjectVisibility(researchObject);

    ctrl.isResearchObjectVisible = (researchObject: ResearchObject): boolean => researchObject.Selected ||
        (ctrl.searchResearchObjectText.length >= 4
            && ctrl.checkResearchObjectVisibility(researchObject)
            && researchObject.Text.toUpperCase().includes(ctrl.searchResearchObjectText.toUpperCase()));

    ctrl.checkResearchObjectVisibility = (researchObject: ResearchObject): boolean => researchObject.Nature == ctrl.nature
        && (!ctrl.group || researchObject.Group.includes(ctrl.group.Text))
        && (!ctrl.sequenceType || researchObject.SequenceType.includes(ctrl.sequenceType.Text))
        && (ctrl.nature !== ctrl.geneticNature || !ctrl.showRefSeqOnly || ctrl.isRefSeq(researchObject));

    // checks if genetic sequence is reference sequence 
    // (its genbank id contains "_")
    ctrl.isRefSeq = (researchObject: ResearchObject): boolean => researchObject.Text.split("|").slice(-1)[0].indexOf("_") !== -1;

    ctrl.selectAllVisibleResearchObjects = (): void =>
        ctrl.visibleResearchObjects.forEach(researchObject => {
            if (!researchObject.Selected && (ctrl.selectedResearchObjectsCount < ctrl.maximumSelectedResearchObjects)) {
                $(`#researchObject${researchObject.Value}`).prop("checked", true);
                researchObject.Selected = true;
                ctrl.selectedResearchObjectsCount++;
            }
        });

    ctrl.unselectAllResearchObjects = (): Promise<void> => ctrl.toggleResearchObjectsVisibility(true);

    ctrl.toggleResearchObjectSelection = (researchObject: ResearchObject): void => {
        if (researchObject.Selected) {
            researchObject.Selected = false;
            ctrl.selectedResearchObjectsCount--;
        } else {
            if (ctrl.maximumSelectedResearchObjects === 1) {
                ctrl.visibleResearchObjects.forEach(m => m.Selected = false);
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
    templateUrl: `/AngularTemplates/_ResearchObjectsTable`,
    controller: ["$scope", "$http", ResearchObjectsTableController],
    bindings: {
        selectedResearchObjectsCount: "=",
        researchObjects: "<?",
        nature: "<",
        groups: "<",
        sequenceTypes: "<",
        maximumSelectedResearchObjects: "<",
        displayMultisequenceNumber: "<",
        multisequenceNumbers: "<?",
        initialSequenceTypeIndex: "<?",
        initialGroupIndex: "<?",
        groupAndTypeRequired: "<",
        setUnselectAllResearchObjectsFunction: "&",
        researchObjectsEndpoint: "@?"
    }
});
