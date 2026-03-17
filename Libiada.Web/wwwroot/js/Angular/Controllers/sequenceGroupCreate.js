import { MapModelFromJson } from "functions";
/**
 * Angular controller class for sequences group creation page
 */
class SequenceGroupCreateHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const sequenceGroupCreate = ($scope) => {
            MapModelFromJson($scope, data);
        };
        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupCreateCtrl", ["$scope", sequenceGroupCreate]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns SequenceGroupsCreate Controller instance
*/
export default function SequenceGroupCreateController(data) {
    return new SequenceGroupCreateHandler(data);
}
//# sourceMappingURL=sequenceGroupCreate.js.map