function ImportResultsTableController() {
    const ctrl = this;
    ctrl.calculateStatusClass = (status) => {
        return status === "Success" ? "table-success"
            : status === "Exists" ? "table-info"
                : status === "Error" ? "table-danger" : "";
    };
}
angular.module("libiada").component("importResultsTable", {
    templateUrl: `/AngularTemplates/_ImportResultsTable`,
    controller: ImportResultsTableController,
    bindings: {
        result: "<",
        hasGroupAndSequenceType: "@"
    }
});
export {};
//# sourceMappingURL=importResultsTable.js.map