"use strict";
/// <reference types="angular" />
/**
 * Controller for accordance functionality
 */
class AccordanceHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        const accordance = ($scope, filterFilter) => {
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
function AccordanceController(data) {
    return new AccordanceHandler(data);
}
//# sourceMappingURL=accordance.js.map