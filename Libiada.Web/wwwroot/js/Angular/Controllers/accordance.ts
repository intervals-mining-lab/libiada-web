/// <reference types="angular" />

/**
 * Interface for accordion data
 */
interface IAccordanceData {
}
/**
 * Interface for controller scope
 */
interface IAccordanceScope extends ng.IScope {
}

/**
 * Controller for accordance functionality
 */
class AccordanceHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data: IAccordanceData) {
        this.ngOnInit(data);
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(data: IAccordanceData): void {
        const accordance = ($scope: IAccordanceScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);
        };

        // Register controller in Angular module
        angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance handler
 */
function AccordanceController(data: IAccordanceData): AccordanceHandler {
    return new AccordanceHandler(data);
}
