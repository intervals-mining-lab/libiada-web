function filters() {
    "use strict";

    function FiltersController() {
        let ctrl = this;

        ctrl.$onInit = () => {
            ctrl.filters ??= [];
        };

        ctrl.onAddingFilter = () => {
            if (ctrl.newFilter.length > 0) {
                ctrl.filters.push({ value: ctrl.newFilter });
                ctrl.addFilter({ newFilter: ctrl.newFilter });
                ctrl.newFilter = "";
            }
            // todo: add error message if filter is empty
        };

        ctrl.onDeletingFilter = (filter) => {
            const filterIndex = ctrl.filters.indexOf(filter);
            ctrl.deleteFilter({ filter: filter, filterIndex: filterIndex });
            ctrl.filters.splice(filterIndex, 1);
        }
    }

    angular.module("libiada").component("filters", {
        templateUrl: `${window.location.origin}/AngularTemplates/_Filters`,
        controller: FiltersController,
        bindings: {
            filters: "=?",
            addFilter: "&",
            deleteFilter: "&"
        }
    });
}

filters();
