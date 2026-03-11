function CharacteristicsTableController() {
    const ctrl = this;
    ctrl.characteristicsTableRendered = false;
    ctrl.$onChanges = (changes) => {
        if (changes.characteristicsTableTabSelected) {
            if (ctrl.characteristicsTableTabSelected === true) {
                ctrl.renderResultsTable();
            }
        }
    };
    ctrl.renderResultsTable = async () => {
        if (!ctrl.characteristicsTableRendered) {
            if ($("#calculationResults").length > 0) {
                $("#calculationResults").append(ctrl.characteristics.map((c, i) => `<tr id="resultRow${i}">
                            <td>${i + 1}</td>
                            <td>${c.ResearchObjectName}</td>
                            ${ctrl.sequenceGroups ? `<td>${c.SequenceGroupId}</td>` : ""}
                            ${c.Characteristics.map(c => `<td>${c}</td>`).join()}`).join());
            }
            ctrl.characteristicsTableRendered = true;
        }
    };
}
angular.module("libiada").component("characteristicsTable", {
    templateUrl: `/AngularTemplates/_CharacteristicsTable`,
    controller: CharacteristicsTableController,
    bindings: {
        characteristicNames: "<",
        characteristics: "<",
        characteristicsTableTabSelected: "<",
        sequenceGroups: "<?"
    }
});
export {};
//# sourceMappingURL=characteristicsTable.js.map