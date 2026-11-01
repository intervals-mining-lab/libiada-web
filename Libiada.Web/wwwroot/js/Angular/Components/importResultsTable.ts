import type { SequenceImportResult } from "viewDataTypes";

type Status = "Success" | "Exists" | "Error" | "";

interface ImportResultsTableComponentController extends ng.IController {
    result: SequenceImportResult[];
    hasGroupAndSequenceType: boolean;

    calculateStatusClass: (status: Status) => string;
}

function ImportResultsTableController(this: ImportResultsTableComponentController) {
    const ctrl: ImportResultsTableComponentController = this;

    ctrl.calculateStatusClass = (status: Status): string =>
        status === "Success" ? "table-success" :
            status === "Exists" ? "table-info" :
                status === "Error" ? "table-danger" : "";
}

angular.module("libiada").component("importResultsTable", {
    templateUrl: `/AngularTemplates/_ImportResultsTable`,
    controller: ImportResultsTableController,
    bindings: {
        result: "<",
        hasGroupAndSequenceType: "@"
    }
});
