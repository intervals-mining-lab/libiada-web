"use strict";
/// <reference types="angular" />
/**
* Controller for sequence groups
*/
class SequenceGroupsHandler {
    /**
    * Creates an instance of the sequence groups controller
    * @param data Data to initialize the controller
    */
    constructor(data) {
        this.data = data;
        this.ngOnInit();
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit() {
        "use strict";
        const sequenceGroups = ($scope) => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupsCtrl", ["$scope", sequenceGroups]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns Sequence Groups Controller instance
*/
function SequenceGroupsController(data) {
    return new SequenceGroupsHandler(data);
}
//# sourceMappingURL=sequenceGroups.js.map