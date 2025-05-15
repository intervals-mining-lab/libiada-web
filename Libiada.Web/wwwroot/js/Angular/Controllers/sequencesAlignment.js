/// <reference types="angular" />
/**
* Sequence alignment controller
*/
class SequencesAlignmentHandler {
    /**
    * Creates an instance of the sequence alignment controller
    * @param data Data for initializing the controller
    */
    constructor(data) {
        this.data = data;
        this.initializeController();
    }
    /**
    * Initializes the Angular controller
    */
    initializeController() {
        "use strict";
        const sequencesAlignment = ($scope) => {
            // Initialize the scope with data from the parameter
            MapModelFromJson($scope, this.data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("SequencesAlignmentCtrl", ["$scope", sequencesAlignment]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data for initializing the controller
* @returns An instance of the sequence alignment controller
*/
function SequencesAlignmentController(data) {
    return new SequencesAlignmentHandler(data);
}
//# sourceMappingURL=sequencesAlignment.js.map