import type { ImportResult } from "viewDataTypes";

interface ImportResultsTableComponentController extends ng.IController {
    result: ImportResult[];
    hasGroupAndSequenceType: boolean;
    calculateStatusClass: (status: string) => string;
}

function ImportResultsTableController(this: ImportResultsTableComponentController) {
    const ctrl = this;

    ctrl.hasGroupColumn = false;
    ctrl.hasSequenceTypeColumn = false;

    ctrl.calculateStatusClass = (status: string): string => {
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
