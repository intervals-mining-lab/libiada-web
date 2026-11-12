import type { Filter } from "viewDataTypes";

interface FiltersComponentController extends ng.IController {
    filters: Filter[];
    newFilter: string;

    $onInit(): void;
    onAddingFilter: () => void;
    onDeletingFilter: (filter: Filter) => void;

    // callback functions passed from the parent
    addFilter: (dataWrapper: { newFilter: string }) => void;
    deleteFilter: (dataWrapper: { filter: Filter; filterIndex: number }) => void;
}

function FiltersController(this: FiltersComponentController): void {
    const ctrl: FiltersComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.filters ??= [];
        ctrl.newFilter ??= "";

        ctrl.addFilter ??= (): void => { };
        ctrl.deleteFilter ??= (): void => { };
    };

    ctrl.onAddingFilter = (): void => {
        if (ctrl.newFilter.length > 0) {
            ctrl.filters.push({ value: ctrl.newFilter });
            ctrl.addFilter({ newFilter: ctrl.newFilter });
            ctrl.newFilter = "";
        }
        // todo: add error message if filter is empty
    };

    ctrl.onDeletingFilter = (filter: Filter): void => {
        const filterIndex: number = ctrl.filters.indexOf(filter);
        ctrl.deleteFilter({ filter, filterIndex });
        ctrl.filters.splice(filterIndex, 1);
    };
}

angular.module("libiada").component("filters", {
    templateUrl: `/AngularTemplates/_Filters`,
    controller: FiltersController,
    bindings: {
        filters: "=?",
        addFilter: "&?",
        deleteFilter: "&?"
    }
});
