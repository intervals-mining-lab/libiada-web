"use strict";
/// <reference types="angular" />
/**
* Controller for sequence order distribution
*/
class SequencesOrderDistributionHandler {
    /**
    * Creates an instance of the order distribution controller
    * @param data Data to initialize the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit(data) {
        const sequencesOrderDistribution = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns Order distribution controller instance
*/
function SequencesOrderDistributionController(data) {
    return new SequencesOrderDistributionHandler(data);
}
//# sourceMappingURL=sequencesOrderDistribution.js.map